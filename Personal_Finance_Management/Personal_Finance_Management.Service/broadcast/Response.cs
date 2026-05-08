using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Personal_Finance_Management.Service.broadcast
{
    public class Response
    {
        public class BroadcastsResponse
        {
            public Guid Id { get; set; }
            public string Title { get; set; } = null!;
            public string Body { get; set; } = null!;
            public string TargetAudience { get; set; } = "All";
            public string Status { get; set; } = "Queued";
            public DateTimeOffset? ScheduledAt { get; set; }
            public DateTimeOffset? SentAt { get; set; }
            public int TargetCount { get; set; } = 0;
            public int DeliveredCount { get; set; } = 0;
        }
    }
}