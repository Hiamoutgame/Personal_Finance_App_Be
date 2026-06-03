using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Constants;
using Personal_Finance_Management.Repository.Entity;
using Personal_Finance_Management.Repository.Enum;
using Personal_Finance_Management.Service.Base;
using Personal_Finance_Management.Service.Common.Constants;
using Personal_Finance_Management.Service.Validations;
using BaseResponse = Personal_Finance_Management.Service.Base.Response;

namespace Personal_Finance_Management.Service.User;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IServices _validationServices;

    public Service(
        AppDbContext dbContext,
        IHttpContextAccessor httpContext,
        IServices validationServices)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
        _validationServices = validationServices;
    }
    public async Task<Response.GetUserInforResponse> GetUserInfor()
    {
        var userIdGuid = GetCurrentUserId();

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

    public async Task<BaseResponse.PagedResponse<Response.AdminUserResponse>> GetAdminUsers(Request.GetAdminUsersRequest request)
    {
        await _validationServices.ValidateAdminUsersRequest(request);

        var query = _dbContext.Accounts
            .AsNoTracking()
            .Where(x => x.Role.Code == AppRoles.Codes.User);

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = ServiceTextHelper.NormalizeEnum<AccountStatus>(request.Status);
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();
            query = query.Where(x =>
                x.Username.ToLower().Contains(keyword)
                || x.Email.ToLower().Contains(keyword)
                || x.FirstName.ToLower().Contains(keyword)
                || x.LastName.ToLower().Contains(keyword));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new Response.AdminUserResponse
            {
                Id = x.Id,
                UserName = x.Username,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                Phone = x.Phone,
                AvatarUrl = x.AvatarUrl,
                PreferredCurrency = x.PreferredCurrency,
                IsOnboardingCompleted = x.IsOnboardingCompleted,
                Status = x.Status,
                StatusReason = x.StatusReason,
                CreatedAt = x.CreatedAt,
                LastLoginAt = x.LastLoginAt
            })
            .ToListAsync();

        return new BaseResponse.PagedResponse<Response.AdminUserResponse>
        {
            Data = users,
            Pagination = new BaseResponse.PaginationResponse
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                TotalCount = totalCount
            }
        };
    }

    public async Task<Response.AdminUserResponse> GetUserInforById(Request.UserIdRequest request)
    {
        var user = await _dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.UserId && x.Role.Code == AppRoles.Codes.User);

        return user is null
            ? throw AppValidationException.NotFound("User not found.", "id", ErrorCodes.UserNotFound)
            : ToAdminUserResponse(user);
    }




    public async Task<Response.UpdateUserResponse> UpdateUserProfile(Request.UpdateUserRequest request)
    {
        var userIdGuid = GetCurrentUserId();

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);

        if (user == null)
            throw new Exception("User not found");

        user.FirstName = request.FirstName ?? user.FirstName;
        user.LastName = request.LastName ?? user.LastName;
        user.Phone = request.Phone ?? user.Phone;
        user.AvatarUrl = request.AvatarUrl ?? user.AvatarUrl;

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

    public async Task<Response.AdminUserResponse> UpdateUserStatus(Request.UserStatusRequest request)
    {
        await _validationServices.ValidateAdminUserStatusRequest(request);

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == request.UserId && x.Role.Code == AppRoles.Codes.User);

        if (user == null)
            throw AppValidationException.NotFound("User not found.", "id", ErrorCodes.UserNotFound);

        user.Status = ServiceTextHelper.NormalizeEnum<AccountStatus>(request.Status!);
        user.StatusReason = request.StatusReason;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
        return ToAdminUserResponse(user);
    }

    public async Task<Response.ViewSetupResponse> ViewSetup()
    {
        var userIdGuid = GetCurrentUserId();

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
            budgetMethod = x.OnboardingProfile == null ? "Undecided" : x.OnboardingProfile.BudgetMethodPreference,
            defaultFinancialAccountId = x.FinancialAccounts
                .Where(account => account.IsActive)
                .OrderByDescending(account => account.IsDefault)
                .Select(account => (Guid?)account.Id)
                .FirstOrDefault(),
            jarCount = _dbContext.Jars.Where(x => x.UserId == userIdGuid).Count(),
            financialAccountCount = _dbContext.FinancialAccounts.Where(x => x.UserId == userIdGuid).Count(),
            limitCount = _dbContext.SpendingLimits.Where(x => x.UserId == userIdGuid).Count(),
            activeGoalCount = _dbContext.Goals.Where(x => x.UserId == userIdGuid).Count(),
        });
        var result = await selectedQuery.FirstOrDefaultAsync();
        return result ?? throw new Exception("User not found");
    }

    private static Response.AdminUserResponse ToAdminUserResponse(Account user)
    {
        return new Response.AdminUserResponse
        {
            Id = user.Id,
            UserName = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            AvatarUrl = user.AvatarUrl,
            PreferredCurrency = user.PreferredCurrency,
            IsOnboardingCompleted = user.IsOnboardingCompleted,
            Status = user.Status,
            StatusReason = user.StatusReason,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    private Guid GetCurrentUserId()
    {
        return ServiceClaimHelper.GetRequiredUserId(_httpContext);
    }
}
