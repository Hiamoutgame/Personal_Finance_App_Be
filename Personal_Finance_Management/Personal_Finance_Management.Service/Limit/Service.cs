using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Entity;


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
        var userId = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == "id")?.Value;

        if (!Guid.TryParse(userId, out var userIdGuid))
            throw new UnauthorizedAccessException("UserId not found in token");

        return userIdGuid;
    }


    
        public async Task<Response.GetLimitsResponse> GetLimits()
        {
            var userId = GetCurrentUserId();
            
            var limits = await _appDbContext.SpendingLimits
                .Include(l => l.Category)
                .Include(l => l.Jar)
                .Where(l => l.UserId == userId && l.IsActive)
                .ToListAsync();

            var jarIds = limits
                .Where(l => l.JarId.HasValue)
                .Select(l => l.JarId!.Value)
                .Distinct()
                .ToList();

            var spentByJarId = jarIds.Count == 0
                ? new Dictionary<Guid, decimal>()
                : await _appDbContext.Transactions
                    .Where(t => t.UserId == userId
                                && !t.IsDeleted
                                && t.Type == "Expense"
                                && t.FromJarId.HasValue
                                && jarIds.Contains(t.FromJarId.Value)
                                && t.ToJarId == null
                                && t.FinancialAccountId == null)
                    .GroupBy(t => t.FromJarId!.Value)
                    .Select(g => new
                    {
                        JarId = g.Key,
                        CurrentSpent = g.Sum(t => t.TransactionsAmount)
                    })
                    .ToDictionaryAsync(x => x.JarId, x => x.CurrentSpent);

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

                    currentSpent = spentByJarId.TryGetValue(limit.JarId.Value, out var spent)
                        ? spent
                        : 0m;
                }
                if (limit.Category != null && limit.CategoryId.HasValue)
                {
                    targetType = "Category";
                    targetId = limit.Category.Id;
                    targetName = limit.Category.Name;
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
                    CurrentPercentage = (double)((currentSpent * 100) / limit.LimitAmount),
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

       
        var limit = new SpendingLimit()
        {
            LimitAmount = request.LimitAmount,
            Period = request.Period,
            AlertAtPercentage = request.AlertAtPercentage,
            IsActive = true,
            UserId = userId,
            CategoryId = null,
            JarId = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        if (request.TargetType == "Category")
        {
            limit.CategoryId = request.TargetId;
        }

        if (request.TargetType == "Jar")
        {
            limit.JarId = request.TargetId;
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
            limit.LimitAmount = request.LimitAmount.Value;

        if (request.AlertAtPercentage.HasValue)
            limit.AlertAtPercentage = request.AlertAtPercentage.Value;
        
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
