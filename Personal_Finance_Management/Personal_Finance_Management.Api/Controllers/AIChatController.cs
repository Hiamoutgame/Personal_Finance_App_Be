using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Api.Extensions;
using Personal_Finance_Management.Service.AI;

namespace Personal_Finance_Management.Api.Controllers;

[ApiController]
[Route("api/v1/ai")]
[Authorize(Policy = AuthorizationExtension.Policies.User)]
public class AIChatController : ControllerBase
{
    private readonly IService _aiService;

    public AIChatController(IService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] Request.ChatBoxRequest request)
    {
        var result = await _aiService.ChatBot(request);
        return Ok(result);
    }
}
