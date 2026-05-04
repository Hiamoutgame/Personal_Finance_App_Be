using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class Goal : BaseEntity, IAudictableEntity
{
    public string Title { get; set; } = null!;
    public decimal TargetAmount { get; set; }
    public decimal SavedAmount { get; set; } = 0;

    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
    //Nối với Account
    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;

    //Nối với Jar
    public Guid? LinkedJarId { get; set; }
    public Jar? LinkedJar { get; set; }

    //Nối với GoalContribution
    public ICollection<GoalContribution> Contributions { get; set; } = new List<GoalContribution>();

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
