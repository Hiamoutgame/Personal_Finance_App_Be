using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Service.Dashboard;

namespace Personal_Finance_Management.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IService _service;
    public DashboardController(IService service)
    {
        _service = service;
    }

    [HttpGet("/user/dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _service.GetDashboard();
        return Ok(result);
    }
    
}