using Personal_Finance_Management.Repository.Entity;
using Personal_Finance_Management.Repository.Enum;

namespace Personal_Finance_Management.Service.Admin;

internal static class AdminQueryExtensions
{
    public static IQueryable<Account> RegularUsers(this IQueryable<Account> query)
    {
        var userRoleCode = AccountRole.User.ToString();
        return query.Where(account => account.Role.Code == userRoleCode);
    }

    public static IQueryable<Transaction> ActiveTransactions(this IQueryable<Transaction> query)
    {
        return query.Where(transaction => !transaction.IsDeleted);
    }

    public static IQueryable<Transaction> InDateRange(
        this IQueryable<Transaction> query,
        DateTimeOffset start,
        DateTimeOffset end)
    {
        return query.Where(transaction => transaction.TransactionDate >= start
            && transaction.TransactionDate < end);
    }

    public static IQueryable<Transaction> Expenses(this IQueryable<Transaction> query)
    {
        return query.Where(transaction => transaction.Type == TransactionType.Expense.ToString());
    }
}
