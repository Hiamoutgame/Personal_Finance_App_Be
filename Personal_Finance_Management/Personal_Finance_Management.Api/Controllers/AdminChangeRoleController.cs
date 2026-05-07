using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Api.Extensions;

namespace Personal_Finance_Management.Api.Controllers;
[ApiController]
[Route("api/v1/change-role")]
[Authorize(Policy = AuthorizationExtension.Policies.Admin)]
public class AdminChangeRoleController: ControllerBase
{
    // hien: use method patch to change role    
    
}