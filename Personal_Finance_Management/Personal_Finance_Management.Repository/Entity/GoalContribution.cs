using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class GoalContribution : BaseEntity<Guid>
{
    public Guid GoalId { get; set; }
    public Goal Goal { get; set; } = null!;

    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}