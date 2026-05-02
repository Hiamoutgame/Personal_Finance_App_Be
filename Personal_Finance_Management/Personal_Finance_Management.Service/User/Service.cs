using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Service.Validations;

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
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        var userIdGuid = Guid.Parse(userId);

        var query = _dbContext.Accounts.Where(x => x.Id == userIdGuid);
        var selectedQuery = query.Select(x => new Response.GetUserInforResponse()
        {
            Id = x.Id,
            userName = x.Username,
            firstName = x.FirstName,
            lastName = x.LastName,
            email = x.Email,
            phone = x.Phone,
            avatarUrl = x.AvatarUrl,
            preferredCurrency = x.PreferredCurrency,
            isOnboardingCompleted = x.IsOnboardingCompleted
        });
        var result = await selectedQuery.FirstOrDefaultAsync();
        return result ?? throw new Exception("User not found");
    }

    public async Task<Response.UpdateUserResponse> UpdateUserProfile(Request.UpdateUserRequest request)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        var userIdGuid = Guid.Parse(userId);

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);

        if (user == null)
            throw new Exception("User not found");

        if (request.firstName is not null)
        {
            var firstName = request.firstName.Trim();
            if (string.IsNullOrWhiteSpace(firstName))
                throw AppValidationException.BadRequest("First name is required.", "firstName", "REQUIRED");

            user.FirstName = firstName;
        }

        if (request.lastName is not null)
        {
            var lastName = request.lastName.Trim();
            if (string.IsNullOrWhiteSpace(lastName))
                throw AppValidationException.BadRequest("Last name is required.", "lastName", "REQUIRED");

            user.LastName = lastName;
        }

        user.Phone = request.phone?.Trim() ?? user.Phone;
        user.AvatarUrl = request.avatarUrl?.Trim() ?? user.AvatarUrl;

        await _dbContext.SaveChangesAsync();
        var result = new Response.UpdateUserResponse()
        {
            Id = user.Id,
            firstName = user.FirstName,
            lastName = user.LastName,
            phone = user.Phone,
            avatarUrl = user.AvatarUrl,
        };
        return result;
    }

    public async Task<Response.ViewSetupResponse> ViewSetup()
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        var userIdGuid = Guid.Parse(userId);

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);

        if (user == null)
            throw new Exception("User not found");
        var selectedQuery = _dbContext.Accounts
            .Where(x => x.Id == userIdGuid)
            .Select(x => new Response.ViewSetupResponse()
        {
            isOnboardingCompleted = x.IsOnboardingCompleted,
            monthlyIncome = x.OnboardingProfile == null ? null : x.OnboardingProfile.MonthlyIncome,
            budgetMethod = x.OnboardingProfile == null
                ? "Undecided"
                : x.OnboardingProfile.BudgetMethodPreference ?? "Undecided",
            defaultFinancialAccountId = x.FinancialAccounts
                .Where(f => f.IsDefault)
                .Select(f => (Guid?)f.Id)
                .FirstOrDefault(),
            jarCount = _dbContext.Jars.Where(x => x.UserId == userIdGuid).Count(),
            financialAccountCount = _dbContext.FinancialAccounts.Where(x => x.UserId == userIdGuid).Count(),
            limitCount = _dbContext.SpendingLimits.Where(x => x.UserId == userIdGuid).Count(),
            activeGoalCount = _dbContext.Goals.Where(x => x.UserId == userIdGuid).Count(),
        });
        var result = await selectedQuery.FirstOrDefaultAsync();
        return result ?? throw new Exception("User not found");
    }
}
