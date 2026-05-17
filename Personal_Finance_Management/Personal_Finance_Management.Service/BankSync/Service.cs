using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Entity;
using Personal_Finance_Management.Service.Base;
using Personal_Finance_Management.Service.Casso;
using Personal_Finance_Management.Service.Validations;
using FinancialAccountEntity = Personal_Finance_Management.Repository.Entity.FinancialAccount;

namespace Personal_Finance_Management.Service.BankSync;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IConfiguration _configuration;
    private readonly ICassoClient _cassoClient;
    private readonly ICassoTokenProtector _tokenProtector;

    public Service(
        AppDbContext dbContext,
        IHttpContextAccessor httpContext,
        IConfiguration configuration,
        ICassoClient cassoClient,
        ICassoTokenProtector tokenProtector)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
        _configuration = configuration;
        _cassoClient = cassoClient;
        _tokenProtector = tokenProtector;
    }

    public async Task<Response.CassoTransactionsResponse> SyncLinkedAccount(
        Guid financialAccountId,
        Request.SyncLinkedAccountRequest request)
    {
        var userId = ServiceClaimHelper.GetRequiredUserId(_httpContext);
        return await SyncLinkedAccountForUser(financialAccountId, userId, request);
    }

    public async Task<Response.CassoTransactionsResponse> SyncLinkedAccountForUser(
        Guid financialAccountId,
        Guid userId,
        Request.SyncLinkedAccountRequest request)
    {
        ValidateSyncRequest(request);

        var financialAccount = await _dbContext.FinancialAccounts
            .FirstOrDefaultAsync(x => x.Id == financialAccountId && x.UserId == userId && x.IsActive);
        if (financialAccount == null)
        {
            throw AppValidationException.NotFound("Financial account not found.", "financialAccountId", "FINANCIAL_ACCOUNT_NOT_FOUND");
        }

        if (!string.Equals(financialAccount.ConnectionMode, "LinkedApi", StringComparison.OrdinalIgnoreCase))
        {
            throw AppValidationException.BadRequest("Only linked bank account can sync Casso transactions.", "financialAccountId", "CASSO_SYNC_LINKED_ACCOUNT_REQUIRED");
        }

        var accessToken = ResolveAccessToken(financialAccount);
        try
        {
            if (request.triggerProviderSync && !string.IsNullOrWhiteSpace(financialAccount.ExternalAccountRef))
            {
                await _cassoClient.TriggerSyncAsync(accessToken, financialAccount.ExternalAccountRef);
            }

            IReadOnlyList<CassoTransactionRecord> records;
            if (!string.IsNullOrWhiteSpace(financialAccount.ExternalAccountId)
                && financialAccount.ExternalAccountId != financialAccount.ExternalAccountRef)
            {
                records = await _cassoClient.GetAccountTransactionsAsync(
                    accessToken,
                    financialAccount.ExternalAccountId,
                    request.fromDate,
                    request.toDate,
                    request.page,
                    request.pageSize,
                    request.sort);
            }
            else
            {
                records = await _cassoClient.GetTransactionsAsync(
                    accessToken,
                    request.fromDate,
                    request.toDate,
                    request.page,
                    request.pageSize,
                    request.sort);
            }

            return await UpsertRecordsForAccount(financialAccount, records.Select(x => x.payload).ToList(), "Casso transactions synced.");
        }
        catch (AppValidationException)
        {
            financialAccount.SyncStatus = "Error";
            financialAccount.LastSyncError = "Casso sync failed.";
            financialAccount.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();
            throw;
        }
    }

    public async Task<Response.CassoTransactionsResponse> ProcessCassoWebhook(
        Request.CassoWebhookRequest request,
        string? secureToken,
        string? cassoSignature)
    {
        if (request == null)
        {
            throw AppValidationException.BadRequest("Request body is required.", "body", "REQUIRED");
        }

        ValidateWebhookSecurity(secureToken, cassoSignature);

        if (request.error != 0)
        {
            return new Response.CassoTransactionsResponse
            {
                receivedCount = 0,
                createdCount = 0,
                skippedCount = 0,
                message = "Casso webhook ignored because error is not zero."
            };
        }

        var records = ExtractWebhookRecords(request.data);
        var createdCount = 0;
        var skippedCount = 0;
        await using var databaseTransaction = await _dbContext.Database.BeginTransactionAsync();

        foreach (var record in records)
        {
            var parsed = ParseTransaction(record);
            if (parsed == null || string.IsNullOrWhiteSpace(parsed.AccountRef))
            {
                skippedCount++;
                continue;
            }

            var matchedAccounts = await _dbContext.FinancialAccounts
                .Where(x => x.ProviderCode == "casso"
                            && x.ConnectionMode == "LinkedApi"
                            && x.IsActive
                            && (x.ExternalAccountRef == parsed.AccountRef
                                || x.ExternalAccountId == parsed.AccountRef
                                || x.MaskedAccountNumber == parsed.AccountRef))
                .ToListAsync();

            if (matchedAccounts.Count > 1)
            {
                throw AppValidationException.Conflict("Multiple linked financial accounts match Casso account.", "accountNumber", "CASSO_ACCOUNT_CONFLICT");
            }

            if (matchedAccounts.Count == 0)
            {
                skippedCount++;
                continue;
            }

            var created = await AddTransactionIfMissing(matchedAccounts[0], parsed);
            if (created)
            {
                createdCount++;
            }
            else
            {
                skippedCount++;
            }
        }

        await _dbContext.SaveChangesAsync();
        await databaseTransaction.CommitAsync();

        return new Response.CassoTransactionsResponse
        {
            receivedCount = records.Count,
            createdCount = createdCount,
            skippedCount = skippedCount,
            message = "Casso webhook processed."
        };
    }

    private async Task<Response.CassoTransactionsResponse> UpsertRecordsForAccount(
        FinancialAccountEntity financialAccount,
        IReadOnlyList<JsonElement> records,
        string message)
    {
        var createdCount = 0;
        var skippedCount = 0;
        await using var databaseTransaction = await _dbContext.Database.BeginTransactionAsync();

        foreach (var record in records)
        {
            var parsed = ParseTransaction(record);
            if (parsed == null)
            {
                skippedCount++;
                continue;
            }

            if (!string.IsNullOrWhiteSpace(parsed.AccountRef)
                && !string.IsNullOrWhiteSpace(financialAccount.ExternalAccountRef)
                && parsed.AccountRef != financialAccount.ExternalAccountRef
                && parsed.AccountRef != financialAccount.ExternalAccountId)
            {
                skippedCount++;
                continue;
            }

            var created = await AddTransactionIfMissing(financialAccount, parsed);
            if (created)
            {
                createdCount++;
            }
            else
            {
                skippedCount++;
            }
        }

        financialAccount.SyncStatus = "Synced";
        financialAccount.LastSyncedAt = DateTimeOffset.UtcNow;
        financialAccount.LastSyncError = null;
        financialAccount.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
        await databaseTransaction.CommitAsync();

        return new Response.CassoTransactionsResponse
        {
            receivedCount = records.Count,
            createdCount = createdCount,
            skippedCount = skippedCount,
            message = message
        };
    }

    private async Task<bool> AddTransactionIfMissing(FinancialAccountEntity financialAccount, ParsedCassoTransaction parsed)
    {
        var existedTransaction = await _dbContext.Transactions.AnyAsync(x =>
            x.FinancialAccountId == financialAccount.Id
            && x.ExternalTransactionId == parsed.ExternalTransactionId
            && !x.IsDeleted);
        if (existedTransaction)
        {
            return false;
        }

        _dbContext.Transactions.Add(new Repository.Entity.Transaction
        {
            Id = Guid.NewGuid(),
            UserId = financialAccount.UserId,
            FinancialAccountId = financialAccount.Id,
            CategoryId = null,
            FromJarId = null,
            ToJarId = null,
            Type = parsed.Amount > 0 ? "Income" : "Expense",
            TransactionsAmount = Math.Abs(parsed.Amount),
            Note = parsed.Description,
            RawDescription = parsed.Description,
            TransactionDate = parsed.TransactionDate,
            SourceType = "Imported",
            ExternalTransactionId = parsed.ExternalTransactionId,
            RawPayloadJson = parsed.RawJson,
            PostedAt = DateTimeOffset.UtcNow,
            ImportJobId = null,
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        if (parsed.RunningBalance.HasValue)
        {
            financialAccount.CurrentBalance = parsed.RunningBalance.Value;
        }
        else if (parsed.Amount > 0)
        {
            financialAccount.CurrentBalance += Math.Abs(parsed.Amount);
        }
        else
        {
            financialAccount.CurrentBalance -= Math.Abs(parsed.Amount);
        }

        financialAccount.SyncStatus = "Synced";
        financialAccount.LastSyncedAt = DateTimeOffset.UtcNow;
        financialAccount.LastSyncError = null;
        financialAccount.LastSyncCursor = parsed.ExternalTransactionId;
        financialAccount.UpdatedAt = DateTimeOffset.UtcNow;
        return true;
    }

    private string? ResolveAccessToken(FinancialAccountEntity financialAccount)
    {
        if (string.IsNullOrWhiteSpace(financialAccount.AccessTokenRef))
        {
            return null;
        }

        if (!financialAccount.AccessTokenRef.StartsWith("v1:", StringComparison.Ordinal))
        {
            return null;
        }

        var token = _tokenProtector.Unprotect(financialAccount.AccessTokenRef);
        return token.accessToken;
    }

    private void ValidateWebhookSecurity(string? secureToken, string? cassoSignature)
    {
        var configuredSecureToken = _configuration["Casso:WebhookSecureToken"]
                                    ?? _configuration["Casso:SecureToken"]
                                    ?? _configuration["CasooOptions:WebhookSecureToken"]
                                    ?? _configuration["CasooOptions:SecureToken"];
        if (string.IsNullOrWhiteSpace(configuredSecureToken))
        {
            throw AppValidationException.BadRequest("Casso webhook secure token is not configured.", "secure-token", "CASSO_WEBHOOK_UNAUTHORIZED");
        }

        if (secureToken?.Trim() != configuredSecureToken.Trim())
        {
            throw AppValidationException.BadRequest("Invalid Casso secure token.", "secure-token", "CASSO_WEBHOOK_UNAUTHORIZED");
        }
    }

    private static List<JsonElement> ExtractWebhookRecords(JsonElement data)
    {
        var records = new List<JsonElement>();
        if (data.ValueKind == JsonValueKind.Object)
        {
            records.Add(data.Clone());
        }
        else if (data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in data.EnumerateArray())
            {
                records.Add(item.Clone());
            }
        }
        else
        {
            throw AppValidationException.BadRequest("Casso webhook data is invalid.", "data", "CASSO_WEBHOOK_INVALID");
        }

        return records;
    }

    private static ParsedCassoTransaction? ParseTransaction(JsonElement item)
    {
        var amount = ReadDecimal(item, "amount");
        if (!amount.HasValue || amount.Value == 0)
        {
            return null;
        }

        var externalTransactionId = ReadFlexibleString(item, "reference")
                                    ?? ReadFlexibleString(item, "tid")
                                    ?? ReadFlexibleString(item, "id")
                                    ?? ReadFlexibleString(item, "privateId");
        if (string.IsNullOrWhiteSpace(externalTransactionId))
        {
            return null;
        }

        var transactionDate = DateTimeOffset.UtcNow;
        var transactionDateText = ReadFlexibleString(item, "transactionDateTime")
                                  ?? ReadFlexibleString(item, "when")
                                  ?? ReadFlexibleString(item, "transactionDate");
        if (!string.IsNullOrWhiteSpace(transactionDateText)
            && DateTimeOffset.TryParse(transactionDateText, out var parsedDate))
        {
            transactionDate = parsedDate.ToUniversalTime();
        }

        return new ParsedCassoTransaction
        {
            Amount = amount.Value,
            ExternalTransactionId = externalTransactionId,
            AccountRef = ReadFlexibleString(item, "accountNumber")
                         ?? ReadFlexibleString(item, "subAccId")
                         ?? ReadFlexibleString(item, "bank_sub_acc_id")
                         ?? ReadFlexibleString(item, "bankSubAccId"),
            TransactionDate = transactionDate,
            Description = ReadFlexibleString(item, "description"),
            RunningBalance = ReadDecimal(item, "runningBalance")
                             ?? ReadDecimal(item, "cusum_balance"),
            RawJson = item.GetRawText()
        };
    }

    private static void ValidateSyncRequest(Request.SyncLinkedAccountRequest request)
    {
        if (request == null)
        {
            throw AppValidationException.BadRequest("Request body is required.", "body", "REQUIRED");
        }

        if (request.page <= 0)
        {
            throw AppValidationException.BadRequest("Page must be greater than zero.", "page", "CASSO_SYNC_INVALID");
        }

        if (request.pageSize <= 0 || request.pageSize > 100)
        {
            throw AppValidationException.BadRequest("Page size must be between 1 and 100.", "pageSize", "CASSO_SYNC_INVALID");
        }
    }

    private static string? ReadFlexibleString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null
        };
    }

    private static decimal? ReadDecimal(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDecimal()
            : null;
    }

    private class ParsedCassoTransaction
    {
        public required decimal Amount { get; set; }
        public required string ExternalTransactionId { get; set; }
        public string? AccountRef { get; set; }
        public required DateTimeOffset TransactionDate { get; set; }
        public string? Description { get; set; }
        public decimal? RunningBalance { get; set; }
        public required string RawJson { get; set; }
    }
}
