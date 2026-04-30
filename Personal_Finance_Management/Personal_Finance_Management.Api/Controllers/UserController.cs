using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Service.User;

namespace Personal_Finance_Management.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IService _service;
    public UserController(IService service)
    {
        _service = service;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var result = await _service.GetUserInfor();
        return Ok(result);
    }

    [HttpPatch("me")]
    public async Task<IActionResult> UpdateMe(Request.UpdateUserRequest request)
    {
        var result = await _service.UpdateUserProfile(request);
        return Ok(result);
    }

    [HttpGet("me/setup")]
    public async Task<IActionResult> GetSetup()
    {
        var result = await _service.ViewSetup();
        return Ok(result);
    }
}