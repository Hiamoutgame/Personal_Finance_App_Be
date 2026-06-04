using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Entity;
using Personal_Finance_Management.Service.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Personal_Finance_Management.Service.limit;

public class SpendingLimitEvaluator : ISpendingLimitEvaluator
{
    private readonly AppDbContext _dbContext;

    public SpendingLimitEvaluator(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EvaluateAsync(Guid userId)
    {
        var limits = await _dbContext.SpendingLimits
            .Include(l => l.Category)
            .Include(l => l.Jar)
            .Where(l => l.UserId == userId && l.IsActive)
            .ToListAsync();

        var now = DateTimeOffset.UtcNow;

        foreach (var limit in limits)
        {
            var periodStart = GetPeriodStart(limit.Period, now);
            var periodKey = GetPeriodKey(limit.Period, now);

            decimal currentSpent = 0m;
            string targetName = "Unknown";
            string targetType = "Jar";
            Guid targetId = Guid.Empty;

            if (limit.JarId.HasValue)
            {
                targetId = limit.JarId.Value;
                targetType = "Jar";
                targetName = limit.Jar?.Name ?? "Hu chi tiêu";

                currentSpent = await _dbContext.Transactions
                    .Where(t => t.UserId == userId
                                && !t.IsDeleted
                                && t.Type == TransactionType.Expense
                                && t.FromJarId == limit.JarId.Value
                                && t.ToJarId == null
                                && t.FinancialAccountId == null
                                && t.TransactionDate >= periodStart)
                    .SumAsync(t => (decimal?)t.TransactionsAmount) ?? 0m;
            }
            else if (limit.CategoryId.HasValue)
            {
                targetId = limit.CategoryId.Value;
                targetType = "Category";
                targetName = limit.Category?.Name ?? "Danh m?c";

                currentSpent = await _dbContext.Transactions
                    .Where(t => t.UserId == userId
                                && !t.IsDeleted
                                && t.Type == TransactionType.Expense
                                && t.CategoryId == limit.CategoryId.Value
                                && t.TransactionDate >= periodStart)
                    .SumAsync(t => (decimal?)t.TransactionsAmount) ?? 0m;
            }
            else
            {
                continue;
            }

            var alertThreshold = limit.LimitAmount * limit.AlertAtPercentage / 100m;

            if (currentSpent >= limit.LimitAmount)
            {
                // Threshold Exceeded
                var hasExceededNotif = await _dbContext.Notifications.AnyAsync(n =>
                    n.UserId == userId
                    && n.LimitId == limit.Id
                    && n.TargetType == targetType
                    && n.ThresholdType == "Exceeded"
                    && n.PeriodKey == periodKey);

                if (!hasExceededNotif)
                {
                    var title = "Thông báo vu?t ngu?ng!";
                    var body = targetType == "Jar"
                        ? $"Xin thông báo! b?n dã ch?m ngu?ng {limit.LimitAmount:N0}d gi?i h?n chi tiêu ? hu {targetName}"
                        : $"Ban da cham nguong {limit.LimitAmount:N0} gioi han chi tieu o danh muc {targetName}";

                    var notification = new Notification
                    {
                        UserId = userId,
                        Type = NotificationType.SpendingAlert,
                        Title = title,
                        Body = body,
                        IsRead = false,
                        CreatedAt = now,
                        LimitId = limit.Id,
                        TargetType = targetType,
                        ThresholdType = "Exceeded",
                        PeriodKey = periodKey,
                        MetadataJson = $"{{\"limitId\": \"{limit.Id}\", \"targetType\": \"{targetType}\", \"{(targetType == "Jar" ? "jarId" : "categoryId")}\": \"{targetId}\", \"thresholdType\": \"Exceeded\"}}"
                    };

                    _dbContext.Notifications.Add(notification);
                    limit.UpdatedAt = now;
                }
            }
            else if (currentSpent >= alertThreshold)
            {
                // Threshold Alert
                var hasAlertNotif = await _dbContext.Notifications.AnyAsync(n =>
                    n.UserId == userId
                    && n.LimitId == limit.Id
                    && n.TargetType == targetType
                    && n.ThresholdType == "Alert"
                    && n.PeriodKey == periodKey);

                if (!hasAlertNotif)
                {
                    var title = "Thông báo vu?t ngu?ng!";
                    var body = targetType == "Jar"
                        ? $"Xin thông báo! b?n dã ch?m ngu?ng thông báo {limit.AlertAtPercentage}% gi?i h?n chi tiêu ? hu {targetName}"
                        : $"Ban da cham nguong thong bao {limit.AlertAtPercentage}% gioi han chi tieu o danh muc {targetName}";

                    var notification = new Notification
                    {
                        UserId = userId,
                        Type = NotificationType.SpendingAlert,
                        Title = title,
                        Body = body,
                        IsRead = false,
                        CreatedAt = now,
                        LimitId = limit.Id,
                        TargetType = targetType,
                        ThresholdType = "Alert",
                        PeriodKey = periodKey,
                        MetadataJson = $"{{\"limitId\": \"{limit.Id}\", \"targetType\": \"{targetType}\", \"{(targetType == "Jar" ? "jarId" : "categoryId")}\": \"{targetId}\", \"thresholdType\": \"Alert\"}}"
                    };

                    _dbContext.Notifications.Add(notification);
                }
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    public static DateTimeOffset GetPeriodStart(string period, DateTimeOffset now)
    {
        return period switch
        {
            "Daily" => new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset),
            "Monthly" => new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset),
            _ => new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset)
        };
    }

    public static string GetPeriodKey(string period, DateTimeOffset now)
    {
        return period switch
        {
            "Daily" => now.ToString("yyyy-MM-dd"),
            "Monthly" => now.ToString("yyyy-MM"),
            _ => now.ToString("yyyy-MM")
        };
    }
}
