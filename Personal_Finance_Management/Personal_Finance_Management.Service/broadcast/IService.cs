using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Personal_Finance_Management.Service.baseServices;
using Personal_Finance_Management.Service.Common.Enums;

namespace Personal_Finance_Management.Service.broadcast
{
    public interface IService
    {
        public Task<Response.BroadcastsResponse> CreateBroadcast(Request.BroadcastsRequest request);
        public Task<Page<Response.BroadcastsResponse>> GetBroadcasts(int pageIndex, int pageSize, string status = BroadcastStatus.Queued);
        public Task<int> DispatchDueBroadcastsAsync(CancellationToken cancellationToken = default);
    }
}
