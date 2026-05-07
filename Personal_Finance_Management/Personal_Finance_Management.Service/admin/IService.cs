using Personal_Finance_Management.Repository.Enum;
using Personal_Finance_Management.Service.baseServices;

namespace Personal_Finance_Management.Service.Admin;

public interface IService
{
    Task<Response.AdminDashboardResponse> GetDashboard();
    Task<Page<Response.AdminAuditLogItem>> GetAuditLogs(Request.AdminAuditLogsRequest request);
    Task<string> UpdateRole(Guid userId, AccountRole role);
}
