namespace Personal_Finance_Management.Service.Common.Enums;

public static class ImportJobStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string AwaitingReview = "AwaitingReview";
    public const string Completed = "Completed";
    public const string Failed = "Failed";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Pending, Processing, AwaitingReview, Completed, Failed
    };

    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}
