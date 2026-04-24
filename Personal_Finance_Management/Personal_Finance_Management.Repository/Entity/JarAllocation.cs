using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class JarAllocation : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;

    public Guid? SourceFinancialAccountId { get; set; }
    public FinancialAccount? SourceFinancialAccount { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Note { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<JarAllocationItem> Items { get; set; } = new List<JarAllocationItem>();
}