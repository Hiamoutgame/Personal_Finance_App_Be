using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Service.Base;

namespace Personal_Finance_Management.Service.Dashboard;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }
    
    public async Task<Response.GetDashboardResult> GetDashboard()
    {
        var userIdGuid = ServiceClaimHelper.GetRequiredUserId(_httpContext);

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);
        if (user == null)
            throw new Exception("User not found");

        // ===============================BalanceSummaryResponse===============================
        var totalJar = await _dbContext.Jars
            .Where(x => x.UserId == userIdGuid)
            .SumAsync(x => x.Balance);
        var totalAccount = await _dbContext.FinancialAccounts
            .Where(x => x.UserId == userIdGuid)
            .SumAsync(x => x.CurrentBalance);
        var totalIncome = await _dbContext.Transactions
            .Where(x => x.UserId == userIdGuid && !x.IsDeleted && x.Type == "Income")
            .SumAsync(x => (decimal?)x.TransactionsAmount) ?? 0m;
        var totalExpense = await _dbContext.Transactions
            .Where(x => x.UserId == userIdGuid && !x.IsDeleted && x.Type == "Expense")
            .SumAsync(x => (decimal?)x.TransactionsAmount) ?? 0m;
        var BalanceSummaryResponse = new Response.BalanceSummaryResponse
        {
            totalBalance = totalJar + totalAccount,
            allocatedBalance = totalJar,
            unallocatedBalance = totalAccount - totalJar,
            totalIncome = totalIncome,
            totalExpense = totalExpense,
            netChange = totalIncome - totalExpense
        };
        
        // ===============================financialAccounts===============================
        var financialAccountQuery = _dbContext.FinancialAccounts.Where(x => x.UserId == userIdGuid);
        var selectedFinancialAccountQuery = financialAccountQuery.Select(x =>
            new Response.FinancialAccountResponse
            {
                id = x.Id,
                name = x.Name,
                currentBalance = x.CurrentBalance,
                isDefault = x.IsDefault
            });
        // ===============================jarSummary===============================
        var jarQuery = _dbContext.Jars.Where(x => x.UserId == userIdGuid);

        var tmpJarObject = jarQuery.Select(j => new
        {
            jar = j,
            spent = _dbContext.Transactions
                .Where(t => t.UserId == userIdGuid
                            && !t.IsDeleted
                            && t.Type == "Expense"
                            && t.FromJarId == j.Id)
                .Sum(s => (decimal?)s.TransactionsAmount) ?? 0,
        });
        var selectedJarQuery = tmpJarObject.Select(x => new Response.JarSummaryResponse
        {
            jarId = x.jar.Id,
            jarName = x.jar.Name,
            balance = x.jar.Balance,
            spent = x.spent,
            spentPercentage = (x.jar.Balance + x.spent) == 0 ? 0 : (x.spent * 100m) / (x.jar.Balance + x.spent)
        });
        
        
        // ===============================categoryBreakdown===============================
        var categoryQuery = _dbContext.Categories
            .Where(x => x.IsActive && (x.OwnerUserId == null || x.OwnerUserId == userIdGuid));
        var tmpCategoryObject = categoryQuery.Select(c => new
        {
            Category = c,
            totalSpent = _dbContext.Transactions
                .Where(t => t.UserId == userIdGuid
                            && !t.IsDeleted
                            && t.CategoryId == c.Id
                            && t.Type == "Expense")
                .Sum(s => (decimal?)s.TransactionsAmount) ?? 0

        });
        var selectedCategoryQuery = tmpCategoryObject.Select(x => new Response.CategoryBreakdownResponse
        {
            categoryId = x.Category.Id,
            categoryName = x.Category.Name,
            totalAmount = x.totalSpent,
            percentage = totalExpense == 0 ? 0 : (x.totalSpent * 100m) / totalExpense
        });
        
        
        // ===============================recentTransactions===============================
        var transactionQuery = _dbContext.Transactions
            .Where(x => x.UserId == userIdGuid && !x.IsDeleted)
            .OrderByDescending(x => x.TransactionDate)
            .Take(10);
        var selectedTransactionsQuery = transactionQuery.Select(x => new Response.RecentTransactionResponse
        {
            id = x.Id,
            type = x.Type,
            transactionsAmount = x.TransactionsAmount,
            note = x.Note,
            date = x.TransactionDate,
        });
        // ===============================goalProgress===============================
        var goalQuery = _dbContext.Goals.Where(x => x.UserId == userIdGuid);
        var selectedGoalQuery = goalQuery.Select(x => new Response.GoalProgressResponse
        {
            goalId = x.Id,
            title = x.Title,
            progressPercentage = x.TargetAmount == 0 ? 0 : (x.SavedAmount * 100m) / x.TargetAmount,
            daysRemaining = (decimal)(x.DueDate - DateTimeOffset.UtcNow).TotalDays
        });

        var result = new Response.GetDashboardResult
        {
            balanceSummary = BalanceSummaryResponse,
            financialAccounts = selectedFinancialAccountQuery.ToList(),
            jarSummary = selectedJarQuery.ToList(),
            categoryBreakdown = selectedCategoryQuery.ToList(),
            recentTransactions = selectedTransactionsQuery.ToList(),
            goalProgress = selectedGoalQuery.ToList(),
        };
        return result;
    }
}
