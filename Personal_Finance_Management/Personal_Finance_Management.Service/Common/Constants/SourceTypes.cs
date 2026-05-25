namespace Personal_Finance_Management.Service.Common.Constants;

public static class SourceTypes
{
    public const string Manual = "Manual";
    public const string Imported = "Imported";
    public const string Ocr = "OCR";
    public const string Jar = "Jar";
    public const string System = "System";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Manual, Imported, Ocr, Jar, System
    };

    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}
