using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Entity;
using Personal_Finance_Management.Service.Base;
using Personal_Finance_Management.Service.Validations;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Personal_Finance_Management.Service.Transaction;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IConfiguration _configuration;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
        _configuration = configuration;
    }
    
    public async Task<Response.GetTransactionsResult> GetTransactions(Request.GetTransactionsRequest request)
    {
        var userIdGuid = GetCurrentUserId();

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);
        if (user == null)
            throw new Exception("User not found");

        if (request.pageIndex <= 0)
        {
            throw AppValidationException.BadRequest("Trang phải lớn hơn 0.", "pageIndex", "INVALID_PAGE_INDEX");
        }

        if (request.pageSize <= 0 || request.pageSize > 100)
        {
            throw AppValidationException.BadRequest("Số dòng mỗi trang phải từ 1 đến 100.", "pageSize", "INVALID_PAGE_SIZE");
        }
        
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

        var totalCount = await query.CountAsync();
        var selectedQuery = ProjectTransaction(query
            .Skip((request.pageIndex - 1) * request.pageSize)
            .Take(request.pageSize));
        var selectedPagination = new Response.PaginationResponse
        {
            page = request.pageIndex,
            pageSize = request.pageSize,
            totalCount = totalCount,
            totalPages = (totalCount + request.pageSize - 1) / request.pageSize,
        };
        
        var result = new Response.GetTransactionsResult
        {
            data = await selectedQuery.ToListAsync(),
            pagination = selectedPagination,
        };
        
        return result;
    }

    public async Task<Response.GetTransactionResponse> GetTransactionById(Guid id)
    {
        var userIdGuid = GetCurrentUserId();
        var result = await ProjectTransaction(_dbContext.Transactions
                .Where(x => x.Id == id && x.UserId == userIdGuid && !x.IsDeleted))
            .FirstOrDefaultAsync();

        if (result == null)
        {
            throw AppValidationException.NotFound("Không tìm thấy giao dịch.", "id", "TRANSACTION_NOT_FOUND");
        }

        return result;
    }

    public async Task<Response.CreateTransactionResponse> CreateTransaction(Request.CreateTransactionRequest request)
    {
        var userIdGuid = GetCurrentUserId();

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);
        if (user == null)
            throw new Exception("User not found");

        var now = DateTimeOffset.UtcNow;
        var transactionType = request.type?.Trim();
        if (transactionType != "Income" && transactionType != "Expense" && transactionType != "Transfer")
        {
            throw AppValidationException.BadRequest("Loại giao dịch không hợp lệ.", "type", "INVALID_TRANSACTION_TYPE");
        }

        if (request.transactionsAmount <= 0)
        {
            throw AppValidationException.BadRequest("Số tiền giao dịch phải lớn hơn 0.", "transactionsAmount", "INVALID_TRANSACTION_AMOUNT");
        }

        var transactionDate = transactionType == "Transfer" ? now : request.date;
        if (transactionType != "Transfer" && transactionDate > now)
        {
            throw AppValidationException.BadRequest("Không thể tạo giao dịch trong tương lai.", "date", "TRANSACTION_DATE_IN_FUTURE");
        }

        if (request.financialAccountId.HasValue)
        {
            var financialAccount = await _dbContext.FinancialAccounts
                .FirstOrDefaultAsync(x => x.Id == request.financialAccountId.Value && x.UserId == userIdGuid && x.IsActive);
            if (financialAccount == null)
            {
                throw AppValidationException.NotFound("Không tìm thấy tài khoản tài chính.", "financialAccountId", "FINANCIAL_ACCOUNT_NOT_FOUND");
            }

            if (financialAccount.ConnectionMode == "LinkedApi")
            {
                throw AppValidationException.BadRequest(
                    "Không thể tạo giao dịch thủ công cho tài khoản ngân hàng đã liên kết.",
                    "financialAccountId",
                    "LINKED_ACCOUNT_MANUAL_TRANSACTION_NOT_ALLOWED");
            }
        }

        if (request.categoryId.HasValue)
        {
            var categoryExists = await _dbContext.Categories.AnyAsync(x =>
                x.Id == request.categoryId.Value
                && x.IsActive
                && (x.OwnerUserId == null || x.OwnerUserId == userIdGuid));
            if (!categoryExists)
            {
                throw AppValidationException.NotFound("Không tìm thấy danh mục.", "categoryId", "CATEGORY_NOT_FOUND");
            }
        }

        await using var databaseTransaction = await _dbContext.Database.BeginTransactionAsync();

        var transaction = new Repository.Entity.Transaction()
        {
            // Identity
            UserId = userIdGuid,
            FinancialAccountId = request.financialAccountId,
            CategoryId = request.categoryId,
            FromJarId = request.fromJarId,
            ToJarId = request.toJarId,

            // Core fields
            Type = transactionType,
            TransactionsAmount = request.transactionsAmount,
            Note = request.note,
            TransactionDate = transactionDate,

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
            CreatedAt = now,
            UpdatedAt = now,
        };
        _dbContext.Transactions.Add(transaction);
        if(transaction.Type == "Expense")
        {
            // Pay for something by selected Jar
            if (transaction.FromJarId != null  && transaction.ToJarId == null && transaction.FinancialAccountId == null)
            {
                var jar = await _dbContext.Jars.FirstOrDefaultAsync(x => x.Id == transaction.FromJarId && x.UserId == userIdGuid);
                if (jar == null)
                {
                    throw AppValidationException.NotFound("Không tìm thấy hũ.", "fromJarId", "JAR_NOT_FOUND");
                }

                if (jar.Balance - transaction.TransactionsAmount >= 0)
                {
                    jar.Balance = jar.Balance - transaction.TransactionsAmount;
                }
                else
                {
                    throw AppValidationException.BadRequest(
                        "Số tiền trong hũ không đủ để thực hiện giao dịch.",
                        "fromJarId",
                        "INSUFFICIENT_JAR_BALANCE");
                }
            }
        }else if (transaction.Type == "Income")
        {
            if (transaction.ToJarId == null && transaction.FromJarId == null 
                                            && transaction.FinancialAccountId != null)
            {
                var financialAccount = await _dbContext.FinancialAccounts.FirstOrDefaultAsync(x => x.Id == transaction.FinancialAccountId && x.UserId == userIdGuid && x.IsActive);
                if (financialAccount == null)
                {
                    throw AppValidationException.NotFound("Không tìm thấy tài khoản tài chính.", "financialAccountId", "FINANCIAL_ACCOUNT_NOT_FOUND");
                }

                financialAccount.CurrentBalance = financialAccount.CurrentBalance +  transaction.TransactionsAmount;
            }
        }else if (transaction.Type == "Transfer") {
            // Transfer from jar to jar
            if (transaction.FromJarId != null && transaction.ToJarId != null && transaction.FinancialAccountId == null)
            {
                var fromJar = await _dbContext.Jars.FirstOrDefaultAsync(x => x.Id == transaction.FromJarId && x.UserId == userIdGuid);
                var toJar = await _dbContext.Jars.FirstOrDefaultAsync(x => x.Id == transaction.ToJarId && x.UserId == userIdGuid);
                if (fromJar == null)
                {
                    throw AppValidationException.NotFound("Không tìm thấy hũ nguồn.", "fromJarId", "JAR_NOT_FOUND");
                }
                if (toJar == null)
                {
                    throw AppValidationException.NotFound("Không tìm thấy hũ đích.", "toJarId", "JAR_NOT_FOUND");
                }
                if (fromJar.Id == toJar.Id)
                {
                    throw AppValidationException.BadRequest("Hũ nguồn và hũ đích phải khác nhau.", "toJarId", "INVALID_TRANSFER_TARGET");
                }

                if (fromJar.Balance - transaction.TransactionsAmount >= 0)
                {
                    fromJar.Balance = fromJar.Balance - transaction.TransactionsAmount;
                    toJar.Balance = toJar.Balance + transaction.TransactionsAmount;
                }
                else
                {
                    throw AppValidationException.BadRequest(
                        "Số tiền trong hũ nguồn không đủ để chuyển tiền.",
                        "fromJarId",
                        "INSUFFICIENT_JAR_BALANCE");
                }
                
            }
            // Transfer from account to jar
            else if (transaction.FromJarId == null && transaction.ToJarId != null && 
                     transaction.FinancialAccountId != null)
            {
                var toJar = await _dbContext.Jars.FirstOrDefaultAsync(x => x.Id == transaction.ToJarId && x.UserId == userIdGuid);
                var finnacialAccount = await _dbContext.FinancialAccounts.FirstOrDefaultAsync(x => x.Id == transaction.FinancialAccountId && x.UserId == userIdGuid && x.IsActive);
                if (toJar == null)
                {
                    throw AppValidationException.NotFound("Không tìm thấy hũ đích.", "toJarId", "JAR_NOT_FOUND");
                }
                if (finnacialAccount == null)
                {
                    throw AppValidationException.NotFound("Không tìm thấy tài khoản tài chính.", "financialAccountId", "FINANCIAL_ACCOUNT_NOT_FOUND");
                }

                if (finnacialAccount.CurrentBalance - transaction.TransactionsAmount >= 0)
                {
                    finnacialAccount.CurrentBalance = finnacialAccount.CurrentBalance - transaction.TransactionsAmount;
                    toJar.Balance = toJar.Balance + transaction.TransactionsAmount;
                }
                else
                {
                    throw AppValidationException.BadRequest(
                        "Số dư tài khoản không đủ để chuyển vào hũ.",
                        "financialAccountId",
                        "INSUFFICIENT_ACCOUNT_BALANCE");
                }
                
            }
            // Transfer from jar to account
            else if (transaction.FromJarId != null && transaction.ToJarId == null && 
                     transaction.FinancialAccountId != null)
            {
                var fromJar = await _dbContext.Jars.FirstOrDefaultAsync(x => x.Id == transaction.FromJarId && x.UserId == userIdGuid);
                var finnacialAccount = await _dbContext.FinancialAccounts.FirstOrDefaultAsync(x => x.Id == transaction.FinancialAccountId && x.UserId == userIdGuid && x.IsActive);
                if (fromJar == null)
                {
                    throw AppValidationException.NotFound("Không tìm thấy hũ nguồn.", "fromJarId", "JAR_NOT_FOUND");
                }
                if (finnacialAccount == null)
                {
                    throw AppValidationException.NotFound("Không tìm thấy tài khoản tài chính.", "financialAccountId", "FINANCIAL_ACCOUNT_NOT_FOUND");
                }

                if (fromJar.Balance - transaction.TransactionsAmount >= 0)
                {
                    fromJar.Balance = fromJar.Balance - transaction.TransactionsAmount;
                    finnacialAccount.CurrentBalance = finnacialAccount.CurrentBalance + transaction.TransactionsAmount;
                }
                else
                {
                    throw AppValidationException.BadRequest(
                        "Số tiền trong hũ nguồn không đủ để chuyển về tài khoản.",
                        "fromJarId",
                        "INSUFFICIENT_JAR_BALANCE");
                }
                
            }
            else
            {
                throw AppValidationException.BadRequest("Thông tin chuyển tiền không hợp lệ.", "type", "INVALID_TRANSFER_TARGET");
            }
        }
        
        await _dbContext.SaveChangesAsync();

        // Evaluate goals for affected jars
        if (transaction.ToJarId != null)
        {
            await CheckAndCompleteGoals(transaction.ToJarId.Value);
        }
        if (transaction.FromJarId != null)
        {
            await CheckJarLimit(transaction.FromJarId.Value);
        }
        if (transaction.CategoryId != null)
        {
            await CheckCategoryLimit(transaction.CategoryId.Value, userIdGuid);
        }

        await databaseTransaction.CommitAsync();

        var result = new Response.CreateTransactionResponse
        {
            id = transaction.Id,
            type = transaction.Type,
            financialAccountId = request.financialAccountId,
            transactionsAmount = request.transactionsAmount,
            date = transaction.TransactionDate,
        };
        return result;
    }

    public async Task<Response.UpdateTransactionResponse> UpdateTransaction(Guid id, Request.UpdateTransactionRequest request)
    {
        var userIdGuid = GetCurrentUserId();

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);
        if (user == null)
            throw new Exception("User not found");
        var transaction = await _dbContext.Transactions
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (transaction == null)
        {
            throw new Exception("Transaction not found");
        }
        if (transaction.UserId != userIdGuid)
        {
            throw AppValidationException.NotFound("Transaction not found.", "id", "TRANSACTION_NOT_FOUND");
        }

        if (transaction.SourceType != "Manual")
        {
            throw AppValidationException.BadRequest(
                "Linked bank transaction cannot be updated manually.",
                "id",
                "LINKED_TRANSACTION_UPDATE_NOT_ALLOWED");
        }

        // transaction.FinancialAccountId = request.financialAccountId ?? transaction.FinancialAccountId;
        // transaction.FromJarId = request.fromJarId ?? transaction.FromJarId;
        // transaction.ToJarId = request.toJarId ?? transaction.ToJarId;
        // transaction.TransactionDate = request.date;
        var newTransactionsAmount = request.transactionsAmount;
        var newCategoryId = request.categoryId;
        var newTransactionNote = request.note;

        if (newTransactionsAmount.HasValue && newTransactionsAmount.Value <= 0)
        {
            throw AppValidationException.BadRequest("Số tiền giao dịch phải lớn hơn 0.", "transactionsAmount", "INVALID_TRANSACTION_AMOUNT");
        }

        if (newCategoryId.HasValue)
        {
            var categoryExists = await _dbContext.Categories.AnyAsync(x =>
                x.Id == newCategoryId.Value
                && x.IsActive
                && (x.OwnerUserId == null || x.OwnerUserId == userIdGuid));
            if (!categoryExists)
            {
                throw AppValidationException.NotFound("Không tìm thấy danh mục.", "categoryId", "CATEGORY_NOT_FOUND");
            }
        }

        var oldCategoryId = transaction.CategoryId;
        await using var databaseTransaction = await _dbContext.Database.BeginTransactionAsync();

        if (newTransactionsAmount.HasValue)
        {
            var updatedAmount = newTransactionsAmount.Value;
            if (transaction.Type == "Expense")
            {
                if (transaction.FromJarId != null && transaction.ToJarId == null && transaction.FinancialAccountId == null)
                {
                    var fromJar = await _dbContext.Jars.FirstOrDefaultAsync(x => x.Id == transaction.FromJarId && x.UserId == userIdGuid);
                    if(fromJar == null)
                    {
                        throw AppValidationException.NotFound("Jar not found.", "fromJarId", "JAR_NOT_FOUND");
                    }
                    fromJar.Balance = fromJar.Balance + transaction.TransactionsAmount;
                    if (fromJar.Balance - updatedAmount >= 0)
                    {
                        transaction.TransactionsAmount = updatedAmount;
                        fromJar.Balance = fromJar.Balance - transaction.TransactionsAmount;
                    }
                    else
                    {
                        throw AppValidationException.BadRequest(
                            "Số tiền trong hũ không đủ để cập nhật giao dịch.",
                            "transactionsAmount",
                            "INSUFFICIENT_JAR_BALANCE");
                    }
                }
                
            }

            else if (transaction.Type == "Income")
            {
                if (transaction.FromJarId == null && transaction.ToJarId == null && transaction.FinancialAccountId != null)
                {
                    var financialAccount = await _dbContext.FinancialAccounts.FirstOrDefaultAsync(x => x.Id == transaction.FinancialAccountId && x.UserId == userIdGuid && x.IsActive);
                    if (financialAccount == null)
                    {
                        throw AppValidationException.NotFound("Financial account not found.", "financialAccountId", "FINANCIAL_ACCOUNT_NOT_FOUND");
                    }
                    var isUse = await _dbContext.Transactions.AnyAsync(x =>
                        x.FinancialAccountId == financialAccount.Id
                        && x.UserId == userIdGuid
                        && x.Id != transaction.Id
                        && x.Type != "Income"
                        && !x.IsDeleted);
                    if (isUse)
                    {
                        throw new Exception("The Income has been used!. The Change will terminated the existed money flow logic");
                    }
                    financialAccount.CurrentBalance = financialAccount.CurrentBalance -  transaction.TransactionsAmount;
                    transaction.TransactionsAmount = updatedAmount;
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
        transaction.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();

        // Evaluate goals for affected jars
        if (transaction.ToJarId != null)
        {
            await CheckAndCompleteGoals(transaction.ToJarId.Value);
        }
        if (transaction.FromJarId != null)
        {
            await CheckAndCompleteGoals(transaction.FromJarId.Value);
        }
        if (transaction.Type == "Expense" && transaction.FromJarId != null)
        {
            await CheckJarLimit(transaction.FromJarId.Value);
        }
        if (transaction.Type == "Expense" && transaction.CategoryId != null)
        {
            await CheckCategoryLimit(transaction.CategoryId.Value, userIdGuid);
        }
        if (transaction.Type == "Expense"
            && oldCategoryId.HasValue
            && oldCategoryId != transaction.CategoryId)
        {
            await CheckCategoryLimit(oldCategoryId.Value, userIdGuid);
        }

        await databaseTransaction.CommitAsync();

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
        var userIdGuid = GetCurrentUserId();

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);
        if (user == null)
            throw new Exception("User not found");
        var transaction = await _dbContext.Transactions
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (transaction == null)
        {
            throw new Exception("Transaction not found");
        }
        if (transaction.UserId != userIdGuid)
        {
            throw AppValidationException.NotFound("Transaction not found.", "id", "TRANSACTION_NOT_FOUND");
        }

        if (transaction.SourceType != "Manual")
        {
            throw AppValidationException.BadRequest(
                "Linked bank transaction cannot be deleted manually.",
                "id",
                "LINKED_TRANSACTION_DELETE_NOT_ALLOWED");
        }

        await using var databaseTransaction = await _dbContext.Database.BeginTransactionAsync();

        if (!transaction.IsDeleted)
        {
            if (transaction.Type == "Expense")
            {
                if (transaction.FromJarId != null && transaction.ToJarId == null && transaction.FinancialAccountId == null)
                {
                    var fromJar = await _dbContext.Jars.FirstOrDefaultAsync(x => x.Id == transaction.FromJarId && x.UserId == userIdGuid);
                    if (fromJar == null)
                    {
                        throw AppValidationException.NotFound("Jar not found.", "fromJarId", "JAR_NOT_FOUND");
                    }

                    fromJar.Balance += transaction.TransactionsAmount;
                    fromJar.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }
            else if (transaction.Type == "Income")
            {
                if (transaction.FromJarId == null && transaction.ToJarId == null && transaction.FinancialAccountId != null)
                {
                    var financialAccount = await _dbContext.FinancialAccounts.FirstOrDefaultAsync(x => x.Id == transaction.FinancialAccountId && x.UserId == userIdGuid && x.IsActive);
                    if (financialAccount == null)
                    {
                        throw AppValidationException.NotFound("Financial account not found.", "financialAccountId", "FINANCIAL_ACCOUNT_NOT_FOUND");
                    }

                    financialAccount.CurrentBalance -= transaction.TransactionsAmount;
                    financialAccount.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }
            else if (transaction.Type == "Transfer")
            {
                if (transaction.FromJarId != null && transaction.ToJarId != null && transaction.FinancialAccountId == null)
                {
                    var fromJar = await _dbContext.Jars.FirstOrDefaultAsync(x => x.Id == transaction.FromJarId && x.UserId == userIdGuid);
                    var toJar = await _dbContext.Jars.FirstOrDefaultAsync(x => x.Id == transaction.ToJarId && x.UserId == userIdGuid);
                    if (fromJar == null)
                    {
                        throw AppValidationException.NotFound("Jar not found.", "fromJarId", "JAR_NOT_FOUND");
                    }
                    if (toJar == null)
                    {
                        throw AppValidationException.NotFound("Jar not found.", "toJarId", "JAR_NOT_FOUND");
                    }

                    if (toJar.Balance - transaction.TransactionsAmount < 0)
                    {
                        throw AppValidationException.BadRequest(
                            "Số tiền trong hũ đích không đủ để hoàn tác giao dịch.",
                            "toJarId",
                            "INSUFFICIENT_JAR_BALANCE");
                    }

                    toJar.Balance -= transaction.TransactionsAmount;
                    fromJar.Balance += transaction.TransactionsAmount;
                    toJar.UpdatedAt = DateTimeOffset.UtcNow;
                    fromJar.UpdatedAt = DateTimeOffset.UtcNow;
                }
                else if (transaction.FromJarId == null && transaction.ToJarId != null && transaction.FinancialAccountId != null)
                {
                    var toJar = await _dbContext.Jars.FirstOrDefaultAsync(x => x.Id == transaction.ToJarId && x.UserId == userIdGuid);
                    var financialAccount = await _dbContext.FinancialAccounts.FirstOrDefaultAsync(x => x.Id == transaction.FinancialAccountId && x.UserId == userIdGuid && x.IsActive);
                    if (toJar == null)
                    {
                        throw AppValidationException.NotFound("Jar not found.", "toJarId", "JAR_NOT_FOUND");
                    }
                    if (financialAccount == null)
                    {
                        throw AppValidationException.NotFound("Financial account not found.", "financialAccountId", "FINANCIAL_ACCOUNT_NOT_FOUND");
                    }
                    if (toJar.Balance - transaction.TransactionsAmount < 0)
                    {
                        throw AppValidationException.BadRequest(
                            "Số tiền trong hũ đích không đủ để hoàn tác giao dịch.",
                            "toJarId",
                            "INSUFFICIENT_JAR_BALANCE");
                    }

                    toJar.Balance -= transaction.TransactionsAmount;
                    financialAccount.CurrentBalance += transaction.TransactionsAmount;
                    toJar.UpdatedAt = DateTimeOffset.UtcNow;
                    financialAccount.UpdatedAt = DateTimeOffset.UtcNow;
                }
                else if (transaction.FromJarId != null && transaction.ToJarId == null && transaction.FinancialAccountId != null)
                {
                    var fromJar = await _dbContext.Jars.FirstOrDefaultAsync(x => x.Id == transaction.FromJarId && x.UserId == userIdGuid);
                    var financialAccount = await _dbContext.FinancialAccounts.FirstOrDefaultAsync(x => x.Id == transaction.FinancialAccountId && x.UserId == userIdGuid && x.IsActive);
                    if (fromJar == null)
                    {
                        throw AppValidationException.NotFound("Jar not found.", "fromJarId", "JAR_NOT_FOUND");
                    }
                    if (financialAccount == null)
                    {
                        throw AppValidationException.NotFound("Financial account not found.", "financialAccountId", "FINANCIAL_ACCOUNT_NOT_FOUND");
                    }

                    financialAccount.CurrentBalance -= transaction.TransactionsAmount;
                    fromJar.Balance += transaction.TransactionsAmount;
                    financialAccount.UpdatedAt = DateTimeOffset.UtcNow;
                    fromJar.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }
        }

        transaction.IsDeleted = true;
        transaction.DeletedAt = DateTimeOffset.UtcNow;
        transaction.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();

        // Evaluate goals for affected jars (deletion might refund money if it was an expense)
        if (transaction.ToJarId != null)
        {
            await CheckAndCompleteGoals(transaction.ToJarId.Value);
        }
        if (transaction.FromJarId != null)
        {
            await CheckAndCompleteGoals(transaction.FromJarId.Value);
        }
        if (transaction.Type == "Expense" && transaction.FromJarId != null)
        {
            await CheckJarLimit(transaction.FromJarId.Value);
        }
        if (transaction.Type == "Expense" && transaction.CategoryId != null)
        {
            await CheckCategoryLimit(transaction.CategoryId.Value, userIdGuid);
        }
        await databaseTransaction.CommitAsync();

        var result = new Response.DeleteTransactionResponse
        {
            message = "Transaction deleted"
        };
        return result;
    }

    private async Task CheckAndCompleteGoals(Guid jarId)
    {
        var jar = await _dbContext.Jars.FirstOrDefaultAsync(j => j.Id == jarId);
        if (jar == null) return;

        // Tìm các Goal đang Active gắn với Jar này
        var activeGoals = await _dbContext.Goals
            .Where(g => g.LinkedJarId == jarId && g.Status == "Active")
            .ToListAsync();

        foreach (var goal in activeGoals)
        {
            // Kiểm tra điều kiện hoàn thành: Số dư hũ >= Target Amount
            // Theo Option B: Chấp nhận hoàn thành cả khi đã quá hạn (DueDate)
            if (jar.Balance >= goal.TargetAmount)
            {
                goal.Status = "Completed";
                goal.UpdatedAt = DateTimeOffset.UtcNow;

                // Tạo thông báo cho người dùng
                var notification = new Notification
                {
                    UserId = goal.UserId,
                    Type = "GoalUpdate",
                    Title = "Chúc mừng! Bạn đã hoàn thành mục tiêu",
                    Body = $"Mục tiêu '{goal.Title}' đã đạt được mức {goal.TargetAmount:N0}đ trong hũ {jar.Name}.",
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow,
                    MetadataJson = $"{{\"goalId\": \"{goal.Id}\", \"jarId\": \"{jar.Id}\"}}"
                };

                _dbContext.Notifications.Add(notification);
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    private static IQueryable<Response.GetTransactionResponse> ProjectTransaction(IQueryable<Repository.Entity.Transaction> query)
    {
        return query.Select(x => new Response.GetTransactionResponse
        {
            id = x.Id,
            type = x.Type,
            transactionsAmount = x.TransactionsAmount,
            note = x.Note,
            date = x.TransactionDate,
            financialAccount = new Response.TransactionFinancialAccountResponse
            {
                id = x.FinancialAccountId,
                name = x.FinancialAccount != null ? x.FinancialAccount.Name : null,
            },
            jar = x.FromJarId == null && x.ToJarId == null
                ? null
                : new Response.TransactionJarResponse
                {
                    id = x.FromJarId ?? x.ToJarId,
                    name = x.FromJar != null
                        ? x.FromJar.Name
                        : x.ToJar != null
                            ? x.ToJar.Name
                            : null,
                },
            category = x.CategoryId == null
                ? null
                : new Response.TransactionCategoryResponse
                {
                    id = x.CategoryId,
                    name = x.Category != null ? x.Category.Name : null,
                }
        });
    }

    private async Task CheckJarLimit(Guid jarId)
    {
        var jar = await _dbContext.Jars.FirstOrDefaultAsync(j => j.Id == jarId);
        if (jar == null) return;

        var activeLimit = await _dbContext.SpendingLimits
            .Where(x => x.JarId == jarId && x.IsActive == true)
            .ToListAsync();

        foreach (var item in activeLimit)
        {
            var currentSpent = await GetCurrentSpentByJar(jarId, item.UserId, item.ResetAt);
            var alertThreshold = item.LimitAmount * item.AlertAtPercentage / 100;

            // Business rule:
            // - Alert threshold only creates warning notification and keeps limit active.
            // - Exceeded limit creates exceeded notification and deactivates the limit.
            // Exceeded has priority, so a transaction crossing directly to limit amount only creates exceeded notification.
            if (currentSpent >= item.LimitAmount)
            {
               
                var notification = new Repository.Entity.Notification()
                {
                    UserId = item.UserId,
                    Type = "SpendingAlert",
                    Title = "Thông báo vượt ngưỡng!",
                    Body = $"Xin thông báo! bạn đã chạm ngưỡng {item.LimitAmount}đ giới hạn chi tiêu ở hũ {jar.Name}",
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow,
                    MetadataJson = $"{{\"limitId\": \"{item.Id}\", \"targetType\": \"Jar\", \"jarId\": \"{jar.Id}\", \"thresholdType\": \"Exceeded\"}}"

                };
                if (await HasLimitNotification(item.UserId, item.Id, "Jar", jar.Id, notification.Body)) continue;
                item.UpdatedAt = DateTimeOffset.UtcNow;
                _dbContext.Notifications.Add(notification);
            }
            else if (currentSpent >= alertThreshold)
            {
                var notification = new Repository.Entity.Notification()
                {
                    UserId = item.UserId,
                    Type = "SpendingAlert",
                    Title = "Thông báo vượt ngưỡng!",
                    Body = $"Xin thông báo! bạn đã chạm ngưỡng thông báo {item.AlertAtPercentage}% giới hạn chi tiêu ở hũ {jar.Name}",
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow,
                    MetadataJson = $"{{\"limitId\": \"{item.Id}\", \"targetType\": \"Jar\", \"jarId\": \"{jar.Id}\", \"thresholdType\": \"Alert\"}}"
                };
                if (await HasLimitNotification(item.UserId, item.Id, "Jar", jar.Id, notification.Body)) continue;
                _dbContext.Notifications.Add(notification);
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task<decimal> GetCurrentSpentByJar(Guid jarId, Guid userId, DateTimeOffset resetAt)
    {
        return (await _dbContext.Transactions
            .Where(t => t.UserId == userId
                        && !t.IsDeleted
                        && t.Type == "Expense"
                        && t.FromJarId == jarId
                        && t.ToJarId == null
                        && t.FinancialAccountId == null
                        && t.TransactionDate >= resetAt)
            .SumAsync(t => (decimal?)t.TransactionsAmount)) ?? 0m;
    }

    private async Task CheckCategoryLimit(Guid categoryId, Guid userId)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c =>
            c.Id == categoryId
            && c.IsActive
            && (c.OwnerUserId == null || c.OwnerUserId == userId));
        if (category == null) return;

        var activeLimit = await _dbContext.SpendingLimits
            .Where(x => x.UserId == userId && x.CategoryId == categoryId && x.IsActive)
            .ToListAsync();

        foreach (var item in activeLimit)
        {
            var currentSpent = await GetCurrentSpentByCategory(categoryId, item.UserId, item.ResetAt);
            var alertThreshold = item.LimitAmount * item.AlertAtPercentage / 100;

            if (currentSpent >= item.LimitAmount)
            {
                var notification = new Repository.Entity.Notification()
                {
                    UserId = item.UserId,
                    Type = "SpendingAlert",
                    Title = "Thong bao vuot nguong!",
                    Body = $"Ban da cham nguong {item.LimitAmount} gioi han chi tieu o danh muc {category.Name}",
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow,
                    MetadataJson = $"{{\"limitId\": \"{item.Id}\", \"targetType\": \"Category\", \"categoryId\": \"{category.Id}\", \"thresholdType\": \"Exceeded\"}}"
                };
                if (await HasLimitNotification(item.UserId, item.Id, "Category", category.Id, notification.Body)) continue;
                item.UpdatedAt = DateTimeOffset.UtcNow;
                _dbContext.Notifications.Add(notification);
            }
            else if (currentSpent >= alertThreshold)
            {
                var notification = new Repository.Entity.Notification()
                {
                    UserId = item.UserId,
                    Type = "SpendingAlert",
                    Title = "Thong bao vuot nguong!",
                    Body = $"Ban da cham nguong thong bao {item.AlertAtPercentage}% gioi han chi tieu o danh muc {category.Name}",
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow,
                    MetadataJson = $"{{\"limitId\": \"{item.Id}\", \"targetType\": \"Category\", \"categoryId\": \"{category.Id}\", \"thresholdType\": \"Alert\"}}"
                };
                if (await HasLimitNotification(item.UserId, item.Id, "Category", category.Id, notification.Body)) continue;
                _dbContext.Notifications.Add(notification);
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task<decimal> GetCurrentSpentByCategory(Guid categoryId, Guid userId, DateTimeOffset resetAt)
    {
        return (await _dbContext.Transactions
            .Where(t => t.UserId == userId
                        && !t.IsDeleted
                        && t.Type == "Expense"
                        && t.CategoryId == categoryId
                        && t.TransactionDate >= resetAt)
            .SumAsync(t => (decimal?)t.TransactionsAmount)) ?? 0m;
    }

    private async Task AddLimitNotificationIfNotExists(
        SpendingLimit limit,
        Repository.Entity.Jar jar,
        string title,
        string body)
    {
        if (await HasLimitNotification(limit.UserId, limit.Id, "Jar", jar.Id, body))
            return;

        var notification = new Notification
        {
            UserId = limit.UserId,
            Type = "SpendingAlert",
            Title = title,
            Body = body,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
            MetadataJson = $"{{\"limitId\": \"{limit.Id}\", \"targetType\": \"Jar\", \"jarId\": \"{jar.Id}\"}}"
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();
    }

    private async Task<bool> HasLimitNotification(Guid userId, Guid limitId, string targetType, Guid targetId, string body)
    {
        var limitIdMarker = $"\"limitId\": \"{limitId}\"";
        var targetIdMarker = targetType == "Category"
            ? $"\"categoryId\": \"{targetId}\""
            : $"\"jarId\": \"{targetId}\"";

        var pendingExists = _dbContext.ChangeTracker
            .Entries<Notification>()
            .Any(entry => entry.State == EntityState.Added
                          && entry.Entity.UserId == userId
                          && entry.Entity.Type == "SpendingAlert"
                          && entry.Entity.Body == body
                          && entry.Entity.MetadataJson != null
                          && entry.Entity.MetadataJson.Contains(limitIdMarker)
                          && entry.Entity.MetadataJson.Contains(targetIdMarker));

        if (pendingExists)
            return true;

        var metadataList = await _dbContext.Notifications
            .Where(n => n.UserId == userId
                        && n.Type == "SpendingAlert"
                        && n.Body == body)
            .Select(n => n.MetadataJson)
            .ToListAsync();

        return metadataList.Any(metadata =>
            metadata != null
            && metadata.Contains(limitIdMarker)
            && metadata.Contains(targetIdMarker));
    }
    
    public async Task<Response.CassoTransactionsResponse> ProcessCassoWebhook(
        Request.CassoWebhookRequest request,
        string? secureToken,
        string? cassoSignature)
    {
        if (request is null)
        {
            throw AppValidationException.BadRequest("Request body is required.", "body", "REQUIRED");
        }

        var configuredSecureToken = _configuration["CasooOptions:SecureToken"]
                                    ?? _configuration["CasooOptions:WebhookSecureToken"]
                                    ?? _configuration["Casso:SecureToken"]
                                    ?? _configuration["Casso:WebhookSecureToken"];
        if (!string.IsNullOrWhiteSpace(configuredSecureToken)
            && secureToken?.Trim() != configuredSecureToken.Trim())
        {
            throw AppValidationException.BadRequest("Invalid Casso secure token.", "secure-token", "CASSO_WEBHOOK_UNAUTHORIZED");
        }

        if (string.IsNullOrWhiteSpace(configuredSecureToken) && string.IsNullOrWhiteSpace(cassoSignature))
        {
            throw AppValidationException.BadRequest("Casso webhook verification header is required.", "secure-token", "CASSO_WEBHOOK_UNAUTHORIZED");
        }

        if (request.error != 0)
        {
            return new Response.CassoTransactionsResponse
            {
                receivedCount = 0,
                createdCount = 0,
                skippedCount = 0,
                message = "Casso webhook ignored because error is not zero."
            };
        }

        var cassoTransactions = new List<JsonElement>();
        if (request.data.ValueKind == JsonValueKind.Object)
        {
            cassoTransactions.Add(request.data);
        }
        else if (request.data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in request.data.EnumerateArray())
            {
                cassoTransactions.Add(item);
            }
        }
        else
        {
            throw AppValidationException.BadRequest("Casso webhook data is invalid.", "data", "CASSO_WEBHOOK_INVALID");
        }

        var createdCount = 0;
        var skippedCount = 0;
        await using var databaseTransaction = await _dbContext.Database.BeginTransactionAsync();

        foreach (var item in cassoTransactions)
        {
            decimal amount;
            if (item.TryGetProperty("amount", out var amountElement) && amountElement.ValueKind == JsonValueKind.Number)
            {
                amount = amountElement.GetDecimal();
            }
            else
            {
                skippedCount++;
                continue;
            }

            if (amount == 0)
            {
                skippedCount++;
                continue;
            }

            string? externalTransactionId = null;
            if (item.TryGetProperty("reference", out var referenceElement))
            {
                externalTransactionId = referenceElement.GetString();
            }
            if (string.IsNullOrWhiteSpace(externalTransactionId) && item.TryGetProperty("tid", out var tidElement))
            {
                externalTransactionId = tidElement.GetString();
            }
            if (string.IsNullOrWhiteSpace(externalTransactionId) && item.TryGetProperty("id", out var idElement))
            {
                externalTransactionId = idElement.ValueKind == JsonValueKind.String
                    ? idElement.GetString()
                    : idElement.GetRawText();
            }

            if (string.IsNullOrWhiteSpace(externalTransactionId))
            {
                skippedCount++;
                continue;
            }

            string? accountRef = null;
            if (item.TryGetProperty("accountNumber", out var accountNumberElement))
            {
                accountRef = accountNumberElement.GetString();
            }
            if (string.IsNullOrWhiteSpace(accountRef) && item.TryGetProperty("subAccId", out var subAccIdElement))
            {
                accountRef = subAccIdElement.GetString();
            }
            if (string.IsNullOrWhiteSpace(accountRef) && item.TryGetProperty("bank_sub_acc_id", out var bankSubAccIdElement))
            {
                accountRef = bankSubAccIdElement.GetString();
            }
            if (string.IsNullOrWhiteSpace(accountRef) && item.TryGetProperty("bankSubAccId", out var bankSubAccIdCamelElement))
            {
                accountRef = bankSubAccIdCamelElement.GetString();
            }

            if (string.IsNullOrWhiteSpace(accountRef))
            {
                skippedCount++;
                continue;
            }

            var matchedAccounts = await _dbContext.FinancialAccounts
                .Where(x => x.ConnectionMode == "LinkedApi"
                            && x.IsActive
                            && (x.ExternalAccountRef == accountRef
                                || x.MaskedAccountNumber == accountRef
                                || x.ExternalAccountId == accountRef))
                .ToListAsync();

            if (matchedAccounts.Count > 1)
            {
                throw AppValidationException.Conflict("Multiple linked financial accounts match Casso account.", "accountNumber", "CASSO_ACCOUNT_CONFLICT");
            }

            if (matchedAccounts.Count == 0)
            {
                skippedCount++;
                continue;
            }

            var financialAccount = matchedAccounts[0];
            var existedTransaction = await _dbContext.Transactions.AnyAsync(x =>
                x.FinancialAccountId == financialAccount.Id
                && x.ExternalTransactionId == externalTransactionId
                && !x.IsDeleted);
            if (existedTransaction)
            {
                skippedCount++;
                continue;
            }

            var transactionDate = DateTimeOffset.UtcNow;
            if (item.TryGetProperty("transactionDateTime", out var transactionDateTimeElement)
                && DateTimeOffset.TryParse(transactionDateTimeElement.GetString(), out var parsedTransactionDateTime))
            {
                transactionDate = parsedTransactionDateTime.ToUniversalTime();
            }
            else if (item.TryGetProperty("when", out var whenElement)
                     && DateTimeOffset.TryParse(whenElement.GetString(), out var parsedWhen))
            {
                transactionDate = parsedWhen.ToUniversalTime();
            }

            string? description = null;
            if (item.TryGetProperty("description", out var descriptionElement))
            {
                description = descriptionElement.GetString();
            }

            decimal? runningBalance = null;
            if (item.TryGetProperty("runningBalance", out var runningBalanceElement)
                && runningBalanceElement.ValueKind == JsonValueKind.Number)
            {
                runningBalance = runningBalanceElement.GetDecimal();
            }
            else if (item.TryGetProperty("cusum_balance", out var cusumBalanceElement)
                     && cusumBalanceElement.ValueKind == JsonValueKind.Number)
            {
                runningBalance = cusumBalanceElement.GetDecimal();
            }

            _dbContext.Transactions.Add(new Repository.Entity.Transaction
            {
                Id = Guid.NewGuid(),
                UserId = financialAccount.UserId,
                FinancialAccountId = financialAccount.Id,
                CategoryId = null,
                FromJarId = null,
                ToJarId = null,
                Type = amount > 0 ? "Income" : "Expense",
                TransactionsAmount = Math.Abs(amount),
                Note = description,
                RawDescription = description,
                TransactionDate = transactionDate,
                SourceType = "Imported",
                ExternalTransactionId = externalTransactionId,
                RawPayloadJson = item.GetRawText(),
                PostedAt = DateTimeOffset.UtcNow,
                ImportJobId = null,
                IsDeleted = false,
                DeletedAt = null,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

            if (runningBalance.HasValue)
            {
                financialAccount.CurrentBalance = runningBalance.Value;
            }
            else if (amount > 0)
            {
                financialAccount.CurrentBalance += Math.Abs(amount);
            }
            else
            {
                financialAccount.CurrentBalance -= Math.Abs(amount);
            }

            financialAccount.SyncStatus = "Synced";
            financialAccount.LastSyncedAt = DateTimeOffset.UtcNow;
            financialAccount.LastSyncError = null;
            financialAccount.LastSyncCursor = externalTransactionId;
            financialAccount.UpdatedAt = DateTimeOffset.UtcNow;
            createdCount++;
        }

        await _dbContext.SaveChangesAsync();
        await databaseTransaction.CommitAsync();

        return new Response.CassoTransactionsResponse
        {
            receivedCount = cassoTransactions.Count,
            createdCount = createdCount,
            skippedCount = skippedCount,
            message = "Casso webhook processed."
        };
    }

    public async Task<Response.CassoTransactionsResponse> SyncCassoTransactions(Request.CassoSyncTransactionsRequest request)
    {
        if (request is null)
        {
            throw AppValidationException.BadRequest("Request body is required.", "body", "REQUIRED");
        }

        var userIdGuid = GetCurrentUserId();
        if (request.financialAccountId == Guid.Empty)
        {
            throw AppValidationException.BadRequest("Financial account is required.", "financialAccountId", "REQUIRED");
        }

        if (request.page <= 0)
        {
            throw AppValidationException.BadRequest("Page must be greater than zero.", "page", "CASSO_SYNC_INVALID");
        }

        if (request.pageSize <= 0 || request.pageSize > 100)
        {
            throw AppValidationException.BadRequest("Page size must be between 1 and 100.", "pageSize", "CASSO_SYNC_INVALID");
        }

        var financialAccount = await _dbContext.FinancialAccounts
            .FirstOrDefaultAsync(x => x.Id == request.financialAccountId && x.UserId == userIdGuid && x.IsActive);
        if (financialAccount == null)
        {
            throw AppValidationException.NotFound("Financial account not found.", "financialAccountId", "FINANCIAL_ACCOUNT_NOT_FOUND");
        }

        if (financialAccount.ConnectionMode != "LinkedApi")
        {
            throw AppValidationException.BadRequest("Only linked bank account can sync Casso transactions.", "financialAccountId", "CASSO_SYNC_LINKED_ACCOUNT_REQUIRED");
        }

        var apiKey = _configuration["CasooOptions:ApiKey"] ?? _configuration["Casso:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw AppValidationException.BadRequest("Casso API key is not configured.", "CasooOptions:ApiKey", "CASSO_CONFIG_MISSING");
        }

        var baseUrlTransactions = _configuration["CasooOptions:BaseUrlTransactions"] ?? _configuration["Casso:BaseUrlTransactions"];
        if (string.IsNullOrWhiteSpace(baseUrlTransactions))
        {
            throw AppValidationException.BadRequest("Casso transactions URL is not configured.", "CasooOptions:BaseUrlTransactions", "CASSO_CONFIG_MISSING");
        }

        var queryParams = new List<string>
        {
            $"page={request.page}",
            $"pageSize={request.pageSize}",
            $"sort={Uri.EscapeDataString(string.IsNullOrWhiteSpace(request.sort) ? "ASC" : request.sort.Trim().ToUpperInvariant())}"
        };
        if (request.fromDate.HasValue)
        {
            queryParams.Add($"fromDate={request.fromDate.Value:yyyy-MM-dd}");
        }
        if (request.toDate.HasValue)
        {
            queryParams.Add($"toDate={request.toDate.Value:yyyy-MM-dd}");
        }

        var requestUri = baseUrlTransactions
                         + (baseUrlTransactions.Contains('?') ? "&" : "?")
                         + string.Join("&", queryParams);

        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(_configuration.GetValue("CasooOptions:TimeoutSeconds", 30))
        };
        var authorizationValue = apiKey.Trim();
        if (!authorizationValue.StartsWith("Apikey ", StringComparison.OrdinalIgnoreCase)
            && !authorizationValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            authorizationValue = $"Apikey {authorizationValue}";
        }
        httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authorizationValue);

        using var cassoResponse = await httpClient.GetAsync(requestUri);
        if (!cassoResponse.IsSuccessStatusCode)
        {
            financialAccount.SyncStatus = "Error";
            financialAccount.LastSyncError = $"Casso request failed with status {(int)cassoResponse.StatusCode}.";
            financialAccount.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();
            throw AppValidationException.BadRequest("Casso transaction sync failed.", "casso", "CASSO_SYNC_FAILED");
        }

        await using var responseStream = await cassoResponse.Content.ReadAsStreamAsync();
        using var json = await JsonDocument.ParseAsync(responseStream);
        if (!json.RootElement.TryGetProperty("error", out var errorElement)
            || errorElement.GetInt32() != 0)
        {
            financialAccount.SyncStatus = "Error";
            financialAccount.LastSyncError = "Casso response error is not zero.";
            financialAccount.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();
            throw AppValidationException.BadRequest("Casso response is invalid.", "casso", "CASSO_RESPONSE_INVALID");
        }

        var records = new List<JsonElement>();
        if (json.RootElement.TryGetProperty("data", out var dataElement)
            && dataElement.ValueKind == JsonValueKind.Object
            && dataElement.TryGetProperty("records", out var recordsElement)
            && recordsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var record in recordsElement.EnumerateArray())
            {
                records.Add(record);
            }
        }
        else if (json.RootElement.TryGetProperty("data", out var dataArrayElement)
                 && dataArrayElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var record in dataArrayElement.EnumerateArray())
            {
                records.Add(record);
            }
        }

        var createdCount = 0;
        var skippedCount = 0;
        await using var databaseTransaction = await _dbContext.Database.BeginTransactionAsync();

        foreach (var record in records)
        {
            decimal amount;
            if (record.TryGetProperty("amount", out var amountElement) && amountElement.ValueKind == JsonValueKind.Number)
            {
                amount = amountElement.GetDecimal();
            }
            else
            {
                skippedCount++;
                continue;
            }

            if (amount == 0)
            {
                skippedCount++;
                continue;
            }

            string? recordAccountRef = null;
            if (record.TryGetProperty("bankSubAccId", out var bankSubAccIdElement))
            {
                recordAccountRef = bankSubAccIdElement.GetString();
            }
            if (string.IsNullOrWhiteSpace(recordAccountRef) && record.TryGetProperty("bank_sub_acc_id", out var bankSubAccIdSnakeElement))
            {
                recordAccountRef = bankSubAccIdSnakeElement.GetString();
            }
            if (string.IsNullOrWhiteSpace(recordAccountRef) && record.TryGetProperty("accountNumber", out var accountNumberElement))
            {
                recordAccountRef = accountNumberElement.GetString();
            }

            if (!string.IsNullOrWhiteSpace(recordAccountRef)
                && !string.IsNullOrWhiteSpace(financialAccount.ExternalAccountRef)
                && recordAccountRef != financialAccount.ExternalAccountRef)
            {
                skippedCount++;
                continue;
            }

            string? externalTransactionId = null;
            if (record.TryGetProperty("reference", out var referenceElement))
            {
                externalTransactionId = referenceElement.GetString();
            }
            if (string.IsNullOrWhiteSpace(externalTransactionId) && record.TryGetProperty("tid", out var tidElement))
            {
                externalTransactionId = tidElement.GetString();
            }
            if (string.IsNullOrWhiteSpace(externalTransactionId) && record.TryGetProperty("id", out var idElement))
            {
                externalTransactionId = idElement.ValueKind == JsonValueKind.String
                    ? idElement.GetString()
                    : idElement.GetRawText();
            }
            if (string.IsNullOrWhiteSpace(externalTransactionId) && record.TryGetProperty("privateId", out var privateIdElement))
            {
                externalTransactionId = privateIdElement.ValueKind == JsonValueKind.String
                    ? privateIdElement.GetString()
                    : privateIdElement.GetRawText();
            }

            if (string.IsNullOrWhiteSpace(externalTransactionId))
            {
                skippedCount++;
                continue;
            }

            var existedTransaction = await _dbContext.Transactions.AnyAsync(x =>
                x.FinancialAccountId == financialAccount.Id
                && x.ExternalTransactionId == externalTransactionId
                && !x.IsDeleted);
            if (existedTransaction)
            {
                skippedCount++;
                continue;
            }

            var transactionDate = DateTimeOffset.UtcNow;
            if (record.TryGetProperty("transactionDateTime", out var transactionDateTimeElement)
                && DateTimeOffset.TryParse(transactionDateTimeElement.GetString(), out var parsedTransactionDateTime))
            {
                transactionDate = parsedTransactionDateTime.ToUniversalTime();
            }
            else if (record.TryGetProperty("when", out var whenElement)
                     && DateTimeOffset.TryParse(whenElement.GetString(), out var parsedWhen))
            {
                transactionDate = parsedWhen.ToUniversalTime();
            }
            else if (record.TryGetProperty("transactionDate", out var transactionDateElement)
                     && DateTimeOffset.TryParse(transactionDateElement.GetString(), out var parsedTransactionDate))
            {
                transactionDate = parsedTransactionDate.ToUniversalTime();
            }

            string? description = null;
            if (record.TryGetProperty("description", out var descriptionElement))
            {
                description = descriptionElement.GetString();
            }

            decimal? runningBalance = null;
            if (record.TryGetProperty("runningBalance", out var runningBalanceElement)
                && runningBalanceElement.ValueKind == JsonValueKind.Number)
            {
                runningBalance = runningBalanceElement.GetDecimal();
            }
            else if (record.TryGetProperty("cusum_balance", out var cusumBalanceElement)
                     && cusumBalanceElement.ValueKind == JsonValueKind.Number)
            {
                runningBalance = cusumBalanceElement.GetDecimal();
            }

            _dbContext.Transactions.Add(new Repository.Entity.Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userIdGuid,
                FinancialAccountId = financialAccount.Id,
                CategoryId = null,
                FromJarId = null,
                ToJarId = null,
                Type = amount > 0 ? "Income" : "Expense",
                TransactionsAmount = Math.Abs(amount),
                Note = description,
                RawDescription = description,
                TransactionDate = transactionDate,
                SourceType = "Imported",
                ExternalTransactionId = externalTransactionId,
                RawPayloadJson = record.GetRawText(),
                PostedAt = DateTimeOffset.UtcNow,
                ImportJobId = null,
                IsDeleted = false,
                DeletedAt = null,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

            if (runningBalance.HasValue)
            {
                financialAccount.CurrentBalance = runningBalance.Value;
            }
            else if (amount > 0)
            {
                financialAccount.CurrentBalance += Math.Abs(amount);
            }
            else
            {
                financialAccount.CurrentBalance -= Math.Abs(amount);
            }

            financialAccount.SyncStatus = "Synced";
            financialAccount.LastSyncedAt = DateTimeOffset.UtcNow;
            financialAccount.LastSyncError = null;
            financialAccount.LastSyncCursor = externalTransactionId;
            financialAccount.UpdatedAt = DateTimeOffset.UtcNow;
            createdCount++;
        }

        await _dbContext.SaveChangesAsync();
        await databaseTransaction.CommitAsync();

        return new Response.CassoTransactionsResponse
        {
            receivedCount = records.Count,
            createdCount = createdCount,
            skippedCount = skippedCount,
            message = "Casso transactions synced."
        };
    }

    private Guid GetCurrentUserId()
    {
        return ServiceClaimHelper.GetRequiredUserId(_httpContext);
    }
}
