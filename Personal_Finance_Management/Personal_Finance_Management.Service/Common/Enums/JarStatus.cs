namespace Personal_Finance_Management.Service.Common.Enums;

public static class JarStatus
{
    public const string Active = "Active";
    public const string Paused = "Paused";
    public const string Archived = "Archived";

    public static readonly IReadOnlySet<string> All = new HashSet<string> { Active, Paused, Archived };
    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}
