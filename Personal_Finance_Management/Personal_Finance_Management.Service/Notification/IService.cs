namespace Personal_Finance_Management.Service.notification;

public interface IService
{
    public Task<Response.GetNotificationsResponse> GetNotifications(string? type, string? status, int page, int pageSize);
    public Task<Response.UpdateStatusResponse> UpdateStatus(Request.UpdateStatusRequest request);
}