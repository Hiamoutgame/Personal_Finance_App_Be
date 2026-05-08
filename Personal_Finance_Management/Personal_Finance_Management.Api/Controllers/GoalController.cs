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
}