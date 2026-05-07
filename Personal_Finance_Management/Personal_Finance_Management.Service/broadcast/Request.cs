using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Personal_Finance_Management.Service.broadcast
{
    public class Request
    {
        public class BroadcastsRequest
        {
            public required string Title { get; set; }
            public required string Body { get; set; }
            public string TargetAudience { get; set; } = "All";
            public DateTimeOffset? ScheduledAt { get; set; }
        }
    }
}