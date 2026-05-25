namespace Personal_Finance_Management.Service.Common.Enums;

public static class AccountStatus
{
    public const string Active = "Active";
    public const string Banned = "Banned";

    public static readonly IReadOnlySet<string> All = new HashSet<string> { Active, Banned };
    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}
