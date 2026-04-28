using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Service.Onboarding;

namespace Personal_Finance_Management.Api.Controllers;

[ApiController]
[Route("[controller]")]
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