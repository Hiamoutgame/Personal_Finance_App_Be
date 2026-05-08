using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Service.Transaction;

namespace Personal_Finance_Management.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IService _service;
    public TransactionsController(IService service)
    {
        _service = service;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetTransactions([FromQuery]Request.GetTransactionsRequest request)
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
}