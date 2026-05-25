namespace Personal_Finance_Management.Service.Common.Enums;

public static class TransactionType
{
    public const string Income = "Income";
    public const string Expense = "Expense";

    public static readonly IReadOnlySet<string> All = new HashSet<string> { Income, Expense };

    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}
