namespace Personal_Finance_Management.Repository.Abtraction;

public interface IAudictableEntity
{
    public DateTimeOffset CreatedAt { get; set; } // Tạo ra khi nào
    public DateTimeOffset? UpdatedAt { get; set; } // Cập Nhật Khi Nào
    public DateTimeOffset? ReadAt { get; set; } // Đọc Khi Nào
}