using Microsoft.AspNetCore.Http;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Entity;

namespace Personal_Finance_Management.Service.Onboarding;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }

    public async Task<Response.OnboardingResponse> CreateOnboarding(Request.FillOnboardingRequest request)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        var userIdGuid = Guid.Parse(userId);

        var onboardingDetail = new Personal_Finance_Management.Repository.Entity.OnboardingProfile
        {
            UserId = userIdGuid,
            MonthlyIncome = request.monthlyIncome,
            OccupationType = request.occupationType,
            FinancialGoalTypes = request.financialGoalTypes,
            BudgetMethodPreference = request.budgetMethodPreference,
            AgeRange = request.ageRange,
            SpendingChallenges = request.spendingChallenges,
            RecommendedMethod = request.budgetMethodPreference,
        };
        _dbContext.Add(onboardingDetail);
        await _dbContext.SaveChangesAsync();
        
        
        var response = new Response.OnboardingResponse()
        {
            recommendedMethod = request.budgetMethodPreference,
            recommendedCategories = new List<Response.Category>()
            {
                new Response.Category()
                {
                    name = "Bills & Housing",
                    icon = "bill"
                },
                new Response.Category()
                {
                    name = "Food & Dining",
                    icon = "food"
                },
                new Response.Category()
                {
                    name = "Transportation",
                    icon = "car"
                },
                new Response.Category()
                {
                    name = "Shopping",
                    icon = "cart"
                },
                new Response.Category()
                {
                    name = "Entertainment",
                    icon = "game"
                },
                new Response.Category()
                {
                    name = "Health",
                    icon = "medicine"
                },
                new Response.Category()
                {
                    name = "Education",
                    icon = "book"
                },
                new Response.Category()
                {
                    name = "Savings & Investment",
                    icon = "bank"
                },
                new Response.Category()
                {
                    name = "Other",
                    icon = "?"
                }
            },
            recommendedJars = new List<Response.Jar>()
            {
                new Response.Jar()
                {
                    name = "Food & Dining",
                    percentage = 25
                },
                new Response.Jar()
                {
                    name = "Shopping",
                    percentage = 15
                },
                new Response.Jar()
                {
                    name = "Transportation",
                    percentage = 10
                },
                new Response.Jar()
                {
                    name = "Savings",
                    percentage = 20
                },
                new Response.Jar()
                {
                    name = "Essentials",
                    percentage = 20
                },
                new Response.Jar()
                {
                    name = "Entertainment",
                    percentage = 10
                }
            },
            defaultFinancialAccount = new Response.defaultFAccount()
            {
                name = "Cash",
                accountType =  "Cash",
            }
        };
        return response;
    }
}
/*
 * Vấn đề: Field hiện tại của FinancialGoalTypes và SpendingChallenges đã được em Nam sửa lại,
 * đúng với kiểu của nó là mảng string hoặc list<String> mà theo em nam đọc lỗi sau khi updateDb
 * thì em nam tìm hiểu được là nó đang bị conflict với các field cũ, em Nam cố gắng tạo cái Db trên máy
 * nhưng mà chưa được. --> Ngày hôm sau phải giải quyết xong vấn đề Db, Entity và test thành công.
 */