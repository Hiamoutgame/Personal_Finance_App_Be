using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Entity;
using Personal_Finance_Management.Service.Base;
using Personal_Finance_Management.Service.Common.Constants;
using Personal_Finance_Management.Service.Common.Enums;
using Personal_Finance_Management.Service.Sepay;
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
    private readonly ISepayClient _sepayClient;
    private readonly ISepayTokenProtector _tokenProtector;
    private readonly BankSyncService _bankSyncService;

    public Service(
        AppDbContext dbContext,
        IHttpContextAccessor httpContext,
        IConfiguration configuration,
        ISepayClient sepayClient,
        ISepayTokenProtector tokenProtector,
        BankSyncService bankSyncService)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
        _configuration = configuration;
        _sepayClient = sepayClient;
        _tokenProtector = tokenProtector;
        _bankSyncService = bankSyncService;
    }

    public async Task<Response.StartSepayConnectionResponse> StartSepayConnection(Request.StartSepayConnectionRequest request)
    {
        var userId = ServiceClaimHelper.GetRequiredUserId(_httpContext);
        var userExists = await _dbContext.Accounts.AnyAsync(x => x.Id == userId);
        if (!userExists)
        {
            throw AppValidationException.NotFound("User not found.", "userId", ErrorCodes.UserNotFound);
        }

        if (!HasOAuthClientConfigured())
        {
            return await StartSepayConnectionWithApiKey(userId, request);
        }

        var redirectUri = ResolveRedirectUri();
        var state = CreateState();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
        var session = new BankConnectionSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProviderCode = ProviderCodes.Sepay,
            State = state,
            CodeVerifier = null,
            ReturnUrl = NormalizeReturnUrl(request?.returnUrl),
            IsDefault = request?.isDefault ?? false,
            AutoSync = request?.autoSync ?? true,
            Status = BankConnectionSessionStatus.Pending,
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.BankConnectionSessions.Add(session);
        await _dbContext.SaveChangesAsync();

        return new Response.StartSepayConnectionResponse
        {
            success = true,
            message = "SePay OAuth connection session created.",
            connectionMode = "OAuth",
            sessionId = session.Id,
            authorizationUrl = _sepayClient.BuildAuthorizationUrl(state, redirectUri),
            expiresAt = expiresAt,
            financialAccountId = null,
            financialAccountIds = new List<Guid>()
        };
    }

    public async Task<Response.SepayCallbackResponse> HandleSepayCallback(string? code, string? state, string? error)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            throw AppValidationException.BadRequest(ErrorMessages.SepayStateRequired, "state", ErrorCodes.SepayStateRequired);
        }

        var session = await _dbContext.BankConnectionSessions
            .FirstOrDefaultAsync(x => x.State == state && x.ProviderCode == ProviderCodes.Sepay);
        if (session == null)
        {
            throw AppValidationException.BadRequest(ErrorMessages.SepaySessionNotFound, "state", ErrorCodes.SepaySessionNotFound);
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            session.Status = BankConnectionSessionStatus.Failed;
            session.ErrorMessage = error;
            session.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();
            return BuildCallbackResponse(false, "SePay authorization failed.", session, null);
        }

        if (session.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            session.Status = BankConnectionSessionStatus.Expired;
            session.ErrorMessage = "Session expired.";
            session.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();
            return BuildCallbackResponse(false, "SePay connection session expired.", session, null);
        }

        if (session.Status != BankConnectionSessionStatus.Pending)
        {
            return BuildCallbackResponse(false, "SePay connection session was already used.", session, null);
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            session.Status = BankConnectionSessionStatus.Failed;
            session.ErrorMessage = "Authorization code is missing.";
            session.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();
            return BuildCallbackResponse(false, "SePay authorization code is missing.", session, null);
        }

        var token = await _sepayClient.ExchangeCodeForTokenAsync(code, ResolveRedirectUri());
        var accounts = await _sepayClient.GetAccountsAsync(token.accessToken);
        if (accounts.Count == 0)
        {
            session.Status = BankConnectionSessionStatus.Failed;
            session.ErrorMessage = "No SePay bank account found.";
            session.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();
            return BuildCallbackResponse(false, "No SePay bank account found.", session, null);
        }

        var storedToken = _tokenProtector.Protect(new SepayStoredToken
        {
            accessToken = token.accessToken,
            refreshToken = token.refreshToken,
            tokenType = token.tokenType,
            expiresAt = token.expiresAt
        });

        await using var databaseTransaction = await _dbContext.Database.BeginTransactionAsync();
        var linkedFinancialAccountIds = await UpsertSepayFinancialAccounts(
            session.UserId,
            accounts,
            storedToken,
            token.expiresAt,
            session.IsDefault);

        session.Status = BankConnectionSessionStatus.Completed;
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
        return BuildCallbackResponse(true, "SePay bank account linked.", session, firstLinkedAccountId);
    }

    private async Task<Response.StartSepayConnectionResponse> StartSepayConnectionWithApiKey(
        Guid userId,
        Request.StartSepayConnectionRequest? request)
    {
        var accounts = await _sepayClient.GetAccountsAsync(null);
        if (accounts.Count == 0)
        {
            throw AppValidationException.BadRequest(ErrorMessages.SepayAccountNotFound, "sepay", ErrorCodes.SepayAccountNotFound);
        }

        await using var databaseTransaction = await _dbContext.Database.BeginTransactionAsync();
        var linkedFinancialAccountIds = await UpsertSepayFinancialAccounts(
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

        return new Response.StartSepayConnectionResponse
        {
            success = true,
            message = "SePay bank accounts linked with API key.",
            connectionMode = "ApiKey",
            sessionId = null,
            authorizationUrl = null,
            expiresAt = null,
            financialAccountId = linkedFinancialAccountIds.Count > 0 ? linkedFinancialAccountIds[0] : null,
            financialAccountIds = linkedFinancialAccountIds
        };
    }

    private async Task<List<Guid>> UpsertSepayFinancialAccounts(
        Guid userId,
        IReadOnlyList<SepayAccount> accounts,
        string? accessTokenRef,
        DateTimeOffset? tokenExpiresAt,
        bool markFirstAsDefault)
    {
        var linkedFinancialAccountIds = new List<Guid>();

        for (var index = 0; index < accounts.Count; index++)
        {
            var sepayAccount = accounts[index];
            var existingOwnerAccount = await _dbContext.FinancialAccounts
                .FirstOrDefaultAsync(x => x.ProviderCode == ProviderCodes.Sepay
                                          && x.ExternalAccountId == sepayAccount.externalId
                                          && x.UserId != userId
                                          && x.IsActive);
            if (existingOwnerAccount != null)
            {
                throw AppValidationException.Conflict(ErrorMessages.SepayAccountAlreadyLinked, "externalAccountId", ErrorCodes.SepayAccountAlreadyLinked);
            }

            var financialAccount = await _dbContext.FinancialAccounts
                .FirstOrDefaultAsync(x => x.ProviderCode == ProviderCodes.Sepay
                                          && x.ExternalAccountId == sepayAccount.externalId
                                          && x.UserId == userId);
            if (financialAccount == null)
            {
                financialAccount = new FinancialAccountEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    AccountType = FinancialAccountType.Bank,
                    ConnectionMode = ConnectionMode.LinkedApi,
                    Currency = CurrencyDefaults.Vnd,
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsActive = true
                };
                _dbContext.FinancialAccounts.Add(financialAccount);
            }

            financialAccount.Name = sepayAccount.bankName ?? sepayAccount.name;
            financialAccount.ProviderCode = ProviderCodes.Sepay;
            financialAccount.ProviderName = ProviderCodes.SepayDisplay;
            financialAccount.ExternalAccountId = sepayAccount.externalId;
            financialAccount.ExternalAccountRef = sepayAccount.accountNumber;
            financialAccount.MaskedAccountNumber = ServiceTextHelper.MaskTrailing(sepayAccount.accountNumber);
            financialAccount.AccountHolderName = sepayAccount.accountHolderName;
            financialAccount.CurrentBalance = sepayAccount.balance ?? financialAccount.CurrentBalance;
            financialAccount.SyncStatus = SyncStatus.Synced;
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
        var connectionMode = _configuration[ConfigKeys.Sepay.ConnectionMode];
        if (string.Equals(connectionMode, "ApiKey", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(_configuration[ConfigKeys.Sepay.ClientId])
               && !string.IsNullOrWhiteSpace(_configuration[ConfigKeys.Sepay.ClientSecret]);
    }

    private string ResolveRedirectUri()
    {
        var redirectUri = _configuration[ConfigKeys.Sepay.RedirectUri];
        if (string.IsNullOrWhiteSpace(redirectUri))
        {
            throw AppValidationException.BadRequest(ErrorMessages.SepayRedirectUriMissing, ConfigKeys.Sepay.RedirectUri, ErrorCodes.SepayConfigMissing);
        }

        return redirectUri;
    }

    private string? NormalizeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return _configuration[ConfigKeys.Sepay.DefaultReturnUrl];
        }

        var normalized = returnUrl.Trim();
        var allowedPrefix = _configuration[ConfigKeys.Sepay.AllowedReturnUrlPrefix];
        if (!string.IsNullOrWhiteSpace(allowedPrefix)
            && normalized.StartsWith(allowedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return normalized;
        }

        if (Uri.TryCreate(normalized, UriKind.Relative, out _))
        {
            return normalized;
        }

        return _configuration[ConfigKeys.Sepay.DefaultReturnUrl];
    }

    private static Response.SepayCallbackResponse BuildCallbackResponse(
        bool success,
        string message,
        BankConnectionSession session,
        Guid? financialAccountId)
    {
        var redirectUrl = session.ReturnUrl;
        if (!string.IsNullOrWhiteSpace(redirectUrl))
        {
            var separator = redirectUrl.Contains('?') ? "&" : "?";
            redirectUrl = $"{redirectUrl}{separator}sepayStatus={(success ? "success" : "failed")}";
        }

        return new Response.SepayCallbackResponse
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
