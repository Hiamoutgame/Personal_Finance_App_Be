namespace Personal_Finance_Management.Service.Goal;

public class Response
{
    public class GetGoalsResponse
    {
        public List<GetGoal> Data { get; set; } = new List<GetGoal>();
    }
    public class GetGoal
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal SavedAmount { get; set; }
        public double ProgressPercentage { get; set; } 
        public DateTime DueDate { get; set; } 
        public required string Status { get; set; }
        public decimal SuggestedMonthlyContribution { get; set; }
    }
    public class GoalDetail
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal SavedAmount { get; set; }
        public int ProgressPercentage { get; set; } 
        public DateTime DueDate { get; set; }
        public int DaysRemaining { get; set; }     
        public required string Status { get; set; }
        public long SuggestedMonthlyContribution { get; set; } 
        public Guid? LinkedJarId { get; set; }
        public List<RecentContribution> RecentContributions { get; set; } = new();
    }
    
    public class RecentContribution
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}