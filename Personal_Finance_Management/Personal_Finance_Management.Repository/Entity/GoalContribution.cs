using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class GoalContribution : BaseEntity
{
    public decimal Amount { get; set; }
    public string? Note { get; set; }

    public Guid GoalId { get; set; }
    public Goal Goal { get; set; } = null!;

    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;

    public Guid? SourceJarId { get; set; }
    public Jar? SourceJar { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
