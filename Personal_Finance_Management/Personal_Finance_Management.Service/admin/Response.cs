namespace Personal_Finance_Management.Service.Admin;

public class Response
{
    public class AdminDashboardResponse
    {
        public List<StatCard> StatCards { get; set; } = [];
        public List<TransactionTrendItem> TransactionVolumeTrend { get; set; } = [];
        public List<TopCategoryItem> TopSpendingCategories { get; set; } = [];
        public List<RetentionTrendItem> RetentionTrend { get; set; } = [];
        public List<RecentUserItem> RecentUsers { get; set; } = [];
        public List<RecentTransactionItem> RecentTransactions { get; set; } = [];
    }

    public class StatCard
    {
        public string Type { get; set; } = null!;
        public string Label { get; set; } = null!;
        public int? Value { get; set; }
        public decimal? DeltaPercent { get; set; }
        public int? Dau { get; set; }
        public int? Mau { get; set; }
        public decimal? StickinessPercent { get; set; }
        public decimal? TotalTransactionValue { get; set; }
        public int? TotalTransactions { get; set; }
        public decimal? ErrorRatePercent { get; set; }
        public string? Status { get; set; }
        public int? BannedUsers { get; set; }
    }

    public class TransactionTrendItem
    {
        public string Label { get; set; } = null!;
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class TopCategoryItem
    {
        public string Label { get; set; } = null!;
        public decimal Value { get; set; }
    }

    public class RetentionTrendItem
    {
        public string PeriodLabel { get; set; } = null!;
        public int CohortA { get; set; }
        public int CohortB { get; set; }
        public int CohortC { get; set; }
        public int CohortD { get; set; }
    }

    public class RecentUserItem
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class RecentTransactionItem
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? Note { get; set; }
        public DateTimeOffset TransactionDate { get; set; }
    }

    public class AdminAuditLogsResponse
    {
        public List<AdminAuditLogItem> Data { get; set; } = [];
        public Pagination Pagination { get; set; } = new();
    }

    public class AdminAuditLogItem
    {
        public Guid Id { get; set; }
        public string AdminUsername { get; set; } = null!;
        public string ActionType { get; set; } = null!;
        public string EntityType { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class Pagination
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
