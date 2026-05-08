using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Entity;

namespace Personal_Finance_Management.Service.goal;

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
    
    public async Task<Response.GetGoalsResponse> GetGoals()
    {
        var userId = GetCurrentUserId();
        
        var goals = await _appDbContext.Goals
            .Where(g => g.UserId == userId && g.Status == "Active")
            .OrderBy(g => g.Title)
            .ToListAsync();
        
        var today = DateTime.UtcNow;
        var items = new List<Response.GetGoalItem>();

        foreach (var goal in goals)
        {
            // Tính % tiến độ: đã tiết kiệm được á / mục tiêu * 100 ra tỉ lệ %
            double progress = 0;
            if (goal.TargetAmount > 0)
                progress = (double)(goal.SavedAmount / goal.TargetAmount) * 100;

            // Tính số tiền cần đóng mỗi tháng để đạt mục tiêu đúng hạn 
            //Cái này sài lại nên tách riêng ra để tính luôn 
            //Mà tôi rối quá rồi nên để tiếng việt cho đỡ lộn
            decimal suggested = TinhSoTienGoiYMoiThang(goal, today);

            items.Add(new Response.GetGoalItem
            {
                Id = goal.Id,
                Title = goal.Title,
                TargetAmount = goal.TargetAmount,
                SavedAmount = goal.SavedAmount,
                ProgressPercentage = progress,
                DueDate = goal.DueDate,
                Status = goal.Status,
                SuggestedMonthlyContribution = suggested
            });
        }

        return new Response.GetGoalsResponse { Data = items };
    }

    
    public async Task<Response.GetGoalByIdResponse> GetGoalById(Guid id)
    {
        var userId = GetCurrentUserId();
        var today = DateTime.UtcNow;
        
        var goal = await _appDbContext.Goals
            .Include(g => g.Contributions)
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

        if (goal == null)
            throw new Exception("Goal not found");
        
        double progress = 0;
        if (goal.TargetAmount > 0)
            progress = (double)(goal.SavedAmount / goal.TargetAmount) * 100;
        
        int daysRemaining = (int)(goal.DueDate - today).TotalDays;
        if (daysRemaining < 0) daysRemaining = 0;
        
        decimal suggested = TinhSoTienGoiYMoiThang(goal, today);

        // 5. Lấy 5 lần đóng góp gần nhất
        var recentContributions = goal.Contributions
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .Select(c => new Response.RecentContributionItem
            {
                Id = c.Id,
                Amount = c.Amount,
                Date = c.CreatedAt
            })
            .ToList();

        return new Response.GetGoalByIdResponse
        {
            Id = goal.Id,
            Title = goal.Title,
            TargetAmount = goal.TargetAmount,
            SavedAmount = goal.SavedAmount,
            ProgressPercentage = Math.Round(progress, 1),
            DueDate = goal.DueDate,
            DaysRemaining = daysRemaining,
            Status = goal.Status,
            SuggestedMonthlyContribution = suggested,
            LinkedJarId = goal.LinkedJarId,
            RecentContributions = recentContributions
        };
    }

    
    public async Task<Response.CreateGoalResponse> CreateGoal(Request.CreateGoalRequest request)
    {
        var userId = GetCurrentUserId();

        
        var goal = new Goal
        {
            Title = request.Title,
            TargetAmount = request.TargetAmount,
            SavedAmount = 0,            
            DueDate = request.DueDate,
            Status = "Active",          
            Note = request.Note,
            UserId = userId,
            LinkedJarId = request.LinkedJarId
        };

       
        _appDbContext.Goals.Add(goal);
        await _appDbContext.SaveChangesAsync();

      
        return new Response.CreateGoalResponse
        {
            Id = goal.Id,
            Title = goal.Title,
            TargetAmount = goal.TargetAmount,
            SavedAmount = 0,
            ProgressPercentage = 0,     
            Status = goal.Status,
            DueDate = goal.DueDate
        };
    }
    
    public async Task<Response.UpdateGoalResponse> UpdateGoal(Guid id, Request.UpdateGoalRequest request)
    {
        var userId = GetCurrentUserId();
        
        var goal = await _appDbContext.Goals
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

        if (goal == null)
            throw new KeyNotFoundException("Goal not found");
        
        if (request.Title != null) goal.Title = request.Title;
        if (request.TargetAmount.HasValue) goal.TargetAmount = request.TargetAmount.Value;
        if (request.DueDate.HasValue) goal.DueDate = request.DueDate.Value;
        if (request.LinkedJarId != null) goal.LinkedJarId = request.LinkedJarId;
        if (request.Note != null) goal.Note = request.Note;
        
        await _appDbContext.SaveChangesAsync();

        
        return new Response.UpdateGoalResponse
        {
            Id = goal.Id,
            Title = goal.Title,
            TargetAmount = goal.TargetAmount,
            DueDate = goal.DueDate,
            Status = goal.Status
        };
    }
    
    public async Task DeleteGoal(Guid id)
    {
        var userId = GetCurrentUserId();

       
        var goal = await _appDbContext.Goals
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

        if (goal == null)
            throw new KeyNotFoundException("Goal not found");
        
        goal.Status = "Deleted";
        await _appDbContext.SaveChangesAsync();
        
    }
    
    private static decimal TinhSoTienGoiYMoiThang(Goal goal, DateTime today)
    {
        decimal conThieu = goal.TargetAmount - goal.SavedAmount;

       //Nó trừ ra âm là mik để giành vượt mức
        if (conThieu <= 0) return 0;

        int soThangConLai = ((goal.DueDate.Year - today.Year) * 12)
                            + (goal.DueDate.Month - today.Month);
        
        if (soThangConLai <= 0) return conThieu;

        return conThieu / soThangConLai;
    }
}