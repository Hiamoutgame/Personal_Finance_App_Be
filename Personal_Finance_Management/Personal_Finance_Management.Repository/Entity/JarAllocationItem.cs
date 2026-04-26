using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class JarAllocationItem : BaseEntity<Guid>,IAudictableEntity
{
    
    public decimal Amount { get; set; }

    public decimal BalanceAfterAllocation { get; set; }
    
    //Nối tới hủ jarAllocation
    public Guid AllocationId { get; set; }
    public JarAllocation Allocation { get; set; } = null!;
    
    //Nối tới Jar
    public Guid JarId { get; set; }
    public Jar Jar { get; set; } = null!;
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}