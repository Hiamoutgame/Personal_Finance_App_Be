using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Entity;
using Personal_Finance_Management.Repository.Enum;
using Personal_Finance_Management.Service.baseServices;
using Personal_Finance_Management.Service.Validations;

namespace Personal_Finance_Management.Service.Admin;

public class Service : IService
{
    private const int TopCategoryLimit = 4;
    private const int RecentLimit = 10;

    private readonly AppDbContext _dbContext;
    private readonly IServices _validationServices;

    public Service(
        AppDbContext dbContext,
        IServices validationServices)
    {
        _dbContext = dbContext;
        _validationServices = validationServices;
    }

    public async Task<Response.AdminDashboardResponse> GetDashboard(string? timeframe)
    {
        var request = new Request.AdminDashboardRequest { Timeframe = timeframe };
        await _validationServices.ValidateAdminDashboardRequest(request);

        var normalizedTimeframe = DashboardTimeframe.Normalize(request.Timeframe);
        var now = DateTimeOffset.UtcNow;
        var dateRange = DashboardDateRangeFactory.Create(normalizedTimeframe, now);

        var userQuery = _dbContext.Accounts.AsNoTracking()
            .RegularUsers();

        var totalUsers = await userQuery.CountAsync();
        var previousTotalUsers = await userQuery
            .Where(account => account.CreatedAt < dateRange.PeriodStart)
            .CountAsync();
        var deltaPercent = CalculatePercentDelta(totalUsers, previousTotalUsers);

        var dayStart = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero);
        var dayEnd = dayStart.AddDays(1);
        var dau = await userQuery
            .Where(account => account.LastLoginAt != null
                && account.LastLoginAt >= dayStart
                && account.LastLoginAt < dayEnd)
            .CountAsync();
        var mau = await userQuery
            .Where(account => account.LastLoginAt != null
                && account.LastLoginAt >= now.AddDays(-30))
            .CountAsync();
        var stickinessPercent = CalculatePercent(dau, mau);

        var transactionPeriodQuery = _dbContext.Transactions.AsNoTracking()
            .ActiveTransactions()
            .InDateRange(dateRange.PeriodStart, dateRange.PeriodEnd);

        var totalTransactions = await transactionPeriodQuery.CountAsync();
        var totalTransactionValue = await transactionPeriodQuery
            .Select(transaction => Math.Abs(transaction.TransactionsAmount))
            .DefaultIfEmpty(0m)
            .SumAsync();

        var transactionTrendQuery = _dbContext.Transactions.AsNoTracking()
            .ActiveTransactions()
            .InDateRange(dateRange.Trend.Start, dateRange.Trend.End);

        var transactionTrend = await BuildTransactionTrend(
            transactionTrendQuery,
            dateRange.Trend,
            normalizedTimeframe);
        var topSpendingCategories = await GetTopSpendingCategories(
            dateRange.PeriodStart,
            dateRange.PeriodEnd);
        var retentionTrend = await BuildRetentionTrend();

        var recentUsers = await userQuery
            .OrderByDescending(account => account.CreatedAt)
            .Take(RecentLimit)
            .Select(account => new Response.RecentUserItem
            {
                Id = account.Id,
                FullName = (account.FirstName + " " + account.LastName).Trim(),
                Email = account.Email,
                Status = account.Status,
                CreatedAt = account.CreatedAt
            })
            .ToListAsync();

        var recentTransactions = await _dbContext.Transactions.AsNoTracking()
            .ActiveTransactions()
            .OrderByDescending(transaction => transaction.TransactionDate)
            .Take(RecentLimit)
            .Select(transaction => new Response.RecentTransactionItem
            {
                Id = transaction.Id,
                Type = transaction.Type,
                Amount = Math.Abs(transaction.TransactionsAmount),
                Note = transaction.Note,
                TransactionDate = transaction.TransactionDate
            })
            .ToListAsync();

        var bannedUsers = await userQuery
            .Where(account => account.Status == AccountStatus.Banned.ToString())
            .CountAsync();
        var totalJobs = await _dbContext.ImportJobs.AsNoTracking().CountAsync();
        var failedJobs = await _dbContext.ImportJobs.AsNoTracking()
            .Where(job => job.Status == ImportJobStatus.Failed.ToString())
            .CountAsync();
        var errorRatePercent = totalJobs == 0
            ? 0
            : Math.Round(failedJobs * 100m / totalJobs, 2, MidpointRounding.AwayFromZero);
        var systemHealthStatus = GetSystemHealthStatus(errorRatePercent);

        return new Response.AdminDashboardResponse
        {
            StatCards =
            [
                new Response.StatCard
                {
                    Type = "total_users",
                    Label = "Total users",
                    Value = totalUsers,
                    DeltaPercent = deltaPercent
                },
                new Response.StatCard
                {
                    Type = "engagement",
                    Label = "Engagement (DAU/MAU)",
                    Dau = dau,
                    Mau = mau,
                    StickinessPercent = stickinessPercent
                },
                new Response.StatCard
                {
                    Type = "transactions",
                    Label = "Total transactions",
                    TotalTransactionValue = totalTransactionValue,
                    TotalTransactions = totalTransactions
                },
                new Response.StatCard
                {
                    Type = "system_health",
                    Label = "System health",
                    ErrorRatePercent = errorRatePercent,
                    Status = systemHealthStatus,
                    BannedUsers = bannedUsers
                }
            ],
            TransactionVolumeTrend = transactionTrend,
            TopSpendingCategories = topSpendingCategories,
            RetentionTrend = retentionTrend,
            RecentUsers = recentUsers,
            RecentTransactions = recentTransactions
        };
    }

    public async Task<Page<Response.AdminAuditLogItem>> GetAuditLogs(Request.AdminAuditLogsRequest request)
    {
        await _validationServices.ValidateAdminAuditLogsRequest(request);

        var adminRoleCode = AccountRole.Admin.ToString();
        var query = _dbContext.AuditLogs.AsNoTracking()
            .Where(log => log.Account.Role.Code == adminRoleCode);

        if (request.AdminId.HasValue)
        {
            query = query.Where(log => log.ActorAccountId == request.AdminId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.ActionType))
        {
            var actionType = request.ActionType.Trim();
            query = query.Where(log => log.ActionType == actionType);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            var entityType = request.EntityType.Trim();
            query = query.Where(log => log.EntityType == entityType);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(log => log.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(log => log.CreatedAt <= request.ToDate.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(log => log.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(log => new Response.AdminAuditLogItem
            {
                Id = log.Id,
                AdminUsername = log.Account.Username,
                ActionType = log.ActionType,
                EntityType = log.EntityType,
                Description = log.Description,
                CreatedAt = log.CreatedAt
            })
            .ToListAsync();

        return new Page<Response.AdminAuditLogItem>
        {
            Items = items,
            Pagination = new PaginationMetadata
            {
                PageIndex = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            }
        };
    }

    private async Task<List<Response.TransactionTrendItem>> BuildTransactionTrend(
        IQueryable<Transaction> transactionQuery,
        TrendRange trendRange,
        string timeframe)
    {
        if (timeframe == DashboardTimeframe.Day)
        {
            var dailyData = await transactionQuery
                .GroupBy(transaction => transaction.TransactionDate.Date)
                .Select(group => new
                {
                    Date = group.Key,
                    Amount = group.Sum(item => Math.Abs(item.TransactionsAmount)),
                    Count = group.Count()
                })
                .ToListAsync();

            var dailyMap = dailyData.ToDictionary(item => item.Date, item => item);

            return trendRange.Buckets
                .Select(bucket =>
                {
                    var key = bucket.Start.UtcDateTime.Date;
                    if (dailyMap.TryGetValue(key, out var value))
                    {
                        return new Response.TransactionTrendItem
                        {
                            Label = bucket.Label,
                            Amount = value.Amount,
                            Count = value.Count
                        };
                    }

                    return new Response.TransactionTrendItem
                    {
                        Label = bucket.Label,
                        Amount = 0,
                        Count = 0
                    };
                })
                .ToList();
        }

        if (timeframe == DashboardTimeframe.Month)
        {
            var monthlyData = await transactionQuery
                .GroupBy(transaction => new { transaction.TransactionDate.Year, transaction.TransactionDate.Month })
                .Select(group => new
                {
                    group.Key.Year,
                    group.Key.Month,
                    Amount = group.Sum(item => Math.Abs(item.TransactionsAmount)),
                    Count = group.Count()
                })
                .ToListAsync();

            var monthlyMap = monthlyData
                .ToDictionary(item => (item.Year, item.Month), item => item);

            return trendRange.Buckets
                .Select(bucket =>
                {
                    var key = (bucket.Start.Year, bucket.Start.Month);
                    if (monthlyMap.TryGetValue(key, out var value))
                    {
                        return new Response.TransactionTrendItem
                        {
                            Label = bucket.Label,
                            Amount = value.Amount,
                            Count = value.Count
                        };
                    }

                    return new Response.TransactionTrendItem
                    {
                        Label = bucket.Label,
                        Amount = 0,
                        Count = 0
                    };
                })
                .ToList();
        }

        var yearlyData = await transactionQuery
            .GroupBy(transaction => transaction.TransactionDate.Year)
            .Select(group => new
            {
                Year = group.Key,
                Amount = group.Sum(item => Math.Abs(item.TransactionsAmount)),
                Count = group.Count()
            })
            .ToListAsync();

        var yearlyMap = yearlyData.ToDictionary(item => item.Year, item => item);

        return trendRange.Buckets
            .Select(bucket =>
            {
                var key = bucket.Start.Year;
                if (yearlyMap.TryGetValue(key, out var value))
                {
                    return new Response.TransactionTrendItem
                    {
                        Label = bucket.Label,
                        Amount = value.Amount,
                        Count = value.Count
                    };
                }

                return new Response.TransactionTrendItem
                {
                    Label = bucket.Label,
                    Amount = 0,
                    Count = 0
                };
            })
            .ToList();
    }

    private async Task<List<Response.TopCategoryItem>> GetTopSpendingCategories(
        DateTimeOffset start,
        DateTimeOffset end)
    {
        var expenseQuery = _dbContext.Transactions.AsNoTracking()
            .ActiveTransactions()
            .Expenses()
            .InDateRange(start, end);

        var grouped = await expenseQuery
            .GroupBy(transaction => transaction.Category != null ? transaction.Category.Name : null)
            .Select(group => new
            {
                CategoryName = group.Key,
                Amount = group.Sum(item => Math.Abs(item.TransactionsAmount))
            })
            .ToListAsync();

        var totalAmount = grouped.Sum(item => item.Amount);

        var topCategories = grouped
            .Where(item => item.CategoryName != null)
            .OrderByDescending(item => item.Amount)
            .Take(TopCategoryLimit)
            .Select(item => new Response.TopCategoryItem
            {
                Label = item.CategoryName!,
                Value = item.Amount
            })
            .ToList();

        var topAmount = topCategories.Sum(item => item.Value);
        var remainder = totalAmount - topAmount;

        if (remainder > 0)
        {
            topCategories.Add(new Response.TopCategoryItem
            {
                Label = "Khac",
                Value = remainder
            });
        }

        return topCategories;
    }

    private async Task<List<Response.RetentionTrendItem>> BuildRetentionTrend()
    {
        var periods = new[] { 0, 7, 14, 21, 30 };
        var cohorts = new[]
        {
            new CohortDefinition(
                "A",
                new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)),
            new CohortDefinition(
                "B",
                new DateTimeOffset(2025, 4, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2025, 7, 1, 0, 0, 0, TimeSpan.Zero)),
            new CohortDefinition(
                "C",
                new DateTimeOffset(2025, 7, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero)),
            new CohortDefinition(
                "D",
                new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))
        };

        var cohortResults = new Dictionary<string, int[]>();

        foreach (var cohort in cohorts)
        {
            var profiles = await _dbContext.OnboardingProfiles.AsNoTracking()
                .Where(profile => profile.CompletedAt >= cohort.Start
                    && profile.CompletedAt < cohort.End)
                .Select(profile => new { profile.CompletedAt, profile.User.LastLoginAt })
                .ToListAsync();

            var total = profiles.Count;
            var percents = new int[periods.Length];

            for (var i = 0; i < periods.Length; i++)
            {
                var days = periods[i];
                if (days == 0)
                {
                    percents[i] = total > 0 ? 100 : 0;
                    continue;
                }

                var active = profiles.Count(profile => profile.LastLoginAt.HasValue
                    && profile.LastLoginAt.Value >= profile.CompletedAt.AddDays(days));
                percents[i] = CalculatePercent(active, total);
            }

            cohortResults[cohort.Code] = percents;
        }

        var result = new List<Response.RetentionTrendItem>();
        for (var i = 0; i < periods.Length; i++)
        {
            result.Add(new Response.RetentionTrendItem
            {
                PeriodLabel = $"D{periods[i]}",
                CohortA = cohortResults["A"][i],
                CohortB = cohortResults["B"][i],
                CohortC = cohortResults["C"][i],
                CohortD = cohortResults["D"][i]
            });
        }

        return result;
    }

    private static int CalculatePercent(int part, int total)
    {
        if (total <= 0)
        {
            return 0;
        }

        return (int)Math.Round((decimal)part / total * 100, MidpointRounding.AwayFromZero);
    }

    private static decimal CalculatePercentDelta(int current, int previous)
    {
        if (previous <= 0)
        {
            return 0;
        }

        return Math.Round(
            (decimal)(current - previous) / previous * 100,
            2,
            MidpointRounding.AwayFromZero);
    }

    private static string GetSystemHealthStatus(decimal errorRatePercent)
    {
        if (errorRatePercent >= 20)
        {
            return "Bad";
        }

        if (errorRatePercent >= 5)
        {
            return "Warning";
        }

        return "Good";
    }

    private sealed record CohortDefinition(
        string Code,
        DateTimeOffset Start,
        DateTimeOffset End);
}
