using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class ImportJob: BaseEntity<Guid>, IAudictableEntity
{
    public string FileName { get; set; }
    public string? OriginalContentType { get; set; }
    public string StoredFilePath { get; set; }
    public string? BankCode { get; set; }
    public string Status{ get; set; } = "Pending";
    public int Progress { get; set; }
    public int? EstimatedRows{ get; set; }
    public int ParsedCount { get; set; }
    public int FailedCount { get; set; }
    public string? ErrorMessage { get; set; }
    

    //Nối với Account
    public Guid UserId { get; set; }
    public Account User{ get; set; }
    
    //Nối với Financial Account
    public Guid FinancialAccountId { get; set; } 
    public FinancialAccount FinancialAccount { get; set; }
 
    // Nối với ImportTransactionDraft 
    public ICollection<ImportTransactionDraft> Drafts { get; set; } = new List<ImportTransactionDraft>();
    
    public DateTimeOffset UploadedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}