using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;

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

    public async Task<Response.CreateFinancialAccountResponse> CreateFinancialAccount(Request.CreateFinancialAccountRequest request)
    {
        
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        var userIdGuid = Guid.Parse(userId);

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);

        if (user == null)
            throw new Exception("User not found");
        var existedAccount = _dbContext.FinancialAccounts.FirstOrDefault(x => x.UserId == userIdGuid && x.Name == request.name);
        if (existedAccount != null)
        {
            throw new Exception("FinancialAccount already exists");
        }
        var FinancialDetail = new Repository.Entity.FinancialAccount()
        {
            Id = Guid.NewGuid(),
            Name = request.name,
            AccountType = request.accountType,
            ConnectionMode = "Manual",
            CurrentBalance = request.currentBalance,
            Currency = request.currency,
            UserId =  user.Id,
            IsDefault = request.isDefault,
            IsActive = true
        };
        _dbContext.FinancialAccounts.Add(FinancialDetail);
        await _dbContext.SaveChangesAsync();

        var result = new Response.CreateFinancialAccountResponse
        {
            id = FinancialDetail.Id,
            name = FinancialDetail.Name,
            accountType = FinancialDetail.AccountType,
            connectionMode = FinancialDetail.ConnectionMode,
            currentBalance = FinancialDetail.CurrentBalance,
            currency = FinancialDetail.Currency,
            isDefault = FinancialDetail.IsDefault,
            isActive = FinancialDetail.IsActive
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
        var existedAccount = _dbContext.FinancialAccounts.FirstOrDefault(x => x.Id == id && x.Name == request.name);
        if (existedAccount != null)
        {
            throw new Exception("FinancialAccount already exists");
        }
        var query = _dbContext.FinancialAccounts.FirstOrDefault(x => x.Id == id);
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