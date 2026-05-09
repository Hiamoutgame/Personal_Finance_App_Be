using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Api.Extensions;
using Personal_Finance_Management.Service.User;

namespace Personal_Finance_Management.Api.Controllers
{
    [ApiController]
    [Route("api/v1/admin/users")]
    [Authorize(Policy = AuthorizationExtension.Policies.Admin)]
    public class AdminUserController : ControllerBase
    {
        private readonly Service.User.IService _userService;

        public AdminUserController(Service.User.IService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] Request.GetAdminUsersRequest request)
        {
            var users = await _userService.GetAdminUsers(request);
            return Ok(users);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userService.GetUserInforById(new Request.UserIdRequest { UserId = id });
            return Ok(user);
        }
        // hien: endpoint nay xem Khóa hoặc mở khóa tài khoản user
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] Request.UserStatusRequest request)
        {
            request ??= new Request.UserStatusRequest();
            request.UserId = id;
            var user = await _userService.UpdateUserStatus(request);
            return Ok(user);
        }
    }
}
