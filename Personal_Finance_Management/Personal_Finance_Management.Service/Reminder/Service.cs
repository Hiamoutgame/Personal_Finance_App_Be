using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;

namespace Personal_Finance_Management.Service.Reminder;

public class Service : IService
{
    private readonly AppDbContext _appDbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(
        AppDbContext appDbContext,
        IHttpContextAccessor httpContextAccessor)
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
    public async Task<Response.GetRemindersResponse> GetReminders()
    {
        var userIdGuid = GetCurrentUserId();
        var remindersFromDb = await _appDbContext.Reminders
            .AsNoTracking()
            .Where(r => r.UserId == userIdGuid && r.Status == "Active")
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
        
        var resultData = new List<Response.ReminderResponse>();
        var now = DateTime.Now;
        foreach (var r in remindersFromDb)
        {
            DateTime calculatedNextDue = r.StartDate; 
            
            if (r.Frequency == "Monthly" && r.DayOfMonth.HasValue)
            {
                int year = now.Year;
                int month = now.Month;
                int day = r.DayOfMonth.Value;
                
                int maxDaysInMonth = DateTime.DaysInMonth(year, month);
                if (day > maxDaysInMonth)
                {
                    day = maxDaysInMonth;
                }

                DateTime dateOfThisMonth = new DateTime(year, month, day);
                
                if (dateOfThisMonth < now.Date)
                {
                    calculatedNextDue = dateOfThisMonth.AddMonths(1);
                }
                else
                {
                    calculatedNextDue = dateOfThisMonth;
                }
            }
            resultData.Add(new Response.ReminderResponse
            {
                Id = r.Id,
                Title = r.Title,
                Amount = r.Amount,
                Frequency = r.Frequency,
                NextDueDate = calculatedNextDue,
                Status = r.Status
            });
        }
        
        return new Response.GetRemindersResponse
        {
            Data = resultData
        };
    }

    public async Task<Response.ReminderResponse> CreateReminder(Request.CreateReminderRequest request)
{
    if (request == null)
    {
        throw new("request is null");
    }

    var userIdGuid = GetCurrentUserId();
    
    var now = DateTimeOffset.UtcNow; 
    
    var newReminder = new Repository.Entity.Reminder 
    {
        Id = Guid.NewGuid(),              
        UserId = userIdGuid,              
        Title = request.Title,            
        Amount = request.Amount,          
        Frequency = request.Frequency,    
        DayOfMonth = request.DayOfMonth, 
        StartDate = request.StartDate.DateTime, 
        CategoryId = request.CategoryId,
        NotifyDaysBefore = request.NotifyDaysBefore, 
        Note = request.Note,
        Status = "Active",                
        CreatedAt = now,
        UpdatedAt = now
    };
    
    _appDbContext.Reminders.Add(newReminder);
    await _appDbContext.SaveChangesAsync();
    
    DateTime calculatedNextDue = request.StartDate.DateTime;
    
    if (request.Frequency == "Monthly" && request.DayOfMonth.HasValue)
    {
        
        DateTime nextMonthDate = calculatedNextDue.AddMonths(1);
        
        int year = nextMonthDate.Year;
        int month = nextMonthDate.Month;
    
        int day = request.DayOfMonth.Value;
        int maxDays = DateTime.DaysInMonth(year, month);
        if (day > maxDays)
        {
            day = maxDays;
        }
        calculatedNextDue = new DateTime(year, month, day);
    }

    return new Response.ReminderResponse
    {
        Id = newReminder.Id,
        Title = newReminder.Title,
        Frequency = newReminder.Frequency,
        NextDueDate = calculatedNextDue,
        Status = newReminder.Status
    };
}

    public Task<Response.ReminderActionResponse> UpdateReminder(Guid id, Request.UpdateReminderRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<Response.MessageResponse> DeleteReminder(Guid id)
    {
        throw new NotImplementedException();
    }
}