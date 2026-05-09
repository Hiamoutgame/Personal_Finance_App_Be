using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Service.Jar;

namespace Personal_Finance_Management.Api.Controllers;

[ApiController]
[Route("api/v1/jars")]
[Authorize]
public class JarController : ControllerBase
{
    private readonly IService _service;
    public JarController(IService service)
    {
        _service = service;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetJar()
    {
        var result = await _service.GetJar();
        return Ok(result);
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateJar([FromBody] Request.CreateJarRequest request)
    {
        var result = await _service.CreateJar(request);
        return Ok(result);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateJar(Guid id, [FromBody] Request.UpdateJarRequest request)
    {
        var result = await _service.UpdateJar(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteJar(Guid id)
    {
        var result = await _service.DeleteJar(id);
        return Ok(result);
    }


}