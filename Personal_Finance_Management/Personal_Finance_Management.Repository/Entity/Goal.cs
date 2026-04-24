using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class Goal : BaseEntity<Guid>, IAudictableEntity
{
    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;

    public string Title { get; set; } = null!;
    public decimal TargetAmount { get; set; }
    public decimal SavedAmount { get; set; }

    public DateTime DueDate { get; set; }

    public ICollection<GoalContribution> Contributions { get; set; } = new List<GoalContribution>();

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}