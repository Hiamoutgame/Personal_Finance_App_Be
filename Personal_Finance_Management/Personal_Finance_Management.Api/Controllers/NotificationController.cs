using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Service.notification; 

namespace Personal_Finance_Management.Api.Controllers;

[ApiController]
[Route("api/v1/notifications")] 
[Authorize]                  
public class NotificationController : ControllerBase
{
    private readonly IService _notificationService;

    
    public NotificationController(IService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications(string? type, string? status, int pageSize = 10, int pageIndex = 1) 
    {
        var result = await _notificationService.GetNotifications(type, status, pageSize, pageIndex);
        return Ok(result);
    }
    
    [HttpPatch("status")]
    public async Task<IActionResult> UpdateStatus([FromBody] Request.UpdateStatusRequest request)
    {
        var result = await _notificationService.UpdateStatus(request);
        return Ok(result);
    }
}