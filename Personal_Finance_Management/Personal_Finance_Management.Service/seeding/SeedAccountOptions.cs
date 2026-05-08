using Personal_Finance_Management.Repository.Enum;

namespace Personal_Finance_Management.Service.Seeding;

public class SeedAccountOptions
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public AccountRole Role { get; set; } = AccountRole.User;
}
