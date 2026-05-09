using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Api.Extensions;
using Personal_Finance_Management.Service.Onboarding;

namespace Personal_Finance_Management.Api.Controllers;

[ApiController]
[Route("api/v1/onboarding")]
[Authorize(Policy = AuthorizationExtension.Policies.User)]
public class OnboardingController : ControllerBase
{
    private readonly IService _service;
    public OnboardingController(IService service)
    {
        _service = service;
    }

    [HttpPost("")]
    public async Task<IActionResult> FillOnboarding(Request.FillOnboardingRequest request)
    {
        var result = await _service.CreateOnboarding(request);
        return Ok(result);
    }
}
