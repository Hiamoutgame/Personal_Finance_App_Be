using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Enum;
using Personal_Finance_Management.Service.Base;
using Personal_Finance_Management.Service.Validations;

namespace Personal_Finance_Management.Service.Reminder;

public class Service : IService
{
    private readonly AppDbContext _appDbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServices _validationServices;

    public Service(
        AppDbContext appDbContext,
        IHttpContextAccessor httpContextAccessor,
        IServices validationServices)
    {
        _appDbContext = appDbContext;
        _httpContextAccessor = httpContextAccessor;
        _validationServices = validationServices;
    }
    private Guid GetCurrentUserId()
    {
        return ServiceClaimHelper.GetRequiredUserId(_httpContextAccessor);
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
        var now = DateTimeOffset.UtcNow;
        foreach (var r in remindersFromDb)
        {
            var calculatedNextDue = CalculateNextDueDate(
                r.StartDate,
                r.Frequency,
                r.DayOfMonth,
                r.NotifyDaysBefore,
                now);

            resultData.Add(new Response.ReminderResponse
            {
                Id = r.Id,
                Title = r.Title,
                Amount = r.Amount,
                Frequency = r.Frequency ?? string.Empty,
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
    await _validationServices.ValidateCreateReminderRequest(request);

    var userIdGuid = GetCurrentUserId();
    
    var now = DateTimeOffset.UtcNow; 
    
    var newReminder = new Repository.Entity.Reminder 
    {
        Id = Guid.NewGuid(),              
        UserId = userIdGuid,              
        Title = request.Title,            
        Amount = request.Amount,          
        Frequency = ServiceTextHelper.NormalizeEnum<ReminderFrequency>(request.Frequency),    
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
    
    var calculatedNextDue = CalculateNextDueDate(
        newReminder.StartDate,
        newReminder.Frequency,
        newReminder.DayOfMonth,
        newReminder.NotifyDaysBefore,
        DateTimeOffset.UtcNow);

    return new Response.ReminderResponse
    {
        Id = newReminder.Id,
        Title = newReminder.Title,
        Amount = newReminder.Amount,
        Frequency = newReminder.Frequency,
        NextDueDate = calculatedNextDue,
        Status = newReminder.Status
    };
}

    public async Task<Response.ReminderActionResponse> UpdateReminder(Guid id, Request.UpdateReminderRequest request)
{
    await _validationServices.ValidateUpdateReminderRequest(request);

    var userIdGuid = GetCurrentUserId();

    var reminder = await _appDbContext.Reminders
        .FirstOrDefaultAsync(r => r.Id == id 
                                  && r.UserId == userIdGuid);
    
    if (reminder == null)
    {
        throw AppValidationException.NotFound("Không tìm thấy nhắc nhở này.", "id", "REMINDER_NOT_FOUND");
    }
    
    if (request.Title != null) reminder.Title = request.Title;
    
    if (request.Amount.HasValue) reminder.Amount = request.Amount.Value;
    
    if (request.Frequency != null) reminder.Frequency = ServiceTextHelper.NormalizeEnum<ReminderFrequency>(request.Frequency);
    
    if (request.DayOfMonth.HasValue) reminder.DayOfMonth = (short?)request.DayOfMonth.Value; 
    
    if (request.Status != null) reminder.Status = ServiceTextHelper.NormalizeEnum<ReminderStatus>(request.Status);
    
    if (request.NotifyDaysBefore.HasValue) reminder.NotifyDaysBefore = (short)request.NotifyDaysBefore.Value; 
    
    if (request.Note != null) reminder.Note = request.Note;
    
    reminder.UpdatedAt = DateTimeOffset.UtcNow;
    
    await _appDbContext.SaveChangesAsync();

    
    var calculatedNextDue = CalculateNextDueDate(
        reminder.StartDate,
        reminder.Frequency,
        reminder.DayOfMonth,
        reminder.NotifyDaysBefore,
        DateTimeOffset.UtcNow);
    
    return new Response.ReminderActionResponse
    {
        Id = reminder.Id,
        Title = reminder.Title,
        Frequency = reminder.Frequency ?? string.Empty,
        NextDueDate = calculatedNextDue,
        Status = reminder.Status
    };
}

    public async Task<Response.MessageResponse> DeleteReminder(Guid id)
    {
        var userIdGuid = GetCurrentUserId();
        
        var reminder = await _appDbContext.Reminders
            .FirstOrDefaultAsync(r => r.Id == id 
                                      && r.UserId == userIdGuid);
        
        if (reminder == null)
        {
            throw AppValidationException.NotFound("Không tìm thấy nhắc nhở này.", "id", "REMINDER_NOT_FOUND");
        }
        
        var now = DateTimeOffset.UtcNow;
        reminder.Status = ReminderStatus.Cancelled.ToString();
    
        reminder.UpdatedAt = now;
        
        await _appDbContext.SaveChangesAsync();
        
        return new Response.MessageResponse
        {
            Message = "Reminder deleted"
        };
    }

    private static DateTimeOffset CalculateNextDueDate(
        DateTime startDate,
        string? frequency,
        short? dayOfMonth,
        short? notifyDaysBefore,
        DateTimeOffset now)
    {
        var normalizedFrequency = Enum.TryParse<ReminderFrequency>(
            frequency,
            ignoreCase: true,
            out var parsedFrequency)
            ? parsedFrequency
            : ReminderFrequency.Monthly;

        var dueDate = BuildDueDate(startDate, normalizedFrequency, dayOfMonth);
        var notifyOffset = Math.Max(0, (int)(notifyDaysBefore ?? 0));

        while (new DateTimeOffset(dueDate.AddDays(-notifyOffset), TimeSpan.Zero) < now)
        {
            dueDate = normalizedFrequency switch
            {
                ReminderFrequency.Daily => dueDate.AddDays(1),
                ReminderFrequency.Weekly => dueDate.AddDays(7),
                ReminderFrequency.Monthly => AddMonthsKeepingDay(dueDate, 1, dayOfMonth),
                ReminderFrequency.Quarterly => AddMonthsKeepingDay(dueDate, 3, dayOfMonth),
                ReminderFrequency.Yearly => AddYearsKeepingDay(dueDate, dayOfMonth),
                _ => dueDate.AddMonths(1)
            };
        }

        return new DateTimeOffset(dueDate.AddDays(-notifyOffset), TimeSpan.Zero);
    }

    private static DateTime BuildDueDate(
        DateTime startDate,
        ReminderFrequency frequency,
        short? dayOfMonth)
    {
        if (frequency is ReminderFrequency.Monthly or ReminderFrequency.Quarterly or ReminderFrequency.Yearly)
        {
            var day = dayOfMonth ?? (short)startDate.Day;
            return CreateDateKeepingDay(startDate.Year, startDate.Month, day);
        }

        return startDate.Date;
    }

    private static DateTime AddMonthsKeepingDay(DateTime date, int months, short? dayOfMonth)
    {
        var next = date.AddMonths(months);
        var day = dayOfMonth ?? (short)date.Day;
        return CreateDateKeepingDay(next.Year, next.Month, day);
    }

    private static DateTime AddYearsKeepingDay(DateTime date, short? dayOfMonth)
    {
        var next = date.AddYears(1);
        var day = dayOfMonth ?? (short)date.Day;
        return CreateDateKeepingDay(next.Year, next.Month, day);
    }

    private static DateTime CreateDateKeepingDay(int year, int month, short dayOfMonth)
    {
        var safeDay = Math.Min(dayOfMonth, (short)DateTime.DaysInMonth(year, month));
        return new DateTime(year, month, safeDay);
    }
}
