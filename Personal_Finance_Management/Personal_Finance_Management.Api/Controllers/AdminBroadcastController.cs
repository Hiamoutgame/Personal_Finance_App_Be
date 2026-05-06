using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Api.Extensions;
using Personal_Finance_Management.Service.broadcast;

namespace Personal_Finance_Management.Api.Controllers
{
    [ApiController]
    [Route("api/v1/admin/broadcasts")]
    [Authorize(Policy = AuthorizationExtension.Policies.Admin)]
    public class AdminBroadcastController : ControllerBase
    {
        private readonly IService _broadcastService;
        public AdminBroadcastController(IService broadcastService)
        {
            _broadcastService = broadcastService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateBroadcast([FromBody] Request.BroadcastsRequest request)
        {
            // Implementation for creating a broadcast
            var result = await _broadcastService.CreateBroadcast(request);
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetBroadcasts([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string status = "Queued")
        {
            // Implementation for retrieving broadcasts
            var result = await _broadcastService.GetBroadcasts(pageIndex, pageSize, status);
            return Ok(result);
        }
    }
}
