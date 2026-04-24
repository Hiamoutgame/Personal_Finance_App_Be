using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class JarAllocationItem : BaseEntity<Guid>
{
    public Guid AllocationId { get; set; }
    public JarAllocation Allocation { get; set; } = null!;

    public Guid JarId { get; set; }
    public Jar Jar { get; set; } = null!;

    public decimal Amount { get; set; }

    public decimal BalanceAfterAllocation { get; set; }
}