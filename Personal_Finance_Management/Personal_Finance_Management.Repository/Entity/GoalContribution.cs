using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class GoalContribution : BaseEntity, IAudictableEntity
{
    public decimal Amount { get; set; }
    public string? Note{ get; set; }
    
    // Nối với Goal
    public Guid GoalId { get; set; }   
    public Goal Goal { get; set; }
    
    //Nối với Account
    public Guid UserId{ get; set; }
    public Account User { get; set; }
 
    //Nối với Jar
    public Guid? SourceJarId{ get; set; }
    public Jar? SourceJar{ get; set; }
    
    //Nối với Financial_Account
    public Guid? SourceFinancialAccountId{ get; set; }
    public FinancialAccount? SourceFinancialAccount { get; set; }
    
    public DateTimeOffset  CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}
