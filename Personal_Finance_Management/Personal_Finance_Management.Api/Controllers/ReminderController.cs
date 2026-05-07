using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Service.Reminder;

namespace Personal_Finance_Management.Api.Controllers;

[ApiController]
[Route("api/v1/reminders")]
[Authorize]
public class ReminderController: ControllerBase
{
    
    private readonly IService _reminderService;

    public ReminderController(IService reminderService)
    {
        _reminderService = reminderService;
    }
    [HttpGet]
    public async Task<IActionResult> GetReminders()
    {
        var result = await _reminderService.GetReminders();
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult>  CreateReminder(Request.CreateReminderRequest request)
    {
        var result = await _reminderService.CreateReminder(request);
        return Ok(result);
    }
    
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateReminder(Guid id, [FromBody] Request.UpdateReminderRequest request)
    {
        var result = await _reminderService.UpdateReminder(id, request);
        return Ok(result);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReminder(Guid id)
    {
        var result = await _reminderService.DeleteReminder(id);
        return Ok(result);
    }
}