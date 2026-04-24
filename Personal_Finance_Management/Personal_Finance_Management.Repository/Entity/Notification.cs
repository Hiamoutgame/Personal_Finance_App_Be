using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class Notification : BaseEntity<Guid>, IAudictableEntity
{
    
    public string Type { get; set; } = null!; // Type này là gì có bao nhiêu trạng thái rậy
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public bool IsRead { get; set; } = false; // này kiểm tra người dùng đọc r hay chưa đk
    public string? MetadataJson { get; set; } //Cái này dùng làm gì lưu gì v 
    
    //Nối với Account
    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;
    
    //Nốí với Broadcast (Bảng này để làm gì ta)
    public Guid? BroadcastId  { get; set; } 
    public Broadcast? Broadcast { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}