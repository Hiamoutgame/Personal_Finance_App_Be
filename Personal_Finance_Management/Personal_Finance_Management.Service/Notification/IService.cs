namespace Personal_Finance_Management.Service.notification;

public interface IService
{
    
    Task<Response.GetNotificationsResponse> GetNotifications(string? type, string? status, int page, int pageSize);
    
    Task<Response.UpdateStatusResponse> UpdateStatus(Request.UpdateStatusRequest request);
}