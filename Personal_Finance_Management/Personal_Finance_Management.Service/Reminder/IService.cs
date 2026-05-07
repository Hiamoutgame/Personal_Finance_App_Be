namespace Personal_Finance_Management.Service.Reminder;

public interface IService
{
    Task<Response.GetRemindersResponse> GetReminders();
    Task<Response.ReminderResponse> CreateReminder(Request.CreateReminderRequest request);
    Task<Response.ReminderActionResponse> UpdateReminder(Guid id, Request.UpdateReminderRequest request);
    Task<Response.MessageResponse> DeleteReminder(Guid id);
}