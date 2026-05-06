namespace Personal_Finance_Management.Service.Admin;

public interface IService
{
    Task<Response.AdminDashboardResponse> GetDashboard(string? timeframe);
    Task<Response.AdminAuditLogsResponse> GetAuditLogs(Request.AdminAuditLogsRequest request);
}
