using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Service.limit;

namespace Personal_Finance_Management.Api.Controllers;

[ApiController]
[Route("api/v1/limits")]
[Authorize]
public class LimitController : ControllerBase
{
    private readonly IService _limitService;

    public LimitController(IService limitService)
    {
        _limitService = limitService;
    }


    [HttpGet]
    public async Task<IActionResult> GetLimits()
    {
        var result = await _limitService.GetLimits();
        return Ok(result);
    }

 
    [HttpPost]
    public async Task<IActionResult> CreateLimit([FromBody] Request.CreateLimitRequest request)
    {
        var result = await _limitService.CreateLimit(request);
        return StatusCode(201, result);
    }

 
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateLimit(Guid id, [FromBody] Request.UpdateLimitRequest request)
    {
        var result = await _limitService.UpdateLimit(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLimit(Guid id)
    {
        await _limitService.DeleteLimit(id);
        return Ok(new { message = "Limit deleted" });
    }
}