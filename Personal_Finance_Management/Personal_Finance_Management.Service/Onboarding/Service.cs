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
        var now = DateTimeOffset.UtcNow;
        var onboardingDetail = new Personal_Finance_Management.Repository.Entity.OnboardingProfile()
        {
            UserId = userIdGuid,
            MonthlyIncome = request.monthlyIncome,
            OccupationType = request.occupationType,
            FinancialGoalTypes = request.financialGoalTypes is null
                ? null
                : string.Join(",", request.financialGoalTypes),
            BudgetMethodPreference = request.budgetMethodPreference ?? "Undecided",
            AgeRange = request.ageRange,
            SpendingChallenges = request.spendingChallenges is null
                ? null
                : string.Join(",", request.spendingChallenges),
            RecommendedMethod = request.budgetMethodPreference,
            CompletedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };
        _dbContext.OnboardingProfiles.Add(onboardingDetail);
        var response = new Response.OnboardingResponse();
        if (request.budgetMethodPreference == "SixJars")
        {
            var result = new Response.OnboardingResponse()
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
                        name = "Food & Dining"
                    },
                    new Response.Jar()
                    {
                        name = "Shopping"
                    },
                    new Response.Jar()
                    {
                        name = "Transportation"
                    },
                    new Response.Jar()
                    {
                        name = "Savings"
                    },
                    new Response.Jar()
                    {
                        name = "Essentials"
                    },
                    new Response.Jar()
                    {
                        name = "Entertainment"
                    }
                },
                defaultFinancialAccount = new Response.defaultFAccount()
                {
                    name = "Cash",
                    accountType = "Cash",
                }
            };
            response = result;
        }else if (request.budgetMethodPreference == "Rule503020")
        {
            var result = new Response.OnboardingResponse()
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
                        name = "Needs"
                    },
                    new Response.Jar()
                    {
                        name = "Wants"
                    },
                    new Response.Jar()
                    {
                        name = "Savings/Investments"
                    }
                },
                defaultFinancialAccount = new Response.defaultFAccount()
                {
                    name = "Cash",
                    accountType = "Cash",
                }
            };
            response = result;
        }else if (request.budgetMethodPreference == "Custom")
        {
            var result = new Response.OnboardingResponse()
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
                recommendedJars = null,
                defaultFinancialAccount = new Response.defaultFAccount()
                {
                    name = "Cash",
                    accountType = "Cash",
                }
            };
            response = result;
        }
        else
        {
            throw new Exception("budgetMethodPreference is invalid");
        }
        var savedFinancialAccount = new Repository.Entity.FinancialAccount
        {
            UserId = userIdGuid,
            Name = response.defaultFinancialAccount.name,
            AccountType = response.defaultFinancialAccount.accountType,
            ConnectionMode = "Manual",
            Currency = "VND",
            CurrentBalance = 0m,
            IsDefault = true,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        _dbContext.FinancialAccounts.Add(savedFinancialAccount);
        var savedCategory = response.recommendedCategories.Select(x => new Repository.Entity.Category()
        {
            OwnerUserId = userIdGuid,
            Name = x.name,
            Icon = x.icon,
            IsDefault = false,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        });
        _dbContext.Categories.AddRange(savedCategory);

        var jarSetup = new Repository.Entity.JarSetup()
        {
            UserId = userIdGuid,
            MethodType = request.budgetMethodPreference,
        };
        _dbContext.JarSetups.Add(jarSetup);
        await _dbContext.SaveChangesAsync();
        if(request.budgetMethodPreference != "Custom")
        {
            var savedJar = response.recommendedJars.Select(x => new Repository.Entity.Jar()
            {
                UserId = userIdGuid,
                Name = x.name,
                IsDefault = true,
                Balance = 0m,
                Currency = "VND",
                Status = "Active",
                JarSetupId = jarSetup.Id,
                CreatedAt = now,
                UpdatedAt = now
            });
            _dbContext.Jars.AddRange(savedJar);
        }
        user.IsOnboardingCompleted = true;
        await _dbContext.SaveChangesAsync();
        return response;
    }
}
/*
    Đang add thêm các category và thêm các cái jar default cho người dùng
 */
