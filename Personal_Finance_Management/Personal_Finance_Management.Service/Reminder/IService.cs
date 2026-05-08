namespace Personal_Finance_Management.Service.Reminder;

public interface IService
{
    public Task<Response.GetRemindersResponse> GetReminders();
    public Task<Response.ReminderResponse> CreateReminder(Request.CreateReminderRequest request);
    public Task<Response.ReminderActionResponse> UpdateReminder(Guid id, Request.UpdateReminderRequest request);
    public Task<Response.MessageResponse> DeleteReminder(Guid id);
}