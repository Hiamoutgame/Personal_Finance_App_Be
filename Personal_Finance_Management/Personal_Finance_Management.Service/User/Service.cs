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
            UserName = x.Username,
            FirstName = x.FirstName,
            LastName = x.LastName,
            Email = x.Email,
            Phone = x.Phone,
            AvatarUrl = x.AvatarUrl,
            PreferredCurrency = x.PreferredCurrency,
            IsOnboardingCompleted = x.IsOnboardingCompleted
        });
        var result = await selectedQuery.FirstOrDefaultAsync();
        return result ?? throw new Exception("User not found");
    }

    public async Task<Response.GetUserInforResponse> GetUserInforById(Request.UserIdRequest request)
    {
        var query = _dbContext.Accounts.Where(x => x.Id == request.UserId);
        var selectedQuery = query.Select(x => new Response.GetUserInforResponse()
        {
            Id = x.Id,
            UserName = x.Username,
            FirstName = x.FirstName,
            LastName = x.LastName,
            Email = x.Email,
            Phone = x.Phone,
            AvatarUrl = x.AvatarUrl,
            PreferredCurrency = x.PreferredCurrency,
            IsOnboardingCompleted = x.IsOnboardingCompleted
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

        if (request.FirstName is not null)
        {
            var firstName = request.FirstName.Trim();
            if (string.IsNullOrWhiteSpace(firstName))
                throw AppValidationException.BadRequest("First name is required.", "firstName", "REQUIRED");

            user.FirstName = firstName;
        }

        if (request.LastName is not null)
        {
            var lastName = request.LastName.Trim();
            if (string.IsNullOrWhiteSpace(lastName))
                throw AppValidationException.BadRequest("Last name is required.", "lastName", "REQUIRED");

            user.LastName = lastName;
        }

        user.Phone = request.Phone?.Trim() ?? user.Phone;
        user.AvatarUrl = request.AvatarUrl?.Trim() ?? user.AvatarUrl;

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

    public async Task<Response.GetUserInforResponse> UpdateUserStatus(Request.UserStatusRequest request)
    {
        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == request.UserId);

        if (user == null)
            throw new Exception("User not found");

        user.Status = user.Status == "Banned" ? "Active" : "Banned";
        user.StatusReason = user.Status == "Banned" ? "User account has been banned." : "User account has been reactivated.";

        await _dbContext.SaveChangesAsync();
        return new Response.GetUserInforResponse()
        {
            Id = user.Id,
            UserName = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            AvatarUrl = user.AvatarUrl,
            PreferredCurrency = user.PreferredCurrency,
            IsOnboardingCompleted = user.IsOnboardingCompleted
        };
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
