using AuthRequest = Personal_Finance_Management.Service.Auth.Request;
using AdminRequest = Personal_Finance_Management.Service.Admin.Request;

namespace Personal_Finance_Management.Service.Validations;

public interface IServices
{
    Task<T> ValidateFormRequest<T>(T request);
    Task ValidateRegisterRequest(AuthRequest.RegisterRequest request);
    Task ValidateImportImageRequest(import.Request.ImportData request);
    Task ValidateAdminDashboardRequest(AdminRequest.AdminDashboardRequest request);
    Task ValidateAdminAuditLogsRequest(AdminRequest.AdminAuditLogsRequest request);
}
