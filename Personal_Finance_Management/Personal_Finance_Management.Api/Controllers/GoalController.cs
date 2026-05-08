using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Service.goal;

namespace Personal_Finance_Management.Api.Controllers;

[ApiController]
[Route("api/v1/goals")]
[Authorize]
public class GoalController : ControllerBase
{
    private readonly IService _goalService;

    public GoalController(IService goalService)
    {
        _goalService = goalService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetGoals()
    {
        var result = await _goalService.GetGoals();
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetGoalById(Guid id)
    {
        var result = await _goalService.GetGoalById(id);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateGoal([FromBody] Request.CreateGoalRequest request)
    {
        var result = await _goalService.CreateGoal(request);
        return StatusCode(201, result);
    }
    
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateGoal(Guid id, [FromBody] Request.UpdateGoalRequest request)
    {
        var result = await _goalService.UpdateGoal(id, request);
        return Ok(result);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGoal(Guid id)
    {
        await _goalService.DeleteGoal(id);
        return Ok(new { message = "Goal deleted" });
    }
}