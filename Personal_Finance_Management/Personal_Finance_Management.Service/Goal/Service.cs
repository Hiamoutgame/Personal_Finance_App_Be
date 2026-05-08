using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Service.goal;

namespace Personal_Finance_Management.Service.Goal;

public class Service: IService
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
        {
            throw new UnauthorizedAccessException("UserId not found in token");
        }

        return userIdGuid;
    }
    public async Task<Response.GetGoalsResponse> GetGoals()
    {
        var userIdGuid = GetCurrentUserId();
        
        var goals = await _appDbContext.Goals
            .Where(g => g.UserId == userIdGuid && g.Status == "Active")
            .ToListAsync();
        
        var resultList = new List<Response.GetGoal>();
        var today = DateTimeOffset.UtcNow; 
        
        foreach (var goal in goals)
        {
            double progress = 0;
            if (goal.TargetAmount > 0)
            {
                progress = (double)(goal.SavedAmount / goal.TargetAmount) * 100;
                
            }
            
            decimal suggested = 0;
            decimal remainingAmount = goal.TargetAmount - goal.SavedAmount; 
            
            if (remainingAmount > 0)
            {
                int monthsRemaining = ((goal.DueDate.Year - today.Year) * 12) + goal.DueDate.Month - today.Month;
                
                if (monthsRemaining > 0)
                {
                    suggested = remainingAmount / monthsRemaining;
                }
                else
                {
                    suggested = remainingAmount;
                }
            }
            
            var item = new Response.GetGoal
            {
                Id = goal.Id,
                Title = goal.Title,
                TargetAmount = goal.TargetAmount,
                SavedAmount = goal.SavedAmount,
                ProgressPercentage = progress,
                DueDate = goal.DueDate, 
                Status = goal.Status,
                SuggestedMonthlyContribution = suggested, 
            };
            
            resultList.Add(item);
        }

 
        return new Response.GetGoalsResponse
        {
            Data = resultList
        };
    }

    public async Task<Response.GoalDetail> GetGoalById(Guid id)
    {
       ;
    }
    
    
}