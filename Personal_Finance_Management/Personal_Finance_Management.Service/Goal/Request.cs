namespace Personal_Finance_Management.Service.goal;

public class Request
{
    public class CreateGoalRequest
    {
        public required string Title { get; set; }
        public decimal TargetAmount { get; set; }
        public DateTime DueDate { get; set; }
        public Guid? LinkedJarId { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateGoalRequest
    {
        public string? Title { get; set; }
        public decimal? TargetAmount { get; set; }
        public DateTime? DueDate { get; set; }
        public Guid? LinkedJarId { get; set; }
        public string? Note { get; set; }
    }
}