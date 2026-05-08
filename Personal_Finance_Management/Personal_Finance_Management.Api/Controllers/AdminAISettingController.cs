using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Api.Extensions;
using Personal_Finance_Management.Service.AI;

namespace Personal_Finance_Management.Api.Controllers;
[ApiController]
[Route("api/v1/admin/ai-settings")]
[Authorize(Policy = AuthorizationExtension.Policies.Admin)]
public class AdminAISettingController: ControllerBase
{
    private readonly Service.AI.IService _aiService;

    public AdminAISettingController(IService aiService)
    {
        _aiService = aiService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAiSettings()
    {
        var settings = await _aiService.GetAdminAiSettings();
        return Ok(settings);
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateAiSettings([FromBody] Request.UpdateAiSettingsRequest request)
    {
        var settings = await _aiService.UpdateAdminAiSettings(request);
        return Ok(settings);
    }
}
