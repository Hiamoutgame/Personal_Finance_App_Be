using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Service.FinancialAccount;

namespace Personal_Finance_Management.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class FinancialAccountController : ControllerBase
{
    private readonly IService _service;
    public FinancialAccountController(IService service)
    {
        _service = service;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetFinancialAccount()
    {
        var result = await _service.GetUserFinancialAccount();
        return Ok(result);
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateFinancialAccount([FromBody] Request.CreateFinancialAccountRequest request)
    {
        var result = await _service.CreateFinancialAccount(request);
        return Ok(result);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateFinancialAccount(Guid id, [FromBody] Request.UpdateFinancialAccountRequest request)
    {
        var result = await _service.UpdateFinancialAccount(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFinancialAccount(Guid id)
    {
        var result = await _service.DeleteFinancialAccount(id);
        return Ok(result);
    }
}