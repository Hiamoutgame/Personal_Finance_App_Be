using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Service.Base;
using Personal_Finance_Management.Service.Common.Constants;
using Personal_Finance_Management.Service.Common.Enums;
using Personal_Finance_Management.Service.Sepay;
using Personal_Finance_Management.Service.Validations;
using FinancialAccountEntity = Personal_Finance_Management.Repository.Entity.FinancialAccount;

namespace Personal_Finance_Management.Service.BankSync;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IConfiguration _configuration;
    private readonly ISepayClient _sepayClient;
    private readonly ISepayTokenProtector _tokenProtector;

    public Service(
        AppDbContext dbContext,
        IHttpContextAccessor httpContext,
        IConfiguration configuration,
        ISepayClient sepayClient,
        ISepayTokenProtector tokenProtector)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
        _configuration = configuration;
        _sepayClient = sepayClient;
        _tokenProtector = tokenProtector;
    }

    public async Task<Response.SepayTransactionsResponse> SyncLinkedAccount(
        Guid financialAccountId,
        Request.SyncLinkedAccountRequest request)
    {
        var userId = ServiceClaimHelper.GetRequiredUserId(_httpContext);
        return await SyncLinkedAccountForUser(financialAccountId, userId, request);
    }

    public async Task<Response.SepayTransactionsResponse> SyncLinkedAccountForUser(
        Guid financialAccountId,
        Guid userId,
        Request.SyncLinkedAccountRequest request)
    {
        ValidateSyncRequest(request);

        var financialAccount = await _dbContext.FinancialAccounts
            .FirstOrDefaultAsync(x => x.Id == financialAccountId && x.UserId == userId && x.IsActive);
        if (financialAccount == null)
        {
            throw AppValidationException.NotFound(ErrorMessages.FinancialAccountNotFound, "financialAccountId", ErrorCodes.FinancialAccountNotFound);
        }

        if (!string.Equals(financialAccount.ConnectionMode, ConnectionMode.LinkedApi, StringComparison.OrdinalIgnoreCase))
        {
            throw AppValidationException.BadRequest(ErrorMessages.SepaySyncLinkedAccountRequired, "financialAccountId", ErrorCodes.SepaySyncLinkedAccountRequired);
        }

        try
        {
            var records = await FetchTransactionsWithRefresh(financialAccount, request);
            return await UpsertRecordsForAccount(financialAccount, records, "SePay transactions synced.");
        }
        catch (AppValidationException)
        {
            financialAccount.SyncStatus = SyncStatus.Error;
            financialAccount.LastSyncError = "SePay sync failed.";
            financialAccount.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();
            throw;
        }
    }

    public async Task<Response.SepayTransactionsResponse> ProcessSepayWebhook(
        Request.SepayWebhookRequest request,
        string? authorizationHeader)
    {
        if (request == null)
        {
            throw AppValidationException.BadRequest("Request body is required.", "body", ErrorCodes.Required);
        }

        ValidateWebhookAuthorization(authorizationHeader);

        var parsed = ParseWebhookTransaction(request);
        if (parsed == null)
        {
            return new Response.SepayTransactionsResponse
            {
                success = true,
                receivedCount = 0,
                createdCount = 0,
                skippedCount = 0,
                message = "SePay webhook ignored: invalid payload."
            };
        }

        var createdCount = 0;
        var skippedCount = 0;
        await using var databaseTransaction = await _dbContext.Database.BeginTransactionAsync();

        var matchedAccounts = await _dbContext.FinancialAccounts
            .Where(x => x.ProviderCode == ProviderCodes.Sepay
                        && x.ConnectionMode == ConnectionMode.LinkedApi
                        && x.IsActive
                        && (x.ExternalAccountRef == parsed.AccountRef
                            || x.ExternalAccountId == parsed.AccountRef
                            || x.MaskedAccountNumber == parsed.AccountRef))
            .ToListAsync();

        if (matchedAccounts.Count > 1)
        {
            throw AppValidationException.Conflict(ErrorMessages.SepayAccountConflict, "accountNumber", ErrorCodes.SepayAccountConflict);
        }

        if (matchedAccounts.Count == 0)
        {
            skippedCount++;
        }
        else
        {
            var created = await AddTransactionIfMissing(matchedAccounts[0], parsed);
            if (created) createdCount++;
            else skippedCount++;
        }

        await _dbContext.SaveChangesAsync();
        await databaseTransaction.CommitAsync();

        return new Response.SepayTransactionsResponse
        {
            success = true,
            receivedCount = 1,
            createdCount = createdCount,
            skippedCount = skippedCount,
            message = "SePay webhook processed."
        };
    }

    private async Task<IReadOnlyList<SepayTransactionRecord>> FetchTransactionsWithRefresh(
        FinancialAccountEntity financialAccount,
        Request.SyncLinkedAccountRequest request)
    {
        var accessToken = ResolveAccessToken(financialAccount);

        try
        {
            return await FetchTransactionsOnce(financialAccount, accessToken, request);
        }
        catch (AppValidationException ex) when (ex.Code == ErrorCodes.SepayTokenInvalid)
        {
            var refreshed = await TryRefreshToken(financialAccount);
            if (refreshed == null)
            {
                throw;
            }

            return await FetchTransactionsOnce(financialAccount, refreshed, request);
        }
    }

    private async Task<IReadOnlyList<SepayTransactionRecord>> FetchTransactionsOnce(
        FinancialAccountEntity financialAccount,
        string? accessToken,
        Request.SyncLinkedAccountRequest request)
    {
        if (!string.IsNullOrWhiteSpace(financialAccount.ExternalAccountId)
            && financialAccount.ExternalAccountId != financialAccount.ExternalAccountRef)
        {
            return await _sepayClient.GetAccountTransactionsAsync(
                accessToken,
                financialAccount.ExternalAccountId,
                request.fromDate,
                request.toDate,
                request.page,
                request.pageSize,
                request.sort);
        }

        return await _sepayClient.GetTransactionsAsync(
            accessToken,
            request.fromDate,
            request.toDate,
            request.page,
            request.pageSize,
            request.sort);
    }

    private async Task<string?> TryRefreshToken(FinancialAccountEntity financialAccount)
    {
        if (string.IsNullOrWhiteSpace(financialAccount.AccessTokenRef)) return null;
        if (!financialAccount.AccessTokenRef.StartsWith("v1:", StringComparison.Ordinal)) return null;

        var stored = _tokenProtector.Unprotect(financialAccount.AccessTokenRef);
        if (string.IsNullOrWhiteSpace(stored.refreshToken)) return null;

        var refreshed = await _sepayClient.RefreshTokenAsync(stored.refreshToken);
        var newStored = new SepayStoredToken
        {
            accessToken = refreshed.accessToken,
            refreshToken = refreshed.refreshToken ?? stored.refreshToken,
            tokenType = refreshed.tokenType,
            expiresAt = refreshed.expiresAt
        };

        financialAccount.AccessTokenRef = _tokenProtector.Protect(newStored);
        financialAccount.TokenExpiresAt = refreshed.expiresAt;
        financialAccount.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();

        return refreshed.accessToken;
    }

    private async Task<Response.SepayTransactionsResponse> UpsertRecordsForAccount(
        FinancialAccountEntity financialAccount,
        IReadOnlyList<SepayTransactionRecord> records,
        string message)
    {
        var createdCount = 0;
        var skippedCount = 0;
        await using var databaseTransaction = await _dbContext.Database.BeginTransactionAsync();

        foreach (var record in records)
        {
            var parsed = ParseSyncTransaction(record);
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
            if (created) createdCount++;
            else skippedCount++;
        }

        financialAccount.SyncStatus = SyncStatus.Synced;
        financialAccount.LastSyncedAt = DateTimeOffset.UtcNow;
        financialAccount.LastSyncError = null;
        financialAccount.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
        await databaseTransaction.CommitAsync();

        return new Response.SepayTransactionsResponse
        {
            success = true,
            receivedCount = records.Count,
            createdCount = createdCount,
            skippedCount = skippedCount,
            message = message
        };
    }

    private async Task<bool> AddTransactionIfMissing(FinancialAccountEntity financialAccount, ParsedSepayTransaction parsed)
    {
        var existedTransaction = await _dbContext.Transactions.AnyAsync(x =>
            x.FinancialAccountId == financialAccount.Id
            && x.ExternalTransactionId == parsed.ExternalTransactionId
            && !x.IsDeleted);
        if (existedTransaction) return false;

        _dbContext.Transactions.Add(new Repository.Entity.Transaction
        {
            Id = Guid.NewGuid(),
            UserId = financialAccount.UserId,
            FinancialAccountId = financialAccount.Id,
            CategoryId = null,
            FromJarId = null,
            ToJarId = null,
            Type = parsed.Type,
            TransactionsAmount = parsed.AmountAbs,
            Note = parsed.Description,
            RawDescription = parsed.Description,
            TransactionDate = parsed.TransactionDate,
            SourceType = SourceTypes.Imported,
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
        else if (parsed.Type == TransactionType.Income)
        {
            financialAccount.CurrentBalance += parsed.AmountAbs;
        }
        else
        {
            financialAccount.CurrentBalance -= parsed.AmountAbs;
        }

        financialAccount.SyncStatus = SyncStatus.Synced;
        financialAccount.LastSyncedAt = DateTimeOffset.UtcNow;
        financialAccount.LastSyncError = null;
        financialAccount.LastSyncCursor = parsed.ExternalTransactionId;
        financialAccount.UpdatedAt = DateTimeOffset.UtcNow;
        return true;
    }

    private string? ResolveAccessToken(FinancialAccountEntity financialAccount)
    {
        if (string.IsNullOrWhiteSpace(financialAccount.AccessTokenRef)) return null;
        if (!financialAccount.AccessTokenRef.StartsWith("v1:", StringComparison.Ordinal)) return null;

        var token = _tokenProtector.Unprotect(financialAccount.AccessTokenRef);
        return token.accessToken;
    }

    private void ValidateWebhookAuthorization(string? authorizationHeader)
    {
        var configuredKey = _configuration[ConfigKeys.Sepay.WebhookApiKey];
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            throw AppValidationException.BadRequest(ErrorMessages.SepayWebhookTokenMissing, "Authorization", ErrorCodes.SepayWebhookUnauthorized);
        }

        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            throw AppValidationException.BadRequest(ErrorMessages.SepayWebhookVerificationHeaderRequired, "Authorization", ErrorCodes.SepayWebhookUnauthorized);
        }

        var trimmed = authorizationHeader.Trim();
        string providedKey;
        if (trimmed.StartsWith("Apikey ", StringComparison.OrdinalIgnoreCase))
        {
            providedKey = trimmed.Substring("Apikey ".Length).Trim();
        }
        else if (trimmed.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            providedKey = trimmed.Substring("Bearer ".Length).Trim();
        }
        else
        {
            providedKey = trimmed;
        }

        if (providedKey != configuredKey.Trim())
        {
            throw AppValidationException.BadRequest(ErrorMessages.SepayWebhookTokenInvalid, "Authorization", ErrorCodes.SepayWebhookUnauthorized);
        }
    }

    private static ParsedSepayTransaction? ParseWebhookTransaction(Request.SepayWebhookRequest request)
    {
        if (request.id <= 0) return null;
        if (request.transferAmount <= 0) return null;

        var type = string.Equals(request.transferType, "in", StringComparison.OrdinalIgnoreCase)
            ? TransactionType.Income
            : TransactionType.Expense;

        var transactionDate = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.transactionDate)
            && DateTimeOffset.TryParse(request.transactionDate, out var parsedDate))
        {
            transactionDate = parsedDate.ToUniversalTime();
        }

        var description = !string.IsNullOrWhiteSpace(request.content) ? request.content : request.description;

        return new ParsedSepayTransaction
        {
            Type = type,
            AmountAbs = Math.Abs(request.transferAmount),
            ExternalTransactionId = request.id.ToString(),
            AccountRef = request.accountNumber ?? request.subAccount,
            TransactionDate = transactionDate,
            Description = description,
            RunningBalance = request.accumulated,
            RawJson = System.Text.Json.JsonSerializer.Serialize(request)
        };
    }

    private static ParsedSepayTransaction? ParseSyncTransaction(SepayTransactionRecord record)
    {
        var item = record.payload;
        var amount = ReadDecimal(item, "amount")
                     ?? ReadDecimal(item, "transferAmount")
                     ?? ReadDecimal(item, "transfer_amount");
        if (!amount.HasValue || amount.Value == 0) return null;

        var externalTransactionId = ReadFlexibleString(item, "id")
                                    ?? ReadFlexibleString(item, "reference_code")
                                    ?? ReadFlexibleString(item, "referenceCode")
                                    ?? ReadFlexibleString(item, "tid");
        if (string.IsNullOrWhiteSpace(externalTransactionId)) return null;

        var transferType = ReadFlexibleString(item, "transferType")
                           ?? ReadFlexibleString(item, "transfer_type");

        string type;
        decimal amountAbs;
        if (!string.IsNullOrWhiteSpace(transferType))
        {
            type = string.Equals(transferType, "in", StringComparison.OrdinalIgnoreCase)
                ? TransactionType.Income
                : TransactionType.Expense;
            amountAbs = Math.Abs(amount.Value);
        }
        else
        {
            type = amount.Value > 0 ? TransactionType.Income : TransactionType.Expense;
            amountAbs = Math.Abs(amount.Value);
        }

        var transactionDate = DateTimeOffset.UtcNow;
        var transactionDateText = ReadFlexibleString(item, "transactionDate")
                                  ?? ReadFlexibleString(item, "transaction_date")
                                  ?? ReadFlexibleString(item, "when");
        if (!string.IsNullOrWhiteSpace(transactionDateText)
            && DateTimeOffset.TryParse(transactionDateText, out var parsedDate))
        {
            transactionDate = parsedDate.ToUniversalTime();
        }

        return new ParsedSepayTransaction
        {
            Type = type,
            AmountAbs = amountAbs,
            ExternalTransactionId = externalTransactionId,
            AccountRef = ReadFlexibleString(item, "accountNumber")
                         ?? ReadFlexibleString(item, "account_number")
                         ?? ReadFlexibleString(item, "subAccount"),
            TransactionDate = transactionDate,
            Description = ReadFlexibleString(item, "content")
                          ?? ReadFlexibleString(item, "description"),
            RunningBalance = ReadDecimal(item, "accumulated")
                             ?? ReadDecimal(item, "runningBalance"),
            RawJson = item.GetRawText()
        };
    }

    private static void ValidateSyncRequest(Request.SyncLinkedAccountRequest request)
    {
        if (request == null)
        {
            throw AppValidationException.BadRequest("Request body is required.", "body", ErrorCodes.Required);
        }

        if (request.page <= 0)
        {
            throw AppValidationException.BadRequest(ErrorMessages.PageMustBeGreaterThanZero, "page", ErrorCodes.SepaySyncInvalid);
        }

        if (request.pageSize <= 0 || request.pageSize > 100)
        {
            throw AppValidationException.BadRequest(ErrorMessages.PageSizeBetween1And100, "pageSize", ErrorCodes.SepaySyncInvalid);
        }
    }

    private static string? ReadFlexibleString(System.Text.Json.JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value)) return null;

        return value.ValueKind switch
        {
            System.Text.Json.JsonValueKind.String => value.GetString(),
            System.Text.Json.JsonValueKind.Number => value.GetRawText(),
            _ => null
        };
    }

    private static decimal? ReadDecimal(System.Text.Json.JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == System.Text.Json.JsonValueKind.Number
            ? value.GetDecimal()
            : null;
    }

    private class ParsedSepayTransaction
    {
        public required string Type { get; set; }
        public required decimal AmountAbs { get; set; }
        public required string ExternalTransactionId { get; set; }
        public string? AccountRef { get; set; }
        public required DateTimeOffset TransactionDate { get; set; }
        public string? Description { get; set; }
        public decimal? RunningBalance { get; set; }
        public required string RawJson { get; set; }
    }
}
