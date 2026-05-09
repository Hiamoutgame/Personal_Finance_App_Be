using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Service.Transaction;

namespace Personal_Finance_Management.Api.Controllers;

[ApiController]
[Route("api/v1/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IService _service;
    public TransactionsController(IService service)
    {
        _service = service;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetTransactions([FromQuery] Request.GetTransactionsRequest request)
    {
        var result = await _service.GetTransactions(request);
        return Ok(result);
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateTransactions([FromBody] Request.CreateTransactionRequest request)
    {
        var result = await _service.CreateTransaction(request);
        return Ok(result);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateTransactions(Guid id, [FromBody] Request.UpdateTransactionRequest request)
    {
        var result = await _service.UpdateTransaction(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransactions(Guid id)
    {
        var result = await _service.DeleteTransaction(id);
        return Ok(result);
    }

    [HttpGet("Casso")]
    public async Task<IActionResult> SyncCassoTransactions([FromQuery] Request.CassoSyncTransactionsRequest request)
    {
        var result = await _service.SyncCassoTransactions(request);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("Casso")]
    public async Task<IActionResult> ProcessCassoWebhook([FromBody] Request.CassoWebhookRequest request)
    {
        var secureToken = Request.Headers["secure-token"].FirstOrDefault();
        var cassoSignature = Request.Headers["X-Casso-Signature"].FirstOrDefault();
        var result = await _service.ProcessCassoWebhook(request, secureToken, cassoSignature);
        return Ok(result);
    }
}
