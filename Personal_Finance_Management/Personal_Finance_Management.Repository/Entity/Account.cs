using Personal_Finance_Management.Repository.Abtraction;
using Personal_Finance_Management.Repository.Enum;

namespace Personal_Finance_Management.Repository.Entity;

public class Account : BaseEntity, IAudictableEntity
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string HashPassword { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; } = null;
    public string Status { get; set; } = "Active"; // | Banned 
    public string? StatusReason { get; set; } // này để làm gì vậy
    public string PreferredCurrency { get; set; } = "VND"; // này để xác nhận mệnh giá mà ng dùng quản lý tài chính hả
    public bool IsOnboardingCompleted { get; set; } = false; // này để làm gì vậy


    //1 Account chỉ có 1 Role
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!; // mặc định role nên được gán qua RoleId hoặc khi tạo dữ liệu seed
    //
    public OnboardingProfile? OnboardingProfile { get; set; }
    //
    public JarSetup? JarSetup { get; set; }
    //
    public ICollection<FinancialAccount> FinancialAccounts { get; set; } = new List<FinancialAccount>();
    //
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    //


    public DateTimeOffset? LastLoginAt { get; set; } //Đăng nhập gần nhất khi nào
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}
