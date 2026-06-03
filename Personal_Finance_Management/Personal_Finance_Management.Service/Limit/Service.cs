using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Entity;
using Personal_Finance_Management.Service.Base;
using Personal_Finance_Management.Service.Common.Constants;
using Personal_Finance_Management.Service.Common.Enums;
using Personal_Finance_Management.Service.Validations;


namespace Personal_Finance_Management.Service.limit;

public class Service : IService
{
    private readonly AppDbContext _appDbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext appDbContext, IHttpContextAccessor httpContextAccessor)
    {
        _appDbContext = appDbContext;
        _httpContextAccessor = httpContextAccessor;
    }
    
    private Guid GetCurrentUserId()
    {
        return ServiceClaimHelper.GetRequiredUserId(_httpContextAccessor);
    }


    
    public async Task<Response.GetLimitsResponse> GetLimits()
    {
        var userId = GetCurrentUserId();
        
        var limits = await _appDbContext.SpendingLimits
            .Include(l => l.Category)
            .Include(l => l.Jar)
            .Where(l => l.UserId == userId && l.IsActive)
            .ToListAsync();

        var items = new List<Response.GetLimitItem>();

        foreach (var limit in limits)
        {
            string targetName = "Unknow";
            Guid targetId = Guid.Empty;
            string targetType = "Jar";

            decimal currentSpent = 0m;
            
            if (limit.Jar != null && limit.JarId.HasValue)
            {
                targetId = limit.JarId.Value;
                targetName = limit.Jar.Name;
                targetType = "Jar";

                currentSpent = await _appDbContext.Transactions
                    .Where(t => t.UserId == userId
                                && !t.IsDeleted
                                && t.Type == TransactionType.Expense
                                && t.FromJarId == limit.JarId.Value
                                && t.ToJarId == null
                                && t.FinancialAccountId == null
                                && t.TransactionDate >= limit.ResetAt)
                    .SumAsync(t => (decimal?)t.TransactionsAmount) ?? 0m;
            }
            if (limit.Category != null && limit.CategoryId.HasValue)
            {
                targetType = "Category";
                targetId = limit.Category.Id;
                targetName = limit.Category.Name;

                currentSpent = await _appDbContext.Transactions
                    .Where(t => t.UserId == userId
                                && !t.IsDeleted
                                && t.Type == TransactionType.Expense
                                && t.CategoryId == limit.CategoryId.Value
                                && t.TransactionDate >= limit.ResetAt)
                    .SumAsync(t => (decimal?)t.TransactionsAmount) ?? 0m;
            }
            var item = new Response.GetLimitItem
            {
                Id = limit.Id,
                TargetId = targetId,
                TargetName = targetName,
                LimitAmount = limit.LimitAmount,
                Period = limit.Period,
                AlertAtPercentage = limit.AlertAtPercentage,
                CurrentSpent = currentSpent,
                CurrentPercentage = limit.LimitAmount <= 0
                    ? 0
                    : (double)((currentSpent * 100) / limit.LimitAmount),
                Status = "Active",
                TargetType = targetType
            };

            items.Add(item);
        }
        return new Response.GetLimitsResponse { Data = items };
    }

    public async Task<Response.CreateLimitResponse> CreateLimit(Request.CreateLimitRequest request)
    {
        var userId = GetCurrentUserId();
        if (request.LimitAmount <= 0)
        {
            throw AppValidationException.BadRequest(ErrorMessages.InvalidLimitAmount, "limitAmount", ErrorCodes.InvalidLimitAmount);
        }

        if (request.AlertAtPercentage <= 0 || request.AlertAtPercentage > 100)
        {
            throw AppValidationException.BadRequest(ErrorMessages.InvalidAlertPercentage, "alertAtPercentage", ErrorCodes.InvalidAlertPercentage);
        }

        var now = DateTimeOffset.UtcNow;
        var limit = new SpendingLimit()
        {
            LimitAmount = request.LimitAmount,
            Period = request.Period,
            AlertAtPercentage = request.AlertAtPercentage,
            IsActive = true,
            UserId = userId,
            CategoryId = null,
            JarId = null,
            CreatedAt = now,
            UpdatedAt = now,
            ResetAt = now,
        };
        if (request.TargetType == "Category")
        {
            var categoryExists = await _appDbContext.Categories.AnyAsync(x =>
                x.Id == request.TargetId
                && x.IsActive
                && (x.OwnerUserId == null || x.OwnerUserId == userId));
            if (!categoryExists)
            {
                throw AppValidationException.NotFound("Category not found.", "targetId", ErrorCodes.CategoryNotFound);
            }

            limit.CategoryId = request.TargetId;
        }

        if (request.TargetType == "Jar")
        {
            var jarExists = await _appDbContext.Jars.AnyAsync(x =>
                x.Id == request.TargetId
                && x.UserId == userId);
            if (!jarExists)
            {
                throw AppValidationException.NotFound("Jar not found.", "targetId", ErrorCodes.JarNotFound);
            }

            limit.JarId = request.TargetId;
        }

        if (limit.CategoryId == null && limit.JarId == null)
        {
            throw AppValidationException.BadRequest(ErrorMessages.InvalidLimitTargetType, "targetType", ErrorCodes.InvalidLimitTargetType);
        }
        
        _appDbContext.SpendingLimits.Add(limit);
        await _appDbContext.SaveChangesAsync();
        
        return new Response.CreateLimitResponse
        {
            Id = limit.Id,
            LimitAmount = limit.LimitAmount,
            Period = limit.Period,
            AlertAtPercentage = limit.AlertAtPercentage,
            TargetType = request.TargetType,
            TargetId = request.TargetId,
        };
    }

    public async Task<Response.UpdateLimitResponse> UpdateLimit(Guid id, Request.UpdateLimitRequest request)
    {
        var userId = GetCurrentUserId();
        
        var limit = await _appDbContext.SpendingLimits
            .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);

        if (limit == null)
            throw new ("Limit not found");
        
        if (request.LimitAmount.HasValue)
        {
            if (request.LimitAmount.Value <= 0)
            {
                throw AppValidationException.BadRequest(ErrorMessages.InvalidLimitAmount, "limitAmount", ErrorCodes.InvalidLimitAmount);
            }
            limit.LimitAmount = request.LimitAmount.Value;
        }

        if (request.AlertAtPercentage.HasValue)
        {
            if (request.AlertAtPercentage.Value <= 0 || request.AlertAtPercentage.Value > 100)
            {
                throw AppValidationException.BadRequest(ErrorMessages.InvalidAlertPercentage, "alertAtPercentage", ErrorCodes.InvalidAlertPercentage);
            }
            limit.AlertAtPercentage = request.AlertAtPercentage.Value;
        }
        
        await _appDbContext.SaveChangesAsync();
        
        return new Response.UpdateLimitResponse
        {
            Id = limit.Id,
            LimitAmount = limit.LimitAmount,
            AlertAtPercentage = limit.AlertAtPercentage
        };
    }
    
    public async Task DeleteLimit(Guid id)
    {
        var userId = GetCurrentUserId();
        
        var limit = await _appDbContext.SpendingLimits
            .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);

        if (limit == null)
            throw new KeyNotFoundException("Limit not found");
        
        limit.IsActive = false;
        await _appDbContext.SaveChangesAsync();
    }
}
