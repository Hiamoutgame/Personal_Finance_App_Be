namespace Personal_Finance_Management.Service.Dashboard;

public class Response
{
    public class GetDashboardResult
    {
        public required BalanceSummaryResponse balanceSummary { get; set; }
        public required List<FinancialAccountResponse> financialAccounts { get; set; }
        public required List<JarSummaryResponse> jarSummary { get; set; }
        public required List<CategoryBreakdownResponse> categoryBreakdown { get; set; }
        public required List<RecentTransactionResponse> recentTransactions { get; set; }
        public required List<GoalProgressResponse> goalProgress { get; set; }
    }

    public class BalanceSummaryResponse
    {
        public required decimal totalBalance { get; set; }
        public required decimal allocatedBalance { get; set; }
        public required decimal unallocatedBalance { get; set; }
        public required decimal totalIncome { get; set; }
        public required decimal totalExpense { get; set; }
        public required decimal netChange { get; set; }
    }

    public class FinancialAccountResponse
    {
        public required Guid id { get; set; }
        public required string name { get; set; }
        public required decimal currentBalance { get; set; }
        public required bool isDefault { get; set; }
    }

    public class JarSummaryResponse
    {
        public required Guid jarId { get; set; }
        public required string jarName { get; set; }
        public required decimal balance { get; set; }
        public required decimal spent { get; set; }
        public required decimal spentPercentage { get; set; }
    }

    public class CategoryBreakdownResponse
    {
        public required Guid categoryId { get; set; }
        public required string categoryName { get; set; }
        public required decimal totalAmount { get; set; }
        public required decimal percentage { get; set; }
    }

    public class RecentTransactionResponse
    {
        public required Guid id { get; set; }
        public required string type { get; set; }
        public required decimal transactionsAmount { get; set; }
        public required string? note { get; set; }
        public required DateTimeOffset date { get; set; }
    }

    public class GoalProgressResponse
    {
        public required Guid goalId { get; set; }
        public required string title { get; set; }
        public required decimal progressPercentage { get; set; }
        public required decimal daysRemaining { get; set; }
    }
}