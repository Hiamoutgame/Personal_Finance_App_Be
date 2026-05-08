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

    public static IQueryable<Repository.Entity.Transaction> ActiveTransactions(this IQueryable<Repository.Entity.Transaction> query)
    {
        return query.Where(transaction => !transaction.IsDeleted);
    }

    public static IQueryable<Repository.Entity.Transaction> InDateRange(
        this IQueryable<Repository.Entity.Transaction> query,
        DateTimeOffset start,
        DateTimeOffset end)
    {
        return query.Where(transaction => transaction.TransactionDate >= start
            && transaction.TransactionDate < end);
    }

    public static IQueryable<Repository.Entity.Transaction> Expenses(this IQueryable<Repository.Entity.Transaction> query)
    {
        return query.Where(transaction => transaction.Type == TransactionType.Expense.ToString());
    }
}
