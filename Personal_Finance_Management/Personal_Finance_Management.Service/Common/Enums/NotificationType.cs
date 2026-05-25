namespace Personal_Finance_Management.Service.Common.Enums;

public static class NotificationType
{
    public const string SpendingAlert = "SpendingAlert";
    public const string GoalUpdate = "GoalUpdate";
    public const string Reminder = "Reminder";
    public const string System = "System";
    public const string Broadcast = "Broadcast";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        SpendingAlert, GoalUpdate, Reminder, System, Broadcast
    };

    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}
