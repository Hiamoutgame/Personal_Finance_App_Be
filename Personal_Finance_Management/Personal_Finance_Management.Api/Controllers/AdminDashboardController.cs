using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Api.Extensions;
using Personal_Finance_Management.Service.Admin;

namespace Personal_Finance_Management.Api.Controllers
{
    [ApiController]
    [Route("api/v1/admin/dashboard")]
    [Authorize(Policy = AuthorizationExtension.Policies.Admin)]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IService _adminService;

        public AdminDashboardController(IService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard([FromQuery] string? timeframe = null)
        {
            var result = await _adminService.GetDashboard(timeframe);
            return Ok(result);
        }
    }
}
