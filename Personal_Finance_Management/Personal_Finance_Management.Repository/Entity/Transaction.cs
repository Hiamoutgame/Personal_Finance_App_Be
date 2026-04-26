using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class Transaction : BaseEntity<Guid>, IAudictableEntity
{
    public string Type{ get; set; }  
    public decimal Amount{ get; set; }   
    public string? Note{ get; set; }
    public string? RawDescription{ get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public string SourceType { get; set; } = "Manual";
    public string? ExternalTransactionId { get; set; }
    public string? RawPayloadJson { get; set; }
    public bool IsDeleted{ get; set; } = false;
    
    //Nối với Account
    public Guid UserId { get; set; }
    public Account User { get; set; }
    
    //Nối với FinancialAccount 
    public Guid FinancialAccountId { get; set; }
    public FinancialAccount FinancialAccount { get; set; } = null!;
    
    //Nối với Jar Cate và Import là nhanh bên kia chua List của mấy này ne
    public Guid? JarId { get; set; }
    public Jar? Jar { get; set; }
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public ImportJob? ImportJob { get; set; }
    
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}