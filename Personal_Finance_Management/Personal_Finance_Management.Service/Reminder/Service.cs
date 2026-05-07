// using Microsoft.AspNetCore.Http;
// using Microsoft.EntityFrameworkCore;
// using Personal_Finance_Management.Repository;
// using Personal_Finance_Management.Service.Validations;
//
// namespace Personal_Finance_Management.Service.Reminder;
//
// public class Service : IService
// {
//     private readonly AppDbContext _appDbContext;
//     private readonly IHttpContextAccessor _httpContextAccessor;
//
//     public Service(
//         AppDbContext appDbContext,
//         IHttpContextAccessor httpContextAccessor)
//     {
//         _appDbContext = appDbContext;
//         _httpContextAccessor = httpContextAccessor;
//     }
//
//     private Guid GetCurrentUserId()
//     {
//         var userId = _httpContextAccessor.HttpContext
//             .User.Claims
//             .FirstOrDefault(x => x.Type == "id")
//             ?.Value;
//
//         if (!Guid.TryParse(userId, out var userIdGuid))
//         {
//             throw new UnauthorizedAccessException(
//                 "UserId not found in token");
//         }
//
//         return userIdGuid;
//     }
//     public async Task<Response.GetRemindersResponse> GetReminders()
//     {
//         var userIdGuid = GetCurrentUserId();
//
//         var reminders = await _appDbContext.Reminders
//             .AsNoTracking()
//             .Where(r =>
//                 r.UserId == userIdGuid &&
//                 r.IsActive &&
//                 r.DeletedAt == null)
//             .OrderBy(r => r.NextDueDate)
//             .Select(r => new Response.ReminderResponse
//             {
//                 Id = r.Id,
//                 Title = r.Title,
//                 Amount = r.Amount,
//                 Frequency = r.Frequency,
//                 NextDueDate = r.NextDueDate,
//                 Status = r.Status
//             })
//             .ToListAsync();
//
//         return new Response.GetRemindersResponse
//         {
//             Data = reminders
//         };
//     }
//
//     public async Task<Response.ReminderActionResponse> CreateReminder(
//         Request.CreateReminderRequest request)
//     {
//         if (request is null)
//         {
//             throw AppValidationException.BadRequest(
//                 "Request body is required.",
//                 "body",
//                 "REQUIRED");
//         }
//
//         var userIdGuid = GetCurrentUserId();
//
//         var now = DateTimeOffset.UtcNow;
//
//         var reminder = new Repository.Entity.Reminder
//         {
//             Id = Guid.NewGuid(),
//             UserId = userIdGuid,
//             Title = request.Title.Trim(),
//             Amount = request.Amount,
//             Frequency = request.Frequency.Trim(),
//             DayOfMonth = request.DayOfMonth,
//             StartDate = request.StartDate,
//             NextDueDate = request.StartDate,
//             CategoryId = request.CategoryId,
//             NotifyDaysBefore = request.NotifyDaysBefore,
//             Note = request.Note,
//             Status = "Active",
//             IsActive = true,
//             CreatedAt = now,
//             UpdatedAt = now
//         };
//
//         _appDbContext.Reminders.Add(reminder);
//
//         await _appDbContext.SaveChangesAsync();
//
//         return MapReminderAction(reminder);
//     }
//
//     public async Task<Response.ReminderActionResponse> UpdateReminder(
//         Guid id,
//         Request.UpdateReminderRequest request)
//     {
//         if (request is null)
//         {
//             throw AppValidationException.BadRequest(
//                 "Request body is required.",
//                 "body",
//                 "REQUIRED");
//         }
//
//         var userIdGuid = GetCurrentUserId();
//
//         var reminder = await GetReminderOrThrow(id, userIdGuid);
//
//         if (request.Title is not null)
//         {
//             reminder.Title = request.Title.Trim();
//         }
//
//         if (request.Amount.HasValue)
//         {
//             reminder.Amount = request.Amount.Value;
//         }
//
//         if (request.Frequency is not null)
//         {
//             reminder.Frequency = request.Frequency.Trim();
//         }
//
//         if (request.DayOfMonth.HasValue)
//         {
//             reminder.DayOfMonth = request.DayOfMonth.Value;
//         }
//
//         if (request.Status is not null)
//         {
//             reminder.Status = request.Status;
//         }
//
//         if (request.NotifyDaysBefore.HasValue)
//         {
//             reminder.NotifyDaysBefore = request.NotifyDaysBefore.Value;
//         }
//
//         if (request.Note is not null)
//         {
//             reminder.Note = request.Note;
//         }
//
//         reminder.UpdatedAt = DateTimeOffset.UtcNow;
//
//         await _appDbContext.SaveChangesAsync();
//
//         return MapReminderAction(reminder);
//     }
//
//     public async Task<Response.MessageResponse> DeleteReminder(Guid id)
//     {
//         var userIdGuid = GetCurrentUserId();
//
//         var reminder = await GetReminderOrThrow(id, userIdGuid);
//
//         var now = DateTimeOffset.UtcNow;
//
//         reminder.IsActive = false;
//         reminder.DeletedAt = now;
//         reminder.UpdatedAt = now;
//
//         await _appDbContext.SaveChangesAsync();
//
//         return new Response.MessageResponse
//         {
//             Message = "Reminder deleted"
//         };
//     }
//
//     private async Task<Repository.Entity.Reminder> GetReminderOrThrow(
//         Guid id,
//         Guid userId)
//     {
//         var reminder = await _appDbContext.Reminders
//             .FirstOrDefaultAsync(r =>
//                 r.Id == id &&
//                 r.UserId == userId &&
//                 r.IsActive &&
//                 r.DeletedAt == null);
//
//         if (reminder is null)
//         {
//             throw AppValidationException.NotFound(
//                 "Reminder not found.",
//                 "id",
//                 "REMINDER_NOT_FOUND");
//         }
//
//         return reminder;
//     }
//
//     
//
//     private static Response.ReminderActionResponse MapReminderAction(
//         Repository.Entity.Reminder reminder)
//     {
//         return new Response.ReminderActionResponse
//         {
//             Id = reminder.Id,
//             Title = reminder.Title,
//             Frequency = reminder.Frequency,
//             NextDueDate = reminder.NextDueDate,
//             Status = reminder.Status
//         };
//     }
// }