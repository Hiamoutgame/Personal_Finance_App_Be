using Personal_Finance_Management.Service.baseServices;

namespace Personal_Finance_Management.Service.Admin;

public interface IService
{
    Task<Response.AdminDashboardResponse> GetDashboard(string? timeframe);
    Task<Page<Response.AdminAuditLogItem>> GetAuditLogs(Request.AdminAuditLogsRequest request);
}
