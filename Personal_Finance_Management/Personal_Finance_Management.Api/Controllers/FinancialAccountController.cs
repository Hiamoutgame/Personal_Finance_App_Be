using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Service.FinancialAccount;
using BankConnectionRequest = Personal_Finance_Management.Service.BankConnection.Request;
using BankConnectionService = Personal_Finance_Management.Service.BankConnection.IService;
using BankSyncRequest = Personal_Finance_Management.Service.BankSync.Request;
using BankSyncService = Personal_Finance_Management.Service.BankSync.IService;

namespace Personal_Finance_Management.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/financial-accounts")]
public class FinancialAccountController : ControllerBase
{
    private readonly IService _service;
    private readonly BankConnectionService _bankConnectionService;
    private readonly BankSyncService _bankSyncService;

    public FinancialAccountController(
        IService service,
        BankConnectionService bankConnectionService,
        BankSyncService bankSyncService)
    {
        _service = service;
        _bankConnectionService = bankConnectionService;
        _bankSyncService = bankSyncService;
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
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> CreateLinkApiFinancialAccount([FromBody] Request.CreateLinkApiFinancialAccountRequest request)
    {
        var result = await _service.CreateLinkApiFinancialAccount(request);
        return Ok(result);
    }

    [HttpPost("sepay/connect")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> StartSepayConnection([FromBody] BankConnectionRequest.StartSepayConnectionRequest request)
    {
        var result = await _bankConnectionService.StartSepayConnection(request);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("sepay/callback")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> HandleSepayCallback([FromQuery] string? code, [FromQuery] string? state, [FromQuery] string? error)
    {
        var result = await _bankConnectionService.HandleSepayCallback(code, state, error);
        if (!string.IsNullOrWhiteSpace(result.redirectUrl))
        {
            return Redirect(result.redirectUrl);
        }

        return result.success ? Ok(result) : BadRequest(result);
    }

    [AllowAnonymous]
    [HttpPost("sepay/webhook")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> ProcessSepayWebhook([FromBody] BankSyncRequest.SepayWebhookRequest request)
    {
        var authorization = Request.Headers["Authorization"].FirstOrDefault();
        var result = await _bankSyncService.ProcessSepayWebhook(request, authorization);
        return Ok(result);
    }

    [HttpPost("{id}/sync")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> SyncFinancialAccount(Guid id, [FromBody] BankSyncRequest.SyncLinkedAccountRequest request)
    {
        var result = await _bankSyncService.SyncLinkedAccount(id, request);
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
