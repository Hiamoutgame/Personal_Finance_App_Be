namespace Personal_Finance_Management.Service.Common.Enums;

public static class BroadcastStatus
{
    public const string Queued = "Queued";
    public const string Sent = "Sent";
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Queued, Sent, Failed, Cancelled
    };

    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}
