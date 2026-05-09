using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Service.FinancialAccount;

namespace Personal_Finance_Management.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/financial-accounts")]
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

    [HttpPost("Manual")]
    public async Task<IActionResult> CreateManualFinancialAccount([FromBody] Request.CreateManualFinancialAccountRequest request)
    {
        var result = await _service.CreateManualFinancialAccount(request);
        return Ok(result);
    }

    [HttpPost("LinkApi")]
    public async Task<IActionResult> CreateLinkApiFinancialAccount([FromBody] Request.CreateLinkApiFinancialAccountRequest request)
    {
        var result = await _service.CreateLinkApiFinancialAccount(request);
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
