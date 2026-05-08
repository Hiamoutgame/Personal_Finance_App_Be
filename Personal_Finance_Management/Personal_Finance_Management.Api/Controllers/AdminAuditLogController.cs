using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Api.Extensions;
using AdminService = Personal_Finance_Management.Service.Admin;

namespace Personal_Finance_Management.Api.Controllers
{
    [ApiController]
    [Route("api/v1/admin/audit-logs")]
    [Authorize(Policy = AuthorizationExtension.Policies.Admin)]
    public class AdminAuditLogController : ControllerBase
    {
        private readonly AdminService.IService _adminService;

        public AdminAuditLogController(AdminService.IService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AdminService.Request.AdminAuditLogsRequest request)
        {
            var result = await _adminService.GetAuditLogs(request);
            return Ok(result);
        }
    }
}
