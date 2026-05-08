using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Api.Extensions;
using Personal_Finance_Management.Repository.Enum;
using Personal_Finance_Management.Service.Admin;

namespace Personal_Finance_Management.Api.Controllers;
[ApiController]
[Route("api/v1/change-role")]
[Authorize(Policy = AuthorizationExtension.Policies.Admin)]
public class AdminChangeRoleController: ControllerBase
{
    // hien: use method patch to change role    
    private readonly IService _service;
    public AdminChangeRoleController(IService service)
    {
        _service = service;
    }

    [HttpPatch("{accountId:guid}")]
    public async Task<IActionResult> ChangeRole(Guid accountId, AccountRole role)
    {
        var result = await _service.UpdateRole(accountId, role);
        return Ok(result);
    }
}