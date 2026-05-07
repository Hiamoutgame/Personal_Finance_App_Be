using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Service.User;

namespace Personal_Finance_Management.Api.Controllers
{
    [ApiController]
    [Route("api/v1/admin/users")]

    public class AdminUserController : ControllerBase
    {
        private readonly Service.User.IService _userService;

        public AdminUserController(Service.User.IService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userService.GetUserInforById(new Request.UserIdRequest { UserId = id });
            return Ok(user);
        }
        // hien: endpoint nay xem Khóa hoặc mở khóa tài khoản user
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateUserStatus(Guid id)
        {
            var user = await _userService.UpdateUserStatus(new Request.UserStatusRequest { UserId = id });
            return Ok(user);
        }
    }
}