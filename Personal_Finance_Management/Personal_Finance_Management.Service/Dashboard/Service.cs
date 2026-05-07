using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;

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
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        var userIdGuid = Guid.Parse(userId);

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);
        if (user == null)
            throw new Exception("User not found");

        // ===============================BalanceSummaryResponse===============================
        var totalJar = _dbContext.Jars.Where(x => x.UserId == userIdGuid).Sum(x => x.Balance);
        var totalAccount = _dbContext.FinancialAccounts.Where(x => x.UserId == userIdGuid).Sum(x => x.CurrentBalance);
        var totalIncome = _dbContext.Transactions.Where(x => x.Type == "Income").Sum(x => x.TransactionsAmount);
        var totalExpense = _dbContext.Transactions.Where(x => x.Type == "Expense").Sum(x => x.TransactionsAmount);
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
                .Where(t => t.FromJarId == j.Id).Sum(s => (decimal?)s.TransactionsAmount) ?? 0,
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
        var categoryQuery = _dbContext.Categories.Where(x => x.OwnerUserId == userIdGuid);
        var tmpCategoryObject = categoryQuery.Select(c => new
        {
            Category = c,
            totalSpent = _dbContext.Transactions.Where(t => t.CategoryId == c.Id && t.Type == "Expense")
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
        var transactionQuery = _dbContext.Transactions.Where(x => x.UserId == userIdGuid);
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