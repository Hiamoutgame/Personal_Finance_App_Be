using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class JarAllocation : BaseEntity, IAudictableEntity
{
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }
    
    //Nối với Account
    public Guid UserId { get; set; }
    public Account User { get; set; } 
    
    //Nối với JarAllocationItem
    public ICollection<JarAllocationItem> Items { get; set; } = new List<JarAllocationItem>();
    
    //Nối với SourceFinancialAccount 
    public Guid? SourceFinancialAccountId { get; set; }
    public FinancialAccount? SourceFinancialAccount { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}
