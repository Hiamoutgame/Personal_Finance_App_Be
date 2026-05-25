namespace Personal_Finance_Management.Service.Common.Enums;

public static class BudgetMethod
{
    public const string SixJars = "SixJars";
    public const string Rule503020 = "Rule503020";
    public const string Custom = "Custom";
    public const string Undecided = "Undecided";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        SixJars, Rule503020, Custom, Undecided
    };

    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}
