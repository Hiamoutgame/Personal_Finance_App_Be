using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Entity;
using Personal_Finance_Management.Service.Base;
using Personal_Finance_Management.Service.Common.Constants;
using TxEnums = Personal_Finance_Management.Service.Common.Enums;
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
            throw AppValidationException.BadRequest("Trang phải lớn hơn 0.", "pageIndex", ErrorCodes.InvalidPageIndex);
        }

        if (request.pageSize <= 0 || request.pageSize > 100)
        {
            throw AppValidationException.BadRequest("Số dòng mỗi trang phải từ 1 đến 100.", "pageSize", ErrorCodes.InvalidPageSize);
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
            throw AppValidationException.NotFound(ErrorMessages.TransactionNotFound, "id", ErrorCodes.TransactionNotFound);
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
        if (transactionType != TxEnums.TransactionType.Income && transactionType != TxEnums.TransactionType.Expense && transactionType != "Transfer")
        {
            throw AppValidationException.BadRequest(ErrorMessages.InvalidTransactionType, "type", ErrorCodes.InvalidTransactionType);
        }

        if (request.transactionsAmount <= 0)
        {
            throw AppValidationException.BadRequest(ErrorMessages.InvalidTransactionAmount, "transactionsAmount", ErrorCodes.InvalidTransactionAmount);
        }

        var transactionDate = transactionType == "Transfer" ? now : request.date;
        if (transactionType != "Transfer" && transactionDate > now)
        {
            throw AppValidationException.BadRequest(ErrorMessages.TransactionDateInFuture, "date", ErrorCodes.TransactionDateInFuture);
        }

        if (request.financialAccountId.HasValue)
        {
            var financialAccount = await _dbContext.FinancialAccounts
                .FirstOrDefaultAsync(x => x.Id == request.financialAccountId.Value && x.UserId == userIdGuid && x.IsActive);
            if (financialAccount == null)
            {
                throw AppValidationException.NotFound("Không tìm thấy tài khoản tài chính.", "financialAccountId", ErrorCodes.FinancialAccountNotFound);
            }

            if (financialAccount.ConnectionMode == TxEnums.ConnectionMode.LinkedApi)
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
                throw AppValidationException.NotFound(ErrorMessages.CategoryNotFound, "categoryId", ErrorCodes.CategoryNotFound);
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
            SourceType = SourceTypes.Manual,
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
        if(transaction.Type == TxEnums.TransactionType.Expense)
        {
            // Pay for something by selected Jar
            if (transaction.FromJarId != null  && transaction.ToJarId == null && transaction.FinancialAccountId == null)
            {
                var jar = await _dbContext.Jars.FirstOrDefaultAsync(x => x.Id == transaction.FromJarId && x.UserId == userIdGuid);
                if (jar == null)
                {
                    throw AppValidationException.NotFound(ErrorMessages.JarNotFound, "fromJarId", ErrorCodes.JarNotFound);
                }

                if (jar.Balance - transaction.TransactionsAmount >= 0)
                {
                    jar.Balance = jar.Balance - transaction.TransactionsAmount;
                }
                else
                {
                    throw AppValidationException.BadRequest(
                        ErrorMessages.InsufficientJarBalance,
                        "fromJarId",
                        ErrorCodes.InsufficientJarBalance);
                }
            }
        }else if (transaction.Type == TxEnums.TransactionType.Income)
        {
            if (transaction.ToJarId == null && transaction.FromJarId == null 
                                            && transaction.FinancialAccountId != null)
            {
                var financialAccount = await _dbContext.FinancialAccounts.FirstOrDefaultAsync(x => x.Id == transaction.FinancialAccountId && x.UserId == userIdGuid && x.IsActive);
                if (financialAccount == null)
                {
                    throw AppValidationException.NotFound("Không tìm thấy tài khoản tài chính.", "financialAccountId", ErrorCodes.FinancialAccountNotFound);
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
                    throw AppValidationException.NotFound("Không tìm thấy hũ nguồn.", "fromJarId", ErrorCodes.JarNotFound);
                }
                if (toJar == null)
                {
                    throw AppValidationException.NotFound("Không tìm thấy hũ đích.", "toJarId", ErrorCodes.JarNotFound);
                }
                if (fromJar.Id == toJar.Id)
                {
                    throw AppValidationException.BadRequest(ErrorMessages.TransferSourceTargetSame, "toJarId", ErrorCodes.InvalidTransferTarget);
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
                        ErrorCodes.InsufficientJarBalance);
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
                    throw AppValidationException.NotFound("Không tìm thấy hũ đích.", "toJarId", ErrorCodes.JarNotFound);
                }
                if (finnacialAccount == null)
                {
                    throw AppValidationException.NotFound("Không tìm thấy tài khoản tài chính.", "financialAccountId", ErrorCodes.FinancialAccountNotFound);
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
                    throw AppValidationException.NotFound("Không tìm thấy hũ nguồn.", "fromJarId", ErrorCodes.JarNotFound);
                }
                if (finnacialAccount == null)
                {
                    throw AppValidationException.NotFound("Không tìm thấy tài khoản tài chính.", "financialAccountId", ErrorCodes.FinancialAccountNotFound);
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
                        ErrorCodes.InsufficientJarBalance);
                }
                
            }
            else
            {
                throw AppValidationException.BadRequest(ErrorMessages.InvalidTransferInfo, "type", ErrorCodes.InvalidTransferTarget);
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
            throw AppValidationException.NotFound("Transaction not found.", "id", ErrorCodes.TransactionNotFound);
        }

        if (transaction.SourceType != SourceTypes.Manual)
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
            throw AppValidationException.BadRequest(ErrorMessages.InvalidTransactionAmount, "transactionsAmount", ErrorCodes.InvalidTransactionAmount);
        }

        if (newCategoryId.HasValue)
        {
            var categoryExists = await _dbContext.Categories.AnyAsync(x =>
                x.Id == newCategoryId.Value
                && x.IsActive
                && (x.OwnerUserId == null || x.OwnerUserId == userIdGuid));
            if (!categoryExists)
            {
                throw AppValidationException.NotFound(ErrorMessages.CategoryNotFound, "categoryId", ErrorCodes.CategoryNotFound);
            }
        }

        var oldCategoryId = transaction.CategoryId;
        await using var databaseTransaction = await _dbContext.Database.BeginTransactionAsync();

        if (newTransactionsAmount.HasValue)
        {
            var updatedAmount = newTransactionsAmount.Value;
            if (transaction.Type == TxEnums.TransactionType.Expense)
            {
                if (transaction.FromJarId != null && transaction.ToJarId == null && transaction.FinancialAccountId == null)
                {
                    var fromJar = await _dbContext.Jars.FirstOrDefaultAsync(x => x.Id == transaction.FromJarId && x.UserId == userIdGuid);
                    if(fromJar == null)
                    {
                        throw AppValidationException.NotFound("Jar not found.", "fromJarId", ErrorCodes.JarNotFound);
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
                            ErrorCodes.InsufficientJarBalance);
                    }
                }
                
            }

            else if (transaction.Type == TxEnums.TransactionType.Income)
            {
                if (transaction.FromJarId == null && transaction.ToJarId == null && transaction.FinancialAccountId != null)
                {
                    var financialAccount = await _dbContext.FinancialAccounts.FirstOrDefaultAsync(x => x.Id == transaction.FinancialAccountId && x.UserId == userIdGuid && x.IsActive);
                    if (financialAccount == null)
                    {
                        throw AppValidationException.NotFound("Financial account not found.", "financialAccountId", ErrorCodes.FinancialAccountNotFound);
                    }
                    var isUse = await _dbContext.Transactions.AnyAsync(x =>
                        x.FinancialAccountId == financialAccount.Id
                        && x.UserId == userIdGuid
                        && x.Id != transaction.Id
                        && x.Type != TxEnums.TransactionType.Income
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
        if (transaction.Type == TxEnums.TransactionType.Expense && transaction.FromJarId != null)
        {
            await CheckJarLimit(transaction.FromJarId.Value);
        }
        if (transaction.Type == TxEnums.TransactionType.Expense && transaction.CategoryId != null)
        {
            await CheckCategoryLimit(transaction.CategoryId.Value, userIdGuid);
        }
        if (transaction.Type == TxEnums.TransactionType.Expense
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
            throw AppValidationException.NotFound("Transaction not found.", "id", ErrorCodes.TransactionNotFound);
        }

        if (transaction.SourceType != SourceTypes.Manual)
        {
            throw AppValidationException.BadRequest(
                "Linked bank transaction cannot be deleted manually.",
                "id",
                "LINKED_TRANSACTION_DELETE_NOT_ALLOWED");
        }

        await using var databaseTransaction = await _dbContext.Database.BeginTransactionAsync();

        if (!transaction.IsDeleted)
        {
            if (transaction.Type == TxEnums.TransactionType.Expense)
            {
                if (transaction.FromJarId != null && transaction.ToJarId == null && transaction.FinancialAccountId == null)
                {
                    var fromJar = await _dbContext.Jars.FirstOrDefaultAsync(x => x.Id == transaction.FromJarId && x.UserId == userIdGuid);
                    if (fromJar == null)
                    {
                        throw AppValidationException.NotFound("Jar not found.", "fromJarId", ErrorCodes.JarNotFound);
                    }

                    fromJar.Balance += transaction.TransactionsAmount;
                    fromJar.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }
            else if (transaction.Type == TxEnums.TransactionType.Income)
            {
                if (transaction.FromJarId == null && transaction.ToJarId == null && transaction.FinancialAccountId != null)
                {
                    var financialAccount = await _dbContext.FinancialAccounts.FirstOrDefaultAsync(x => x.Id == transaction.FinancialAccountId && x.UserId == userIdGuid && x.IsActive);
                    if (financialAccount == null)
                    {
                        throw AppValidationException.NotFound("Financial account not found.", "financialAccountId", ErrorCodes.FinancialAccountNotFound);
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
                        throw AppValidationException.NotFound("Jar not found.", "fromJarId", ErrorCodes.JarNotFound);
                    }
                    if (toJar == null)
                    {
                        throw AppValidationException.NotFound("Jar not found.", "toJarId", ErrorCodes.JarNotFound);
                    }

                    if (toJar.Balance - transaction.TransactionsAmount < 0)
                    {
                        throw AppValidationException.BadRequest(
                            "Số tiền trong hũ đích không đủ để hoàn tác giao dịch.",
                            "toJarId",
                            ErrorCodes.InsufficientJarBalance);
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
                        throw AppValidationException.NotFound("Jar not found.", "toJarId", ErrorCodes.JarNotFound);
                    }
                    if (financialAccount == null)
                    {
                        throw AppValidationException.NotFound("Financial account not found.", "financialAccountId", ErrorCodes.FinancialAccountNotFound);
                    }
                    if (toJar.Balance - transaction.TransactionsAmount < 0)
                    {
                        throw AppValidationException.BadRequest(
                            "Số tiền trong hũ đích không đủ để hoàn tác giao dịch.",
                            "toJarId",
                            ErrorCodes.InsufficientJarBalance);
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
                        throw AppValidationException.NotFound("Jar not found.", "fromJarId", ErrorCodes.JarNotFound);
                    }
                    if (financialAccount == null)
                    {
                        throw AppValidationException.NotFound("Financial account not found.", "financialAccountId", ErrorCodes.FinancialAccountNotFound);
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
        if (transaction.Type == TxEnums.TransactionType.Expense && transaction.FromJarId != null)
        {
            await CheckJarLimit(transaction.FromJarId.Value);
        }
        if (transaction.Type == TxEnums.TransactionType.Expense && transaction.CategoryId != null)
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
            .Where(g => g.LinkedJarId == jarId && g.Status == TxEnums.GoalStatus.Active)
            .ToListAsync();

        foreach (var goal in activeGoals)
        {
            // Kiểm tra điều kiện hoàn thành: Số dư hũ >= Target Amount
            // Theo Option B: Chấp nhận hoàn thành cả khi đã quá hạn (DueDate)
            if (jar.Balance >= goal.TargetAmount)
            {
                goal.Status = TxEnums.GoalStatus.Completed;
                goal.UpdatedAt = DateTimeOffset.UtcNow;

                // Tạo thông báo cho người dùng
                var notification = new Notification
                {
                    UserId = goal.UserId,
                    Type = TxEnums.NotificationType.GoalUpdate,
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
                    Type = TxEnums.NotificationType.SpendingAlert,
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
                    Type = TxEnums.NotificationType.SpendingAlert,
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
                        && t.Type == TxEnums.TransactionType.Expense
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
                    Type = TxEnums.NotificationType.SpendingAlert,
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
                    Type = TxEnums.NotificationType.SpendingAlert,
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
                        && t.Type == TxEnums.TransactionType.Expense
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
            Type = TxEnums.NotificationType.SpendingAlert,
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
                          && entry.Entity.Type == TxEnums.NotificationType.SpendingAlert
                          && entry.Entity.Body == body
                          && entry.Entity.MetadataJson != null
                          && entry.Entity.MetadataJson.Contains(limitIdMarker)
                          && entry.Entity.MetadataJson.Contains(targetIdMarker));

        if (pendingExists)
            return true;

        var metadataList = await _dbContext.Notifications
            .Where(n => n.UserId == userId
                        && n.Type == TxEnums.NotificationType.SpendingAlert
                        && n.Body == body)
            .Select(n => n.MetadataJson)
            .ToListAsync();

        return metadataList.Any(metadata =>
            metadata != null
            && metadata.Contains(limitIdMarker)
            && metadata.Contains(targetIdMarker));
    }

    private Guid GetCurrentUserId()
    {
        return ServiceClaimHelper.GetRequiredUserId(_httpContext);
    }
}
