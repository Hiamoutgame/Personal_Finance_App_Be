using Microsoft.AspNetCore.Mvc;
using AuthRequest = Personal_Finance_Management.Service.Auth.Request;
using AuthService = Personal_Finance_Management.Service.Auth;

namespace Personal_Finance_Management.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService.IService _authService;

    public AuthController(AuthService.IService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRequest.RegisterRequest request)
    {
        var result = await _authService.Register(request);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest.LoginRequest request)
    {
        var result = await _authService.Login(request);
        return Ok(result);
    }
}
