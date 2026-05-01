using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
        if (request == null)
        {
            throw new ArgumentException("Request cannot be null");
        }
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        var userIdGuid = Guid.Parse(userId);

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);

        if (user == null)
            throw new Exception("User not found");
        if (user.IsOnboardingCompleted == true)
        {
            throw new Exception("Onboarding is already completed");
        }
        var onboardingDetail = new Personal_Finance_Management.Repository.Entity.OnboardingProfile()
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
        _dbContext.OnboardingProfiles.Add(onboardingDetail);
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
                accountType = "Cash",
            }
        };
        var savedFinancialAccount = new Repository.Entity.FinancialAccount
        {
            UserId = userIdGuid,
            Name = response.defaultFinancialAccount.name,
            AccountType = response.defaultFinancialAccount.accountType,
            ConnectionMode = "Manual"
        };
        _dbContext.FinancialAccounts.Add(savedFinancialAccount);
        var savedCategory = response.recommendedCategories.Select(x => new Repository.Entity.Category()
        {
            OwnerUserId = userIdGuid,
            Name = x.name,
            Icon = x.icon,
            IsDefault = true,
        });
        _dbContext.Categories.AddRange(savedCategory);
        var savedJar = response.recommendedJars.Select(x => new Repository.Entity.Jar()
        {
            UserId = userIdGuid,
            Name = x.name,
            Percentage = x.percentage,
            IsDefault = true,
        });
        
        _dbContext.Jars.AddRange(savedJar);
        user.IsOnboardingCompleted = true;
        await _dbContext.SaveChangesAsync();
        return response;
    }
}
/*
    Đang add thêm các category và thêm các cái jar default cho người dùng
 */