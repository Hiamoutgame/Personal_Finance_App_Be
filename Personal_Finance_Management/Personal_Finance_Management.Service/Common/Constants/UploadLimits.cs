namespace Personal_Finance_Management.Service.Common.Constants;

public static class UploadLimits
{
    public const long MaxFileSizeBytes = 10L * 1024 * 1024;
    public const int MaxImagePixels = 5000;
    public const int OcrTimeoutSeconds = 300;
}

public static class OcrLayouts
{
    public const string None = "none";
    public const string Invoice = "invoice";
    public const string Document = "document";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        None, Invoice, Document
    };

    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}
