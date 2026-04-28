using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class Goal : BaseEntity<Guid>, IAudictableEntity
{
    public string Title { get; set; } = null!;
    public decimal TargetAmount { get; set; }
    public decimal SavedAmount { get; set; }
    
    public DateTime DueDate { get; set; }
    public string Status { get; set; }
    public string? Note { get; set; }
    //Nối với Account
    public Guid UserId { get; set; }
    public Account User { get; set; }
    
    //Nối với Jar
    public Guid? LinkedJarId{ get; set; }
    public Jar? LinkedJar { get; set; }
    
    //Nối với GoalContribution
    public ICollection<GoalContribution> Contributions { get; set; } = new List<GoalContribution>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}