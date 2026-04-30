using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class JarAllocation : BaseEntity, IAudictableEntity
{
    //là các hủ nhỏ phân ra trong hủ lớn
    //Note tổng số dư
    //Phân bổ
    //1 Cái Auditlog 
    //từ 7tr5 chuyển 2tr5 qua hủ ăn
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
    //MAIN FLOW NHÓM ĐỊNH NGHĨA RA 
    //Tìm ra được long mạch của app chưa
}
