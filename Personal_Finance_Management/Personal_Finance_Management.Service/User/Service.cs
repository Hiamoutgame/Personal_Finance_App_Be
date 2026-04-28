using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;

namespace Personal_Finance_Management.Service.User;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }
    public async Task<Response.GetUserInforResponse> GetUserInfor()
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        var userIdGuid = Guid.Parse(userId);
        
        var query = _dbContext.Accounts.Where(x => x.Id == userIdGuid);
        var selectedQuery = query.Select(x => new Response.GetUserInforResponse()
        {
            Id = x.Id,
            userName = x.Username,
            fullName = x.FirstName + " " + x.LastName,
            email = x.Email,
            phone = x.Email,
            avatarUrl = x.AvatarUrl,
            preferredCurrency = x.PreferredCurrency,
            isOnboardingCompleted = x.IsOnboardingCompleted
        });
        var result = await selectedQuery.FirstOrDefaultAsync();
        return result;
    }

    public async Task<Response.UpdateUserResponse> UpdateUserProfile(Request.UpdateUserRequest request)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        var userIdGuid = Guid.Parse(userId);

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);

        if (user == null)
            throw new Exception("User not found");

        user.FirstName = request.firstName ?? user.FirstName;
        user.LastName = request.lastName ?? user.LastName;
        user.Phone = request.phone ?? user.Phone;
        user.AvatarUrl = request.avatarUrl ?? user.AvatarUrl;
        
        await _dbContext.SaveChangesAsync();
        var result = new Response.UpdateUserResponse()
        {
            Id = user.Id,
            fullName = user.FirstName + " " + user.LastName,
            phone = user.Phone,
            avatarUrl = user.AvatarUrl,
        };
        return result;
    }

    public async Task<Response.ViewSetupResponse> ViewSetup()
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        var userIdGuid = Guid.Parse(userId);

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);

        if (user == null)
            throw new Exception("User not found");
        var query =  _dbContext.Accounts.Where(x => x.Id == userIdGuid);
        var selectedQuery = query.Select(x => new Response.ViewSetupResponse()
        {
            isOnboardingCompleted = x.IsOnboardingCompleted,
            monthlyIncome = x.OnboardingProfile.MonthlyIncome,
            budgetMethod  = x.OnboardingProfile.BudgetMethodPreference,
            defaultFinancialAccountId = x.FinancialAccounts.FirstOrDefault().Id,
            jarCount = x.JarSetup.Jars.Count,
            financialAccountCount = x.FinancialAccounts.Count,
            limitCount = _dbContext.SpendingLimits.Where(x => x.UserId == userIdGuid).Count(),
            activeGoalCount = _dbContext.Goals.Where(x => x.UserId == userIdGuid).Count(),
        });
        var result = await selectedQuery.FirstOrDefaultAsync();
        return result;
    }
}