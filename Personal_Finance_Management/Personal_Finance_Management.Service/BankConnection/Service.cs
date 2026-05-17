using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Entity;
using Personal_Finance_Management.Service.Base;
using Personal_Finance_Management.Service.Casso;
using Personal_Finance_Management.Service.Validations;
using BankSyncRequest = Personal_Finance_Management.Service.BankSync.Request;
using BankSyncService = Personal_Finance_Management.Service.BankSync.IService;
using FinancialAccountEntity = Personal_Finance_Management.Repository.Entity.FinancialAccount;

namespace Personal_Finance_Management.Service.BankConnection;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IConfiguration _configuration;
    private readonly ICassoClient _cassoClient;
    private readonly ICassoTokenProtector _tokenProtector;
    private readonly BankSyncService _bankSyncService;

    public Service(
        AppDbContext dbContext,
        IHttpContextAccessor httpContext,
        IConfiguration configuration,
        ICassoClient cassoClient,
        ICassoTokenProtector tokenProtector,
        BankSyncService bankSyncService)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
        _configuration = configuration;
        _cassoClient = cassoClient;
        _tokenProtector = tokenProtector;
        _bankSyncService = bankSyncService;
    }

    public async Task<Response.StartCassoConnectionResponse> StartCassoConnection(Request.StartCassoConnectionRequest request)
    {
        var userId = ServiceClaimHelper.GetRequiredUserId(_httpContext);
        var userExists = await _dbContext.Accounts.AnyAsync(x => x.Id == userId);
        if (!userExists)
        {
            throw AppValidationException.NotFound("User not found.", "userId", "USER_NOT_FOUND");
        }

        if (!HasOAuthClientConfigured())
        {
            return await StartCassoConnectionWithApiKey(userId, request);
        }

        var redirectUri = ResolveRedirectUri();
        var state = CreateState();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
        var session = new BankConnectionSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProviderCode = "casso",
            State = state,
            CodeVerifier = null,
            ReturnUrl = NormalizeReturnUrl(request?.returnUrl),
            IsDefault = request?.isDefault ?? false,
            AutoSync = request?.autoSync ?? true,
            Status = "Pending",
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.BankConnectionSessions.Add(session);
        await _dbContext.SaveChangesAsync();

        return new Response.StartCassoConnectionResponse
        {
            success = true,
            message = "Casso OAuth connection session created.",
            connectionMode = "OAuth",
            sessionId = session.Id,
            authorizationUrl = _cassoClient.BuildAuthorizationUrl(state, redirectUri),
            expiresAt = expiresAt,
            financialAccountId = null,
            financialAccountIds = new List<Guid>()
        };
    }

    public async Task<Response.CassoCallbackResponse> HandleCassoCallback(string? code, string? state, string? error)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            throw AppValidationException.BadRequest("Casso callback state is required.", "state", "CASSO_STATE_REQUIRED");
        }

        var session = await _dbContext.BankConnectionSessions
            .FirstOrDefaultAsync(x => x.State == state && x.ProviderCode == "casso");
        if (session == null)
        {
            throw AppValidationException.BadRequest("Casso connection session not found.", "state", "CASSO_SESSION_NOT_FOUND");
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            session.Status = "Failed";
            session.ErrorMessage = error;
            session.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();
            return BuildCallbackResponse(false, "Casso authorization failed.", session, null);
        }

        if (session.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            session.Status = "Expired";
            session.ErrorMessage = "Session expired.";
            session.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();
            return BuildCallbackResponse(false, "Casso connection session expired.", session, null);
        }

        if (session.Status != "Pending")
        {
            return BuildCallbackResponse(false, "Casso connection session was already used.", session, null);
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            session.Status = "Failed";
            session.ErrorMessage = "Authorization code is missing.";
            session.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();
            return BuildCallbackResponse(false, "Casso authorization code is missing.", session, null);
        }

        var token = await _cassoClient.ExchangeCodeForTokenAsync(code, ResolveRedirectUri());
        var accounts = await _cassoClient.GetAccountsAsync(token.accessToken);
        if (accounts.Count == 0)
        {
            session.Status = "Failed";
            session.ErrorMessage = "No Casso bank account found.";
            session.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();
            return BuildCallbackResponse(false, "No Casso bank account found.", session, null);
        }

        var storedToken = _tokenProtector.Protect(new CassoStoredToken
        {
            accessToken = token.accessToken,
            refreshToken = token.refreshToken,
            tokenType = token.tokenType,
            expiresAt = token.expiresAt
        });

        await using var databaseTransaction = await _dbContext.Database.BeginTransactionAsync();
        var linkedFinancialAccountIds = await UpsertCassoFinancialAccounts(
            session.UserId,
            accounts,
            storedToken,
            token.expiresAt,
            session.IsDefault);

        session.Status = "Completed";
        session.CompletedAt = DateTimeOffset.UtcNow;
        session.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
        await databaseTransaction.CommitAsync();

        if (session.AutoSync)
        {
            foreach (var financialAccountId in linkedFinancialAccountIds)
            {
                try
                {
                    await _bankSyncService.SyncLinkedAccountForUser(
                        financialAccountId,
                        session.UserId,
                        new BankSyncRequest.SyncLinkedAccountRequest
                        {
                            page = 1,
                            pageSize = 50,
                            sort = "ASC",
                            triggerProviderSync = false
                        });
                }
                catch (AppValidationException)
                {
                    // Linking has already succeeded. The linked account keeps sync metadata for the failed sync attempt.
                }
            }
        }

        var firstLinkedAccountId = linkedFinancialAccountIds.Count > 0
            ? linkedFinancialAccountIds[0]
            : (Guid?)null;
        return BuildCallbackResponse(true, "Casso bank account linked.", session, firstLinkedAccountId);
    }

    private async Task<Response.StartCassoConnectionResponse> StartCassoConnectionWithApiKey(
        Guid userId,
        Request.StartCassoConnectionRequest? request)
    {
        var accounts = await _cassoClient.GetAccountsAsync(null);
        if (accounts.Count == 0)
        {
            throw AppValidationException.BadRequest("No Casso bank account found.", "casso", "CASSO_ACCOUNT_NOT_FOUND");
        }

        await using var databaseTransaction = await _dbContext.Database.BeginTransactionAsync();
        var linkedFinancialAccountIds = await UpsertCassoFinancialAccounts(
            userId,
            accounts,
            accessTokenRef: null,
            tokenExpiresAt: null,
            markFirstAsDefault: request?.isDefault ?? false);
        await _dbContext.SaveChangesAsync();
        await databaseTransaction.CommitAsync();

        if (request?.autoSync ?? true)
        {
            foreach (var financialAccountId in linkedFinancialAccountIds)
            {
                try
                {
                    await _bankSyncService.SyncLinkedAccountForUser(
                        financialAccountId,
                        userId,
                        new BankSyncRequest.SyncLinkedAccountRequest
                        {
                            page = 1,
                            pageSize = 50,
                            sort = "ASC",
                            triggerProviderSync = false
                        });
                }
                catch (AppValidationException)
                {
                    // Linking has already succeeded. The linked account keeps sync metadata for the failed sync attempt.
                }
            }
        }

        return new Response.StartCassoConnectionResponse
        {
            success = true,
            message = "Casso bank accounts linked with API key.",
            connectionMode = "ApiKey",
            sessionId = null,
            authorizationUrl = null,
            expiresAt = null,
            financialAccountId = linkedFinancialAccountIds.Count > 0 ? linkedFinancialAccountIds[0] : null,
            financialAccountIds = linkedFinancialAccountIds
        };
    }

    private async Task<List<Guid>> UpsertCassoFinancialAccounts(
        Guid userId,
        IReadOnlyList<CassoAccount> accounts,
        string? accessTokenRef,
        DateTimeOffset? tokenExpiresAt,
        bool markFirstAsDefault)
    {
        var linkedFinancialAccountIds = new List<Guid>();

        for (var index = 0; index < accounts.Count; index++)
        {
            var cassoAccount = accounts[index];
            var existingOwnerAccount = await _dbContext.FinancialAccounts
                .FirstOrDefaultAsync(x => x.ProviderCode == "casso"
                                          && x.ExternalAccountId == cassoAccount.externalId
                                          && x.UserId != userId
                                          && x.IsActive);
            if (existingOwnerAccount != null)
            {
                throw AppValidationException.Conflict("Casso bank account is already linked to another user.", "externalAccountId", "CASSO_ACCOUNT_ALREADY_LINKED");
            }

            var financialAccount = await _dbContext.FinancialAccounts
                .FirstOrDefaultAsync(x => x.ProviderCode == "casso"
                                          && x.ExternalAccountId == cassoAccount.externalId
                                          && x.UserId == userId);
            if (financialAccount == null)
            {
                financialAccount = new FinancialAccountEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    AccountType = "Bank",
                    ConnectionMode = "LinkedApi",
                    Currency = "VND",
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsActive = true
                };
                _dbContext.FinancialAccounts.Add(financialAccount);
            }

            financialAccount.Name = cassoAccount.bankName ?? cassoAccount.name;
            financialAccount.ProviderCode = "casso";
            financialAccount.ProviderName = "Casso";
            financialAccount.ExternalAccountId = cassoAccount.externalId;
            financialAccount.ExternalAccountRef = cassoAccount.accountNumber;
            financialAccount.MaskedAccountNumber = ServiceTextHelper.MaskTrailing(cassoAccount.accountNumber);
            financialAccount.AccountHolderName = cassoAccount.accountHolderName;
            financialAccount.CurrentBalance = cassoAccount.balance ?? financialAccount.CurrentBalance;
            financialAccount.SyncStatus = "Synced";
            financialAccount.LastSyncedAt = DateTimeOffset.UtcNow;
            financialAccount.LastSyncError = null;
            financialAccount.AccessTokenRef = accessTokenRef;
            financialAccount.TokenExpiresAt = tokenExpiresAt;
            financialAccount.UpdatedAt = DateTimeOffset.UtcNow;
            financialAccount.IsDefault = markFirstAsDefault && index == 0;

            if (financialAccount.IsDefault)
            {
                var otherDefaultAccounts = await _dbContext.FinancialAccounts
                    .Where(x => x.UserId == userId && x.Id != financialAccount.Id && x.IsDefault)
                    .ToListAsync();
                foreach (var otherAccount in otherDefaultAccounts)
                {
                    otherAccount.IsDefault = false;
                    otherAccount.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }

            linkedFinancialAccountIds.Add(financialAccount.Id);
        }

        return linkedFinancialAccountIds;
    }

    private bool HasOAuthClientConfigured()
    {
        var connectionMode = _configuration["Casso:ConnectionMode"];
        if (string.Equals(connectionMode, "ApiKey", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(_configuration["Casso:ClientId"])
               && !string.IsNullOrWhiteSpace(_configuration["Casso:ClientSecret"]);
    }

    private string ResolveRedirectUri()
    {
        var redirectUri = _configuration["Casso:RedirectUri"];
        if (string.IsNullOrWhiteSpace(redirectUri))
        {
            throw AppValidationException.BadRequest("Casso redirect URI is not configured.", "Casso:RedirectUri", "CASSO_CONFIG_MISSING");
        }

        return redirectUri;
    }

    private string? NormalizeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return _configuration["Casso:DefaultReturnUrl"];
        }

        var normalized = returnUrl.Trim();
        var allowedPrefix = _configuration["Casso:AllowedReturnUrlPrefix"];
        if (!string.IsNullOrWhiteSpace(allowedPrefix)
            && normalized.StartsWith(allowedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return normalized;
        }

        if (Uri.TryCreate(normalized, UriKind.Relative, out _))
        {
            return normalized;
        }

        return _configuration["Casso:DefaultReturnUrl"];
    }

    private static Response.CassoCallbackResponse BuildCallbackResponse(
        bool success,
        string message,
        BankConnectionSession session,
        Guid? financialAccountId)
    {
        var redirectUrl = session.ReturnUrl;
        if (!string.IsNullOrWhiteSpace(redirectUrl))
        {
            var separator = redirectUrl.Contains('?') ? "&" : "?";
            redirectUrl = $"{redirectUrl}{separator}cassoStatus={(success ? "success" : "failed")}";
        }

        return new Response.CassoCallbackResponse
        {
            success = success,
            message = message,
            financialAccountId = financialAccountId,
            redirectUrl = redirectUrl
        };
    }

    private static string CreateState()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
