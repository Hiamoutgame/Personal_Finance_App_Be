namespace Personal_Finance_Management.Service.Common.Enums;

public static class FinancialAccountType
{
    public const string Cash = "Cash";
    public const string Bank = "Bank";
    public const string EWallet = "EWallet";
    public const string Other = "Other";

    public static readonly IReadOnlySet<string> All = new HashSet<string> { Cash, Bank, EWallet, Other };
    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}

public static class ConnectionMode
{
    public const string Manual = "Manual";
    public const string LinkedApi = "LinkedApi";

    public static readonly IReadOnlySet<string> All = new HashSet<string> { Manual, LinkedApi };
    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}

public static class SyncStatus
{
    public const string NeverSynced = "NeverSynced";
    public const string Synced = "Synced";
    public const string Syncing = "Syncing";
    public const string Error = "Error";
    public const string Disconnected = "Disconnected";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        NeverSynced, Synced, Syncing, Error, Disconnected
    };

    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}

public static class BankConnectionSessionStatus
{
    public const string Pending = "Pending";
    public const string Authorized = "Authorized";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Expired = "Expired";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Pending, Authorized, Completed, Failed, Expired
    };

    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}
