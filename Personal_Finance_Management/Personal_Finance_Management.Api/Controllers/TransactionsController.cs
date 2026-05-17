using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Service.Transaction;
using BankSyncRequest = Personal_Finance_Management.Service.BankSync.Request;
using BankSyncService = Personal_Finance_Management.Service.BankSync.IService;

namespace Personal_Finance_Management.Api.Controllers;

[ApiController]
[Route("api/v1/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IService _service;
    private readonly BankSyncService _bankSyncService;

    public TransactionsController(IService service, BankSyncService bankSyncService)
    {
        _service = service;
        _bankSyncService = bankSyncService;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetTransactions([FromQuery] Request.GetTransactionsRequest request)
    {
        var result = await _service.GetTransactions(request);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransactionById(Guid id)
    {
        var result = await _service.GetTransactionById(id);
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
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> SyncCassoTransactions([FromQuery] Request.CassoSyncTransactionsRequest request)
    {
        var result = await _bankSyncService.SyncLinkedAccount(
            request.financialAccountId,
            new BankSyncRequest.SyncLinkedAccountRequest
            {
                fromDate = request.fromDate,
                toDate = request.toDate,
                page = request.page,
                pageSize = request.pageSize,
                sort = request.sort,
                triggerProviderSync = false
            });
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("Casso")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> ProcessCassoWebhook([FromBody] Request.CassoWebhookRequest request)
    {
        var secureToken = Request.Headers["secure-token"].FirstOrDefault();
        var cassoSignature = Request.Headers["X-Casso-Signature"].FirstOrDefault();
        var result = await _bankSyncService.ProcessCassoWebhook(
            new BankSyncRequest.CassoWebhookRequest
            {
                error = request.error,
                data = request.data
            },
            secureToken,
            cassoSignature);
        return Ok(result);
    }
}
