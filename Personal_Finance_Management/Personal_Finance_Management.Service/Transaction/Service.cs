using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;

namespace Personal_Finance_Management.Service.Transaction;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }
    
    public async Task<Response.GetTransactionsResult> GetTransactions(Request.GetTransactionsRequest request)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        var userIdGuid = Guid.Parse(userId);

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);
        if (user == null)
            throw new Exception("User not found");
        
        var query = _dbContext.Transactions
            .Where(x => x.UserId == userIdGuid && x.IsDeleted == false);

        // Filter by financialAccountId
        if (request.financialAccountId.HasValue)
            query = query.Where(x => x.FinancialAccountId == request.financialAccountId);

        // Filter by type
        if (!string.IsNullOrEmpty(request.type))
            query = query.Where(x => x.Type == request.type);

        // Filter by jarId (matches FromJarId or ToJarId)
        if (request.jarId.HasValue)
            query = query.Where(x => x.FromJarId == request.jarId || x.ToJarId == request.jarId);

        // Filter by categoryId
        if (request.categoryId.HasValue)
            query = query.Where(x => x.CategoryId == request.categoryId.Value);

        // Filter by fromDate
        if (request.fromDate.HasValue)
        {
            var from = new DateTimeOffset(request.fromDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            query = query.Where(x => x.TransactionDate >= from);
        }

        // Filter by toDate
        if (request.toDate.HasValue)
        {
            var to = new DateTimeOffset(request.toDate.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
            query = query.Where(x => x.TransactionDate <= to);
        }
        
        // Filter by keyword (search in Note and RawDescription)
        if (!string.IsNullOrEmpty(request.keyword))
        {
            var kw = request.keyword.ToLower();
            query = query.Where(x =>
                (x.Note != null && x.Note.ToLower().Contains(kw))
                || (x.RawDescription != null && x.RawDescription.ToLower().Contains(kw)));
        }
        // Sorting
        if (!string.IsNullOrEmpty(request.sortBy))
        {
            var sortDir = request.sortDir?.ToLower() == "asc" ? "asc" : "desc";

            switch (request.sortBy.ToLower())
            {
                case "date":
                    query = sortDir == "asc"
                        ? query.OrderBy(x => x.TransactionDate)
                        : query.OrderByDescending(x => x.TransactionDate);
                    break;

                case "amount":
                    query = sortDir == "asc"
                        ? query.OrderBy(x => x.TransactionsAmount)
                        : query.OrderByDescending(x => x.TransactionsAmount);
                    break;

                case "createdat":
                    query = sortDir == "asc"
                        ? query.OrderBy(x => x.CreatedAt)
                        : query.OrderByDescending(x => x.CreatedAt);
                    break;

                case "type":
                    query = sortDir == "asc"
                        ? query.OrderBy(x => x.Type)
                        : query.OrderByDescending(x => x.Type);
                    break;

                default:
                    query = query.OrderByDescending(x => x.CreatedAt);
                    break;
            }
        }
        else
        {
            // Default sort
            query = query.OrderByDescending(x => x.CreatedAt);
        }
        
        query = query.Skip((request.pageIndex - 1) * request.pageSize).Take(request.pageSize);
        var selectedQuery = query.Select(x => new Response.GetTransactionResponse
        {
            id = x.Id,
            type = x.Type,
            transactionsAmount = x.TransactionsAmount,
            note = x.Note,
            date = x.CreatedAt,
            financialAccount = new Response.TransactionFinancialAccountResponse
            {
                id = x.FinancialAccountId,
                name = x.FinancialAccount.Name,
            },
            jar = new Response.TransactionJarResponse
            {
                id = x.FromJar.Id,
                name = x.FromJar.Name,
            },
            category = new Response.TransactionCategoryResponse
            {
                id = x.Category.Id,
                name = x.Category.Name,
            }
        });
        var totalCount =  query.Count();
        var selectedPagination = new Response.PaginationResponse
        {
            page = request.pageIndex,
            pageSize = request.pageSize,
            totalCount = totalCount,
            totalPages = (totalCount + request.pageSize - 1) / request.pageSize,
        };
        
        var result = new Response.GetTransactionsResult
        {
            data = selectedQuery.ToList(),
            pagination = selectedPagination,
        };
        
        return result;
    }

    public async Task<Response.CreateTransactionResponse> CreateTransaction(Request.CreateTransactionRequest request)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        var userIdGuid = Guid.Parse(userId);

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);
        if (user == null)
            throw new Exception("User not found");
        var transaction = new Repository.Entity.Transaction()
        {
            // Identity
            UserId = userIdGuid,
            FinancialAccountId = request.financialAccountId,
            CategoryId = request.categoryId,
            FromJarId = request.fromJarId,
            ToJarId = request.toJarId,

            // Core fields
            Type = request.type,
            TransactionsAmount = request.transactionsAmount,
            Note = request.note,
            TransactionDate = request.date,

            // Source metadata
            SourceType = "Manual",
            RawDescription = null,
            ExternalTransactionId = null,
            RawPayloadJson = null,
            JarBalanceAfterAllocation = null,

            // Posting
            PostedAt = null,
            ImportJobId = null,

            // Soft delete
            IsDeleted = false,
            DeletedAt = null,

            // Audit
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        _dbContext.Transactions.Add(transaction);
        if(transaction.Type == "Expense")
        {
            // Pay for something by selected Jar
            if (transaction.FromJarId != null  && transaction.ToJarId == null && transaction.FinancialAccountId == null)
            {
                var jar = _dbContext.Jars.FirstOrDefault(x => x.Id == transaction.FromJarId);
                if(jar.Balance - transaction.TransactionsAmount >= 0)
                {
                    jar.Balance = jar.Balance - transaction.TransactionsAmount;
                }
                else
                {
                    throw new Exception("Insufficient funds");
                }
            }
        }else if (transaction.Type == "Income")
        {
            if (transaction.ToJarId == null && transaction.FromJarId == null 
                                            && transaction.FinancialAccountId != null)
            {
                var financialAccount = _dbContext.FinancialAccounts.FirstOrDefault(x => x.Id == transaction.FinancialAccountId);
                financialAccount.CurrentBalance = financialAccount.CurrentBalance +  transaction.TransactionsAmount;
            }
        }else if (transaction.Type == "Transfer")
        {
            // Transfer from jar to jar
            if (transaction.FromJarId != null && transaction.ToJarId != null && transaction.FinancialAccountId == null)
            {
                var fromJar = _dbContext.Jars.FirstOrDefault(x => x.Id == transaction.FromJarId);
                var toJar = _dbContext.Jars.FirstOrDefault(x => x.Id == transaction.ToJarId);
                if (fromJar.Balance - transaction.TransactionsAmount >= 0)
                {
                    fromJar.Balance = fromJar.Balance - transaction.TransactionsAmount;
                    toJar.Balance = toJar.Balance + transaction.TransactionsAmount;
                }
                else
                {
                    throw new Exception("Insufficient funds");
                }
                
            }
            // Transfer from account to jar
            else if (transaction.FromJarId == null && transaction.ToJarId != null && 
                     transaction.FinancialAccountId != null)
            {
                var toJar = _dbContext.Jars.FirstOrDefault(x => x.Id == transaction.ToJarId);
                var finnacialAccount = _dbContext.FinancialAccounts.FirstOrDefault(x => x.Id == transaction.FinancialAccountId);
                if (finnacialAccount.CurrentBalance - transaction.TransactionsAmount >= 0)
                {
                    finnacialAccount.CurrentBalance = finnacialAccount.CurrentBalance - transaction.TransactionsAmount;
                    toJar.Balance = toJar.Balance + transaction.TransactionsAmount;
                }
                else
                {
                    throw new Exception("Insufficient funds");
                }
                
            }
            // Transfer from jar to account
            else if (transaction.FromJarId != null && transaction.ToJarId == null && 
                     transaction.FinancialAccountId != null)
            {
                var fromJar = _dbContext.Jars.FirstOrDefault(x => x.Id == transaction.ToJarId);
                var finnacialAccount = _dbContext.FinancialAccounts.FirstOrDefault(x => x.Id == transaction.FinancialAccountId);
                if (fromJar.Balance - transaction.TransactionsAmount >= 0)
                {
                    fromJar.Balance = fromJar.Balance - transaction.TransactionsAmount;
                    finnacialAccount.CurrentBalance = finnacialAccount.CurrentBalance + transaction.TransactionsAmount;
                }
                else
                {
                    throw new Exception("Insufficient funds");
                }
                
            }
        }
        
        await _dbContext.SaveChangesAsync();
        var result = new Response.CreateTransactionResponse
        {
            id = transaction.Id,
            type = transaction.Type,
            financialAccountId = request.financialAccountId,
            transactionsAmount = request.transactionsAmount,
            date = request.date,
        };
        return result;
    }

    public async Task<Response.UpdateTransactionResponse> UpdateTransaction(Guid id, Request.UpdateTransactionRequest request)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        var userIdGuid = Guid.Parse(userId);

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);
        if (user == null)
            throw new Exception("User not found");
        var transaction = await _dbContext.Transactions
            .FirstOrDefaultAsync(x => x.Id == id);
        if (transaction == null)
        {
            throw new Exception("Transaction not found");
        }
        // transaction.FinancialAccountId = request.financialAccountId ?? transaction.FinancialAccountId;
        // transaction.FromJarId = request.fromJarId ?? transaction.FromJarId;
        // transaction.ToJarId = request.toJarId ?? transaction.ToJarId;
        // transaction.TransactionDate = request.date;
        var newTransactionsAmount = request.transactionsAmount;
        var newCategoryId = request.categoryId;
        var newTransactionNote = request.note;
        if( request.transactionsAmount != null )
        {
            if (transaction.Type == "Expense")
            {
                if (transaction.FromJarId != null && transaction.ToJarId == null && transaction.FinancialAccountId == null)
                {
                    var fromJar = _dbContext.Jars.FirstOrDefault(x => x.Id == transaction.FromJarId);
                    if(fromJar == null) throw new Exception("Jar not found");
                    fromJar.Balance = fromJar.Balance + transaction.TransactionsAmount;
                    if (fromJar.Balance - newTransactionsAmount >= 0)
                    {
                        transaction.TransactionsAmount = (decimal)newTransactionsAmount;
                        fromJar.Balance = fromJar.Balance - transaction.TransactionsAmount;
                    }
                    else
                    {
                        throw new Exception("Insufficient funds");
                    }
                }
                
            }

            else if (transaction.Type == "Income")
            {
                if (transaction.FromJarId == null && transaction.ToJarId == null && transaction.FinancialAccountId != null)
                {
                    var financialAccount = _dbContext.FinancialAccounts.FirstOrDefault(x => x.Id == transaction.FinancialAccountId);
                    if (financialAccount == null) throw new Exception("Financial account not found");
                    var isUse = _dbContext.Transactions.Any(x => x.FinancialAccountId == financialAccount.Id);
                    if (isUse != null)
                    {
                        throw new Exception("The Income has been used!. The Change will terminated the existed money flow logic");
                    }
                    financialAccount.CurrentBalance = financialAccount.CurrentBalance -  transaction.TransactionsAmount;
                    transaction.TransactionsAmount = (decimal)newTransactionsAmount;
                    financialAccount.CurrentBalance = financialAccount.CurrentBalance + transaction.TransactionsAmount;
                }
            }
            
            else throw new Exception("Type not supported");
           
        }
        transaction.CategoryId = newCategoryId ?? transaction.CategoryId;
        transaction.Note = newTransactionNote ?? transaction.Note;
        // Đang sửa Update transaction trong đó update chỉ được số tiền, cate, Note. Nếu
        // Update tiền thì thu tiền mới và trả tiền cũ về chỗ
        // Vậy suy nghĩ đến bài toán chênh lệch Tiền cũ + (Khoảng mới - tiền cũ)
        // Khoảng mới nhập > tiền cũ thì thu được số dương vậy thì + vô tiền cũ là ra khoảng cần bù. Vise versa
        
        // Nguồn = Tiền cũ + (KHoảng mới -Tiền cũ)
        await _dbContext.SaveChangesAsync();
        var result = new Response.UpdateTransactionResponse
        {
            id = id,
            type = transaction.Type,
            transactionsAmount = transaction.TransactionsAmount,
            date = transaction.TransactionDate,
        };
        return result;
    }

    public async Task<Response.DeleteTransactionResponse> DeleteTransaction(Guid id)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        var userIdGuid = Guid.Parse(userId);

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);
        if (user == null)
            throw new Exception("User not found");
        var transaction = await _dbContext.Transactions
            .FirstOrDefaultAsync(x => x.Id == id);
        if (transaction == null)
        {
            throw new Exception("Transaction not found");
        }
        transaction.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        var result = new Response.DeleteTransactionResponse
        {
            message = "Transaction deleted"
        };
        return result;
    }
}