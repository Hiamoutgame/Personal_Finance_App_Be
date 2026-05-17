using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Entity;
using Personal_Finance_Management.Service.Base;
using Personal_Finance_Management.Service.Validations;

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
        return ServiceClaimHelper.GetRequiredUserId(_httpContextAccessor);
    }
    
    public async Task<Response.GetGoalsResponse> GetGoals()
    {
        var userId = GetCurrentUserId();
        
        var goals = await _appDbContext.Goals
            .Include(g => g.LinkedJar)
            .Where(g => g.UserId == userId && (g.Status == "Active" || g.Status == "Completed"))
            .OrderBy(g => g.Title)
            .ToListAsync();
        
        var today = DateTime.UtcNow;
        var items = new List<Response.GetGoalItem>();

        foreach (var goal in goals)
        {
            decimal savedAmount = goal.LinkedJar?.Balance ?? 0;
            double progress = 0;
            if (goal.TargetAmount > 0)
                progress = (double)(savedAmount / goal.TargetAmount) * 100;

            decimal suggested = TinhSoTienGoiYMoiThang(goal, savedAmount, today);

            items.Add(new Response.GetGoalItem
            {
                Id = goal.Id,
                Title = goal.Title,
                TargetAmount = goal.TargetAmount,
                SavedAmount = savedAmount,
                ProgressPercentage = Math.Round(progress, 1),
                DueDate = goal.DueDate,
                Status = goal.Status,
                SuggestedMonthlyContribution = suggested,
                LinkedJarId = goal.LinkedJarId,
                LinkedJarName = goal.LinkedJar?.Name
            });
        }

        return new Response.GetGoalsResponse { Data = items };
    }

    public async Task<Response.GetGoalByIdResponse> GetGoalById(Guid id)
    {
        var userId = GetCurrentUserId();
        var today = DateTime.UtcNow;
        
        var goal = await _appDbContext.Goals
            .Include(g => g.LinkedJar)
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

        if (goal == null)
            throw new Exception("Goal not found");
        
        decimal savedAmount = goal.LinkedJar?.Balance ?? 0;
        double progress = 0;
        if (goal.TargetAmount > 0)
            progress = (double)(savedAmount / goal.TargetAmount) * 100;
        
        int daysRemaining = (int)(goal.DueDate - today).TotalDays;
        if (daysRemaining < 0) daysRemaining = 0;
        
        decimal suggested = TinhSoTienGoiYMoiThang(goal, savedAmount, today);

        return new Response.GetGoalByIdResponse
        {
            Id = goal.Id,
            Title = goal.Title,
            TargetAmount = goal.TargetAmount,
            SavedAmount = savedAmount,
            ProgressPercentage = Math.Round(progress, 1),
            DueDate = goal.DueDate,
            DaysRemaining = daysRemaining,
            Status = goal.Status,
            SuggestedMonthlyContribution = suggested,
            LinkedJarId = goal.LinkedJarId,
            LinkedJarName = goal.LinkedJar?.Name,
            Note = goal.Note
        };
    }

    public async Task<Response.CreateGoalResponse> CreateGoal(Request.CreateGoalRequest request)
    {
        var userId = GetCurrentUserId();
        ValidateGoalInput(request.Title, request.TargetAmount, request.DueDate);

        var linkedJar = request.LinkedJarId.HasValue
            ? await _appDbContext.Jars.FirstOrDefaultAsync(x => x.Id == request.LinkedJarId.Value && x.UserId == userId)
            : null;

        if (request.LinkedJarId.HasValue && linkedJar == null)
        {
            throw AppValidationException.NotFound("Jar not found.", "linkedJarId", "JAR_NOT_FOUND");
        }

        var now = DateTimeOffset.UtcNow;

        var goal = new Goal
        {
            Title = request.Title,
            TargetAmount = request.TargetAmount,
            SavedAmount = linkedJar?.Balance ?? 0,            
            DueDate = request.DueDate,
            Status = "Active",          
            Note = request.Note,
            UserId = userId,
            LinkedJarId = request.LinkedJarId,
            CreatedAt = now,
            UpdatedAt = now
        };

        _appDbContext.Goals.Add(goal);
        await _appDbContext.SaveChangesAsync();
        if (linkedJar != null && linkedJar.Balance >= request.TargetAmount)
        {
            goal.Status = "Completed";
            goal.UpdatedAt = DateTimeOffset.UtcNow;

            // Tạo thông báo cho người dùng
            var notification = new Notification
            {
                UserId = goal.UserId,
                Type = "GoalUpdate",
                Title = "Chúc mừng! Bạn đã hoàn thành mục tiêu",
                Body = $"Mục tiêu '{goal.Title}' đã đạt được mức {goal.TargetAmount:N0}đ trong hũ {linkedJar.Name}.",
                IsRead = false,
                CreatedAt = DateTimeOffset.UtcNow,
                MetadataJson = $"{{\"goalId\": \"{goal.Id}\", \"jarId\": \"{linkedJar.Id}\"}}"
            };

            _appDbContext.Notifications.Add(notification);
            await _appDbContext.SaveChangesAsync();
        }
        // Get saved amount from jar if linked
        decimal savedAmount = 0;
        if (goal.LinkedJarId.HasValue)
        {
            var jar = await _appDbContext.Jars.FindAsync(goal.LinkedJarId.Value);
            savedAmount = jar?.Balance ?? 0;
        }

        double progress = 0;
        if (goal.TargetAmount > 0)
            progress = (double)(savedAmount / goal.TargetAmount) * 100;

        return new Response.CreateGoalResponse
        {
            Id = goal.Id,
            Title = goal.Title,
            TargetAmount = goal.TargetAmount,
            SavedAmount = savedAmount,
            ProgressPercentage = Math.Round(progress, 1),     
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
            throw new ("Goal not found");
        
        if (request.Title != null && string.IsNullOrWhiteSpace(request.Title))
        {
            throw AppValidationException.BadRequest("Goal title is required.", "title", "GOAL_TITLE_REQUIRED");
        }

        if (request.TargetAmount.HasValue && request.TargetAmount.Value <= 0)
        {
            throw AppValidationException.BadRequest("Target amount must be greater than zero.", "targetAmount", "INVALID_GOAL_TARGET_AMOUNT");
        }

        if (request.DueDate.HasValue && request.DueDate.Value.Date < DateTime.UtcNow.Date)
        {
            throw AppValidationException.BadRequest("Goal due date cannot be in the past.", "dueDate", "GOAL_DUE_DATE_IN_PAST");
        }

        if (request.LinkedJarId.HasValue)
        {
            var jarExists = await _appDbContext.Jars.AnyAsync(x => x.Id == request.LinkedJarId.Value && x.UserId == userId);
            if (!jarExists)
            {
                throw AppValidationException.NotFound("Jar not found.", "linkedJarId", "JAR_NOT_FOUND");
            }
        }

        if (request.Title != null) goal.Title = request.Title;
        if (request.TargetAmount.HasValue) goal.TargetAmount = request.TargetAmount.Value;
        if (request.DueDate.HasValue) goal.DueDate = request.DueDate.Value;
        if (request.LinkedJarId != null) goal.LinkedJarId = request.LinkedJarId;
        if (request.Note != null) goal.Note = request.Note;
        goal.UpdatedAt = DateTimeOffset.UtcNow;
        
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
            throw new ("Goal not found");
        
        goal.Status = "Cancelled";
        await _appDbContext.SaveChangesAsync();
    }
    
    private static decimal TinhSoTienGoiYMoiThang(Goal goal, decimal currentSaved, DateTime today)
    {
        decimal conThieu = goal.TargetAmount - currentSaved;

        if (conThieu <= 0) return 0;

        int soThangConLai = ((goal.DueDate.Year - today.Year) * 12)
                            + (goal.DueDate.Month - today.Month);
        
        if (soThangConLai <= 0) return conThieu;

        return conThieu / soThangConLai;
    }

    private static void ValidateGoalInput(string title, decimal targetAmount, DateTime dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw AppValidationException.BadRequest("Goal title is required.", "title", "GOAL_TITLE_REQUIRED");
        }

        if (targetAmount <= 0)
        {
            throw AppValidationException.BadRequest("Target amount must be greater than zero.", "targetAmount", "INVALID_GOAL_TARGET_AMOUNT");
        }

        if (dueDate.Date < DateTime.UtcNow.Date)
        {
            throw AppValidationException.BadRequest("Goal due date cannot be in the past.", "dueDate", "GOAL_DUE_DATE_IN_PAST");
        }
    }
}
