using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Service.Validations;

namespace Personal_Finance_Management.Service.FinancialAccount;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }
    
    public async Task<Response.GetFinancialAccountResult> GetUserFinancialAccount()
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        var userIdGuid = Guid.Parse(userId);

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);

        if (user == null)
            throw new Exception("User not found");
        var query = _dbContext.FinancialAccounts.Where(x => x.UserId == userIdGuid);
        var selectedQuery = query.Select(x => new Response.GetFinancialAccountResponse
        {
            id = x.Id,
            name = x.Name,
            accountType = x.AccountType,
            connectionMode = x.ConnectionMode,
            providerName = x.ProviderName,
            maskedAccountNumber = x.MaskedAccountNumber,
            currency = x.Currency,
            currentBalance = x.CurrentBalance,
            syncStatus = x.SyncStatus,
            isDefault = x.IsDefault,
            isActive = x.IsActive
        });
        var accountList = await selectedQuery.ToListAsync();
        var result = new Response.GetFinancialAccountResult
        {
            data = accountList,
        };
        return result;
    }

    public async Task<Response.CreateManualFinancialAccountResponse> CreateManualFinancialAccount(Request.CreateManualFinancialAccountRequest request)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        var userIdGuid = Guid.Parse(userId);

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);

        if (user == null)
            throw new Exception("User not found");

        var accountName = request.name?.Trim();
        var accountType = request.accountType?.Trim();
        var currency = string.IsNullOrWhiteSpace(request.currency) ? "VND" : request.currency.Trim();

        if (string.IsNullOrWhiteSpace(accountName))
        {
            throw AppValidationException.BadRequest("Financial account name is required", "name", "FINANCIAL_ACCOUNT_NAME_REQUIRED");
        }

        if (accountName.Length > 100)
        {
            throw AppValidationException.BadRequest("Financial account name is too long", "name", "FINANCIAL_ACCOUNT_NAME_TOO_LONG");
        }

        if (string.IsNullOrWhiteSpace(accountType)
            || !new[] { "Cash", "Bank", "EWallet", "Other" }.Contains(accountType))
        {
            throw AppValidationException.BadRequest("Invalid financial account type", "accountType", "INVALID_FINANCIAL_ACCOUNT_TYPE");
        }

        if (currency.Length != 3 || currency.Any(x => !char.IsLetter(x)))
        {
            throw AppValidationException.BadRequest("Currency must be a 3-letter code, for example VND", "currency", "INVALID_CURRENCY");
        }

        currency = currency.ToUpperInvariant();

        var existedAccount = _dbContext.FinancialAccounts.FirstOrDefault(x => x.UserId == userIdGuid && x.Name == accountName);
        if (existedAccount != null)
        {
            throw AppValidationException.Conflict("Financial account already exists", "name", "FINANCIAL_ACCOUNT_ALREADY_EXISTS");
        }

        var financialAccount = new Repository.Entity.FinancialAccount()
        {
            Id = Guid.NewGuid(),
            Name = accountName,
            AccountType = accountType,
            ConnectionMode = "Manual",
            CurrentBalance = request.currentBalance,
            Currency = currency,
            SyncStatus = "NeverSynced",
            UserId = user.Id,
            IsDefault = request.isDefault,
            IsActive = true
        };

        _dbContext.FinancialAccounts.Add(financialAccount);
        await _dbContext.SaveChangesAsync();

        var result = new Response.CreateManualFinancialAccountResponse
        {
            id = financialAccount.Id,
            name = financialAccount.Name,
            accountType = financialAccount.AccountType,
            connectionMode = financialAccount.ConnectionMode,
            currentBalance = financialAccount.CurrentBalance,
            currency = financialAccount.Currency,
            isDefault = financialAccount.IsDefault,
            isActive = financialAccount.IsActive
        };

        return result;
    }

    public async Task<Response.CreateLinkApiFinancialAccountResponse> CreateLinkApiFinancialAccount(Request.CreateLinkApiFinancialAccountRequest request)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        var userIdGuid = Guid.Parse(userId);

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);

        if (user == null)
            throw new Exception("User not found");

        var bankName = request.bankName?.Trim();
        var accountNumber = request.accountNumber?.Trim();
        var accountHolderName = request.accountHolderName?.Trim();
        var bankCode = request.bankCode?.Trim();

        if (string.IsNullOrWhiteSpace(bankName))
        {
            throw AppValidationException.BadRequest("Bank name is required", "bankName", "BANK_NAME_REQUIRED");
        }

        if (bankName.Length > 100)
        {
            throw AppValidationException.BadRequest("Bank name is too long", "bankName", "BANK_NAME_TOO_LONG");
        }

        if (bankCode?.Length > 50)
        {
            throw AppValidationException.BadRequest("Bank code is too long", "bankCode", "BANK_CODE_TOO_LONG");
        }

        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            throw AppValidationException.BadRequest("Bank account number is required", "accountNumber", "BANK_ACCOUNT_NUMBER_REQUIRED");
        }

        if (accountNumber.Length > 50)
        {
            throw AppValidationException.BadRequest("Bank account number is too long", "accountNumber", "BANK_ACCOUNT_NUMBER_TOO_LONG");
        }

        if (accountHolderName?.Length > 150)
        {
            throw AppValidationException.BadRequest("Account holder name is too long", "accountHolderName", "ACCOUNT_HOLDER_NAME_TOO_LONG");
        }

        var existedLinkedAccount = _dbContext.FinancialAccounts.FirstOrDefault(x =>
            x.UserId == userIdGuid
            && x.ConnectionMode == "LinkedApi"
            && (x.ExternalAccountRef == accountNumber
                || x.ExternalAccountId == accountNumber
                || x.MaskedAccountNumber == accountNumber));

        if (existedLinkedAccount != null)
        {
            throw AppValidationException.Conflict("Linked bank account already exists", "accountNumber", "LINKED_ACCOUNT_ALREADY_EXISTS");
        }

        var maskedAccountNumber = accountNumber.Length <= 4
            ? accountNumber
            : new string('*', accountNumber.Length - 4) + accountNumber.Substring(accountNumber.Length - 4);

        var financialAccount = new Repository.Entity.FinancialAccount()
        {
            Id = Guid.NewGuid(),
            Name = bankName,
            AccountType = "Bank",
            ConnectionMode = "LinkedApi",
            ProviderCode = "casso",
            ProviderName = "Casso",
            ExternalAccountId = accountNumber,
            ExternalAccountRef = accountNumber,
            MaskedAccountNumber = maskedAccountNumber,
            AccountHolderName = accountHolderName,
            CurrentBalance = 0,
            Currency = "VND",
            SyncStatus = "NeverSynced",
            UserId = user.Id,
            IsDefault = request.isDefault,
            IsActive = true
        };

        _dbContext.FinancialAccounts.Add(financialAccount);
        await _dbContext.SaveChangesAsync();

        var result = new Response.CreateLinkApiFinancialAccountResponse
        {
            id = financialAccount.Id,
            name = financialAccount.Name,
            accountType = financialAccount.AccountType,
            connectionMode = financialAccount.ConnectionMode,
            providerName = financialAccount.ProviderName,
            maskedAccountNumber = financialAccount.MaskedAccountNumber,
            currentBalance = financialAccount.CurrentBalance,
            currency = financialAccount.Currency,
            syncStatus = financialAccount.SyncStatus,
            isDefault = financialAccount.IsDefault,
            isActive = financialAccount.IsActive
        };

        return result;
    }

    public async Task<Response.UpdateFinancialAccountResponse> UpdateFinancialAccount(Guid id, Request.UpdateFinancialAccountRequest request)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        var userIdGuid = Guid.Parse(userId);

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);

        if (user == null)
            throw new Exception("User not found");
        // var existedAccount = _dbContext.FinancialAccounts.FirstOrDefault(x => x.Id == id && x.Name == request.name);
        // if (existedAccount != null)
        // {
        //     throw new Exception("FinancialAccount already exists");
        // }
        var query = _dbContext.FinancialAccounts.FirstOrDefault(x => x.Id == id && x.UserId == userIdGuid);
        if (query == null)
        {
            throw AppValidationException.NotFound("Financial account not found", "financialAccountId", "FINANCIAL_ACCOUNT_NOT_FOUND");
        }

        if (string.Equals(query.ConnectionMode, "LinkedApi", StringComparison.OrdinalIgnoreCase)
            && request.currentBalance.HasValue)
        {
            throw AppValidationException.BadRequest("Linked bank account balance cannot be updated manually.", "currentBalance", "LINKED_ACCOUNT_BALANCE_READ_ONLY");
        }

        query.Name = request.name ?? query.Name;
        query.CurrentBalance = request.currentBalance ?? query.CurrentBalance;
        query.IsDefault = request.isDefault ?? query.IsDefault;
        query.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        var result = new Response.UpdateFinancialAccountResponse
        {
            id = query.Id,
            name = query.Name,
            currentBalance = query.CurrentBalance,
            isDefault = query.IsDefault,
            updatedAt = query.UpdatedAt,
        };
        return result;
    }

    public async Task<Response.DeleteFinancialAccountResponse> DeleteFinancialAccount(Guid id)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        var userIdGuid = Guid.Parse(userId);

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);

        if (user == null)
            throw new Exception("User not found");
        var financialAccount = _dbContext.FinancialAccounts.FirstOrDefault(x => x.Id == id);
        financialAccount.IsActive = false;
        await _dbContext.SaveChangesAsync();
        var result = new Response.DeleteFinancialAccountResponse()
        {
            message = "Financial account deactivated"
        };
        return result;
    }
}
