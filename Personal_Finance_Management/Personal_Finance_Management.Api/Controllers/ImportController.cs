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

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateImport([FromForm] ImportRequest.ImportData request)
    {
        var result = await _importService.ImportImage(request);
        return Ok(result);
    }

    [HttpPost("image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportImage([FromForm] ImportRequest.ImportData request)
    {
        var result = await _importService.ImportImage(request);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetImports([FromQuery] ImportRequest.GetImportsRequest request)
    {
        var result = await _importService.GetImports(request);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetImport([FromRoute] Guid id)
    {
        var result = await _importService.GetImport(id);
        return Ok(result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateImport(
        [FromRoute] Guid id,
        [FromBody] ImportRequest.UpdateImportDraftRequest request)
    {
        var result = await _importService.UpdateImport(id, request);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/drafts/{draftId:guid}")]
    public async Task<IActionResult> UpdateImportDraft(
        [FromRoute] Guid id,
        [FromRoute] Guid draftId,
        [FromBody] ImportRequest.UpdateImportDraftRequest request)
    {
        var result = await _importService.UpdateImportDraft(id, draftId, request);
        return Ok(result);
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> ConfirmImport(
        [FromRoute] Guid id,
        [FromBody] ImportRequest.ConfirmImportRequest request)
    {
        var result = await _importService.ConfirmImport(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteImport([FromRoute] Guid id)
    {
        var result = await _importService.DeleteImport(id);
        return Ok(result);
    }

    [HttpGet("images/{fileName}")]
    public async Task<IActionResult> GetUploadedImage([FromRoute] string fileName)
    {
        var file = await _importService.GetUploadedImage(fileName);
        return PhysicalFile(file.StoredFilePath, file.ContentType, file.FileName);
    }
}
