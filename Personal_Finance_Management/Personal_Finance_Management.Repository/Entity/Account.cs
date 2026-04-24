using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class Account: BaseEntity<Guid>, IAudictableEntity
{
    
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; } = null;
    public string Status { get; set; } = "Active";
    public string? StatusReason { get; set; } // này để làm gì vậy
    public string PreferredCurrency { get; set; } = "VND"; // này để làm gì vậy
    public bool IsOnboardingCompleted { get; set; } = false; // này để làm gì vậy
    public DateTimeOffset? LastLoginAt { get; set; } //Đăng nhập gần nhất khi nào
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    
    //1 Account chỉ có 1 Role
    public short RoleId { get; set; }
    public Role? Role { get; set; }

    //A
    public OnboardingProfile? OnboardingProfile { get; set; }
    public JarSetup? JarSetup { get; set; }

    public ICollection<FinancialAccount> FinancialAccounts { get; set; } = new List<FinancialAccount>();
    
}