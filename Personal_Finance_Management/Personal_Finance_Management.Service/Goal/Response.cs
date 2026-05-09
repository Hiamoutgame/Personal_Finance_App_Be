namespace Personal_Finance_Management.Service.goal;

public class Response
{
 
    public class GetGoalsResponse
    {
        public List<GetGoalItem> Data { get; set; } = new();
    }

    public class GetGoalItem
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
    
    public class GetGoalByIdResponse
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal SavedAmount { get; set; }
        public double ProgressPercentage { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysRemaining { get; set; }
        public required string Status { get; set; }
        public decimal SuggestedMonthlyContribution { get; set; }
        public Guid? LinkedJarId { get; set; }
    }


    public class CreateGoalResponse
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal SavedAmount { get; set; }
        public double ProgressPercentage { get; set; }
        public required string Status { get; set; }
        public DateTime DueDate { get; set; }
    }
    
    public class UpdateGoalResponse
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public decimal TargetAmount { get; set; }
        public DateTime DueDate { get; set; }
        public required string Status { get; set; }
    }
}