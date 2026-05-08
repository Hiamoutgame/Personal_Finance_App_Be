namespace Personal_Finance_Management.Service.Seeding;

public class SeedAccountsOptions
{
    public const string SectionName = "SeedAccounts";

    public bool Enabled { get; set; }
    public bool ResetPasswords { get; set; }
    public List<SeedAccountOptions> Accounts { get; set; } = [];
}
