using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class AiSetting : BaseEntity, IAudictableEntity
{
    public string ModelName { get; set; } = null!;
    public string SystemPrompt { get; set; } = null!; // Này để làm gì ta 
    //khun đưa câu lệnh tương tác với người dùng 
    public decimal Temperature { get; set; }
    public int MaxTokens { get; set; }
    //Limit lại giới hạn token cho mỗi lần trả ra
    //canf trả ra nhiều thì càng mệt tốn tiền
    
    public string? ApiKeyEncrypted { get; set; } // Này để làm gì ta
    //Bên hệ thống Ai mỗi cái chìa khóa đó đóng tiền để có đc
    //muón sài đc chức năng này thì phải có key 
    
    public bool IsEnabled { get; set; }
    
    // nối với Account 
    public Guid? UpdatedByAdminId { get; set; } 
    public Account? UpdatedByAdmin { get; set; }
    
    
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

}
