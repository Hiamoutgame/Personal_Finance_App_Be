using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class Transaction : BaseEntity, IAudictableEntity
{
    public string Type { get; set; } = null!;
    public decimal TransactionsAmount { get; set; }
    public string? Note { get; set; }
    public string? RawDescription { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public string SourceType { get; set; } = "Manual";
    public string? ExternalTransactionId { get; set; }
    public decimal? JarBalanceAfterAllocation { get; set; }
    public Guid? FromJarId { get; set; }
    public Guid? ToJarId { get; set; }
    public string? RawPayloadJson { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; }

    //Nối với Account
    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;

    //Nối với FinancialAccount 
    public Guid FinancialAccountId { get; set; }
    public FinancialAccount FinancialAccount { get; set; } = null!;

    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public Guid? ImportJobId { get; set; }
    public ImportJob? ImportJob { get; set; }
    public Jar? FromJar { get; set; }
    public Jar? ToJar { get; set; }


    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
