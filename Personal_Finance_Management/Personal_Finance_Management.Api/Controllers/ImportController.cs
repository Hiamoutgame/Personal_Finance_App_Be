using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Api.Extensions;
using ImportRequest = Personal_Finance_Management.Service.import.Request;
using ImportService = Personal_Finance_Management.Service.import;

namespace Personal_Finance_Management.Api.Controllers;

[ApiController]
[Route("api/v1/imports")]
[Authorize(Policy = AuthorizationExtension.Policies.User)]
public class ImportController : ControllerBase
{
    private readonly ImportService.IServices _importService;

    public ImportController(ImportService.IServices importService)
    {
        _importService = importService;
    }

    [HttpPost("image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportImage([FromForm] ImportRequest.ImportData request)
    {
        var result = await _importService.ImportImage(request);
        return Ok(result);
    }
}
