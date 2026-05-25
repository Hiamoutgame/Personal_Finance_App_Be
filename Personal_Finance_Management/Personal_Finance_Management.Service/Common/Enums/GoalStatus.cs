namespace Personal_Finance_Management.Service.Common.Enums;

public static class GoalStatus
{
    public const string Active = "Active";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlySet<string> All = new HashSet<string> { Active, Completed, Cancelled };
    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}
