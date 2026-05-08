namespace Personal_Finance_Management.Service.Admin;

public class Request
{
    public class AdminDashboardRequest
    {
        public string? Timeframe { get; set; }
    }

    public class AdminAuditLogsRequest
    {
        public Guid? AdminId { get; set; }
        public string? ActionType { get; set; }
        public string? EntityType { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
