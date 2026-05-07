namespace Personal_Finance_Management.Service.Admin;

public class Response
{
    public class AdminDashboardResponse
    {
        public AdminDashboardSummary Summary { get; set; } = null!;
        public List<RecentUserItem> RecentUsers { get; set; } = [];
        public List<RecentTransactionItem> RecentTransactions { get; set; } = [];
    }

    public class AdminDashboardSummary
    {
        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int ActiveUsersLast30Days { get; set; }
        public int BannedUsers { get; set; }
        public int TotalTransactions { get; set; }
        public int TransactionsThisMonth { get; set; }
        public int TotalJars { get; set; }
        public int ActiveGoals { get; set; }
        public int PendingImportJobs { get; set; }
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
        public string Username { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Status { get; set; } = null!;
        public bool IsOnboardingCompleted { get; set; }
        public DateTimeOffset? LastLoginAt { get; set; }
    }

    public class RecentTransactionItem
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public decimal TransactionsAmount { get; set; }
        public string? Note { get; set; }
        public DateTimeOffset TransactionDate { get; set; }
        public TransactionUserItem User { get; set; } = null!;
        public TransactionFinancialAccountItem? FinancialAccount { get; set; }
        public TransactionCategoryItem? Category { get; set; }
    }

    public class TransactionUserItem
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
    }

    public class TransactionFinancialAccountItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string AccountType { get; set; } = null!;
    }

    public class TransactionCategoryItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
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
}
