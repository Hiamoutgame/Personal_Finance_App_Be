namespace Personal_Finance_Management.Service.Common.Enums;

public static class SpendingLimitPeriod
{
    public const string Daily = "Daily";
    public const string Monthly = "Monthly";

    public static readonly IReadOnlySet<string> All = new HashSet<string> { Daily, Monthly };
    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}

public static class SpendingLimitTargetType
{
    public const string Jar = "Jar";
    public const string Category = "Category";

    public static readonly IReadOnlySet<string> All = new HashSet<string> { Jar, Category };
    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}
