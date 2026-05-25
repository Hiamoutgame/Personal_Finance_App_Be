namespace Personal_Finance_Management.Service.Common.Enums;

public static class ReminderFrequency
{
    public const string Daily = "Daily";
    public const string Weekly = "Weekly";
    public const string Monthly = "Monthly";
    public const string Quarterly = "Quarterly";
    public const string Yearly = "Yearly";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Daily, Weekly, Monthly, Quarterly, Yearly
    };

    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}

public static class ReminderStatus
{
    public const string Active = "Active";
    public const string Paused = "Paused";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Active, Paused, Completed, Cancelled
    };

    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}
