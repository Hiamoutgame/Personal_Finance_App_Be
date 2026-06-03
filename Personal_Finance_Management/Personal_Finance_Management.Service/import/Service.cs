using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Entity;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Service.Base;
using Personal_Finance_Management.Service.Common.Constants;
using TxEnums = Personal_Finance_Management.Service.Common.Enums;
using Personal_Finance_Management.Service.Validations;
using ValidationServices = Personal_Finance_Management.Service.Validations;
using OcrService = Personal_Finance_Management.Service.ocr;

namespace Personal_Finance_Management.Service.import
{
    public class Service : IServices
    {
        private readonly ValidationServices.IServices _validationService;
        private readonly OcrService.IService _ocrService;
        private readonly OcrService.IReceiptParserService _receiptParserService;
        private readonly AppDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContext;

        public Service(
            AppDbContext dbContext,
            ValidationServices.IServices validationService,
            OcrService.IService ocrService,
            OcrService.IReceiptParserService receiptParserService,
            IHttpContextAccessor httpContext)
        {
            _dbContext = dbContext;
            _validationService = validationService;
            _ocrService = ocrService;
            _receiptParserService = receiptParserService;
            _httpContext = httpContext;
        }

        public async Task<Response.ImportImageResponse> ImportImage(Request.ImportData request)
        {
            await _validationService.ValidateImportImageRequest(request);
            var userId = GetCurrentUserId();
            var financialAccount = await GetImportFinancialAccount(userId, request.FinancialAccountId);

            var uploadFolder = GetUploadFolderPath();
            Directory.CreateDirectory(uploadFolder);

            var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            var fileName = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
            var savePath = Path.Combine(uploadFolder, fileName);

            await using (var fileStream = new FileStream(savePath, FileMode.CreateNew, FileAccess.Write))
            {
                await request.File.CopyToAsync(fileStream);
            }

            OcrService.OCRResult? ocrResult = null;
            string? ocrJsonFileName = null;
            string? storedOcrJsonPath = null;
            Response.OcrPreviewResponse? preview = null;

            if (request.RunOcr)
            {
                ocrResult = await _ocrService.ReadImageAsync(savePath, request.Layout);
                if (ocrResult.IsSuccess)
                {
                    ocrResult.Receipt = await _receiptParserService.ExtractReceiptAsync(ocrResult);
                }

                if (!string.IsNullOrWhiteSpace(ocrResult.RawJson))
                {
                    var ocrJsonFolder = GetOcrJsonFolderPath();
                    Directory.CreateDirectory(ocrJsonFolder);

                    ocrJsonFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_ocr.json";
                    storedOcrJsonPath = Path.Combine(ocrJsonFolder, ocrJsonFileName);
                    await File.WriteAllTextAsync(storedOcrJsonPath, ocrResult.RawJson);
                }
            }

            preview = BuildPreview(fileName, ocrResult);

            var now = DateTimeOffset.UtcNow;
            var importJob = new ImportJob
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                OriginalContentType = request.File.ContentType,
                StoredFilePath = savePath,
                BankCode = ServiceTextHelper.NormalizeOptionalText(request.BankCode),
                Status = request.RunOcr
                    ? ocrResult is { IsSuccess: true } ? "AwaitingReview" : "Failed"
                    : "Pending",
                Progress = request.RunOcr
                    ? ocrResult is { IsSuccess: true } ? 100 : 0
                    : 0,
                EstimatedRows = preview.Transaction.Amount.HasValue ? 1 : 0,
                ParsedCount = preview.Transaction.Amount.HasValue ? 1 : 0,
                FailedCount = request.RunOcr && ocrResult is not { IsSuccess: true } ? 1 : 0,
                ErrorMessage = ocrResult?.ErrorMessage,
                UserId = userId,
                FinancialAccountId = financialAccount.Id,
                UploadedAt = now,
                UpdatedAt = now
            };

            if (preview.Transaction.Amount.HasValue || !string.IsNullOrWhiteSpace(preview.Transaction.MerchantName))
            {
                importJob.Drafts.Add(new ImportTransactionDraft
                {
                    Id = Guid.NewGuid(),
                    RowIndex = 0,
                    TransactionDate = preview.Transaction.Date,
                    Amount = preview.Transaction.Amount,
                    Type = TxEnums.TransactionType.Expense,
                    RawDescription = preview.Transaction.MerchantName,
                    EditedNote = preview.Transaction.Note,
                    EditedCategoryId = preview.Transaction.SuggestedCategoryId,
                    EditedJarId = null,
                    IsValid = preview.Transaction.Amount.HasValue,
                    ValidationError = preview.Transaction.Amount.HasValue ? null : "Amount could not be extracted.",
                    NormalizedPayloadJson = JsonSerializer.Serialize(preview),
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            _dbContext.ImportJobs.Add(importJob);
            await _dbContext.SaveChangesAsync();

            return new Response.ImportImageResponse
            {
                Id = importJob.Id,
                FinancialAccountId = importJob.FinancialAccountId,
                Status = importJob.Status,
                Message = ocrResult is { IsSuccess: true }
                    ? "OCR completed successfully"
                    : "Imported file successfully",
                FileName = fileName,
                OriginalFileName = Path.GetFileName(request.File.FileName),
                StoredFilePath = savePath,
                ContentType = request.File.ContentType,
                SizeInBytes = request.File.Length,
                OcrJsonFileName = ocrJsonFileName,
                StoredOcrJsonPath = storedOcrJsonPath,
                RawOcrJson = request.IncludeDebug ? ocrResult?.RawJson : null,
                OcrResult = request.IncludeDebug ? ocrResult : null,
                Receipt = ocrResult?.Receipt,
                Preview = preview
            };
        }

        public async Task<Response.ImportJobListResponse> GetImports(Request.GetImportsRequest request)
        {
            var userId = GetCurrentUserId();
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

            var query = _dbContext.ImportJobs
                .AsNoTracking()
                .Where(job => job.UserId == userId);

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(job => job.Status == request.Status.Trim());
            }

            if (request.FinancialAccountId.HasValue)
            {
                query = query.Where(job => job.FinancialAccountId == request.FinancialAccountId.Value);
            }

            var totalCount = await query.CountAsync();
            var importJobs = await query
                .OrderByDescending(job => job.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var jobs = importJobs.Select(MapJobSummary).ToList();

            return new Response.ImportJobListResponse
            {
                Data = jobs,
                Pagination = new Response.PaginationResponse
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            };
        }

        public async Task<Response.ImportJobDetailResponse> GetImport(Guid id)
        {
            var job = await GetOwnedImportJob(id);
            return MapJobDetail(job);
        }

        public async Task<Response.ImportDraftResponse> UpdateImport(Guid id, Request.UpdateImportDraftRequest request)
        {
            var userId = GetCurrentUserId();
            var draft = await _dbContext.ImportTransactionDrafts
                .Include(item => item.ImportJob)
                .Include(item => item.EditedCategory)
                .Include(item => item.EditedJar)
                .Where(item => item.ImportJobId == id
                               && item.ImportJob != null
                               && item.ImportJob.UserId == userId)
                .OrderBy(item => item.RowIndex)
                .FirstOrDefaultAsync();

            if (draft is null)
            {
                throw AppValidationException.NotFound(ErrorMessages.ImportDraftNotFound, "id", ErrorCodes.ImportDraftNotFound);
            }

            return await UpdateDraft(draft, request);
        }

        public async Task<Response.ImportDraftResponse> UpdateImportDraft(
            Guid id,
            Guid draftId,
            Request.UpdateImportDraftRequest request)
        {
            var userId = GetCurrentUserId();
            var draft = await _dbContext.ImportTransactionDrafts
                .Include(item => item.ImportJob)
                .Include(item => item.EditedCategory)
                .Include(item => item.EditedJar)
                .FirstOrDefaultAsync(item => item.Id == draftId
                                             && item.ImportJobId == id
                                             && item.ImportJob != null
                                             && item.ImportJob.UserId == userId);

            if (draft is null)
            {
                throw AppValidationException.NotFound(ErrorMessages.ImportDraftNotFound, "draftId", ErrorCodes.ImportDraftNotFound);
            }

            return await UpdateDraft(draft, request);
        }

        public async Task<Response.ConfirmImportResponse> ConfirmImport(
            Guid id,
            Request.ConfirmImportRequest request)
        {
            request ??= new Request.ConfirmImportRequest();
            var userId = GetCurrentUserId();
            if (request.FinancialAccountId.HasValue && request.FromJarId.HasValue)
            {
                throw AppValidationException.BadRequest(
                    "Choose either a financial account or a jar, not both.",
                    "body",
                    "IMPORT_CONFIRM_SOURCE_CONFLICT");
            }

            await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();

            var job = await _dbContext.ImportJobs
                .Include(item => item.Drafts)
                .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

            if (job is null)
            {
                throw AppValidationException.NotFound(ErrorMessages.ImportNotFound, "id", ErrorCodes.ImportNotFound);
            }

            if (job.Status == "Completed")
            {
                throw AppValidationException.BadRequest(
                    "Import has already been confirmed.",
                    "id",
                    "IMPORT_ALREADY_CONFIRMED");
            }

            var hasCreatedTransactions = await _dbContext.Transactions
                .AnyAsync(item => item.ImportJobId == id && !item.IsDeleted);
            if (hasCreatedTransactions)
            {
                throw AppValidationException.BadRequest(
                    "Import already has created transactions.",
                    "id",
                    "IMPORT_ALREADY_CONFIRMED");
            }

            var selectedDraftIds = request.DraftIds?
                .Where(item => item != Guid.Empty)
                .ToHashSet();

            var drafts = job.Drafts
                .Where(draft => selectedDraftIds is null || selectedDraftIds.Contains(draft.Id))
                .OrderBy(draft => draft.RowIndex)
                .ToList();

            if (drafts.Count == 0)
            {
                throw AppValidationException.BadRequest(
                    "Import has no draft to confirm.",
                    "draftIds",
                    "IMPORT_DRAFT_REQUIRED");
            }

            var now = DateTimeOffset.UtcNow;
            var useJar = request.FromJarId.HasValue || drafts.Any(draft => draft.EditedJarId.HasValue);
            Personal_Finance_Management.Repository.Entity.Jar? sourceJar = null;
            Personal_Finance_Management.Repository.Entity.FinancialAccount? financialAccount = null;

            if (request.FromJarId.HasValue)
            {
                sourceJar = await _dbContext.Jars
                    .FirstOrDefaultAsync(item =>
                        item.Id == request.FromJarId.Value
                        && item.UserId == userId
                        && item.Status == "Active");
                if (sourceJar is null)
                {
                    throw AppValidationException.NotFound("Jar not found.", "fromJarId", ErrorCodes.JarNotFound);
                }
            }

            if (!useJar)
            {
                financialAccount = await GetImportFinancialAccount(
                    userId,
                    request.FinancialAccountId ?? job.FinancialAccountId);
                job.FinancialAccountId = financialAccount.Id;
            }

            var responseTransactions = new List<Response.ConfirmedTransactionResponse>();
            foreach (var draft in drafts)
            {
                ValidateConfirmDraft(draft);

                if (draft.EditedCategoryId.HasValue)
                {
                    await EnsureCategoryCanBeUsed(userId, draft.EditedCategoryId.Value);
                }

                var transactionType = draft.Type ?? TxEnums.TransactionType.Expense;
                var amount = draft.Amount!.Value;
                var transactionDate = draft.TransactionDate!.Value;
                Guid? transactionFinancialAccountId = null;
                Guid? transactionFromJarId = null;

                if (transactionType == TxEnums.TransactionType.Expense)
                {
                    var draftJarId = request.FromJarId ?? draft.EditedJarId;
                    if (draftJarId.HasValue)
                    {
                        var jar = sourceJar is not null && sourceJar.Id == draftJarId.Value
                            ? sourceJar
                            : await _dbContext.Jars.FirstOrDefaultAsync(item =>
                                item.Id == draftJarId.Value
                                && item.UserId == userId
                                && item.Status == "Active");

                        if (jar is null)
                        {
                            throw AppValidationException.NotFound("Jar not found.", "fromJarId", ErrorCodes.JarNotFound);
                        }

                        if (jar.Balance < amount)
                        {
                            throw AppValidationException.BadRequest(
                                "Insufficient jar balance.",
                                "fromJarId",
                                ErrorCodes.InsufficientJarBalance);
                        }

                        jar.Balance -= amount;
                        jar.UpdatedAt = now;
                        transactionFromJarId = jar.Id;
                    }
                    else
                    {
                        financialAccount ??= await GetImportFinancialAccount(
                            userId,
                            request.FinancialAccountId ?? job.FinancialAccountId);
                        if (financialAccount.CurrentBalance < amount)
                        {
                            throw AppValidationException.BadRequest(
                                "Insufficient financial account balance.",
                                "financialAccountId",
                                "INSUFFICIENT_ACCOUNT_BALANCE");
                        }

                        financialAccount.CurrentBalance -= amount;
                        financialAccount.UpdatedAt = now;
                        transactionFinancialAccountId = financialAccount.Id;
                        job.FinancialAccountId = financialAccount.Id;
                    }
                }
                else
                {
                    financialAccount ??= await GetImportFinancialAccount(
                        userId,
                        request.FinancialAccountId ?? job.FinancialAccountId);
                    financialAccount.CurrentBalance += amount;
                    financialAccount.UpdatedAt = now;
                    transactionFinancialAccountId = financialAccount.Id;
                    job.FinancialAccountId = financialAccount.Id;
                }

                var transaction = new Repository.Entity.Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    FinancialAccountId = transactionFinancialAccountId,
                    CategoryId = draft.EditedCategoryId,
                    FromJarId = transactionFromJarId,
                    ToJarId = null,
                    Type = transactionType,
                    TransactionsAmount = amount,
                    Note = draft.EditedNote,
                    RawDescription = draft.RawDescription,
                    TransactionDate = transactionDate,
                    SourceType = "OCR",
                    ExternalTransactionId = null,
                    RawPayloadJson = draft.NormalizedPayloadJson,
                    JarBalanceAfterAllocation = null,
                    PostedAt = now,
                    ImportJobId = job.Id,
                    IsDeleted = false,
                    DeletedAt = null,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _dbContext.Transactions.Add(transaction);
                responseTransactions.Add(new Response.ConfirmedTransactionResponse
                {
                    Id = transaction.Id,
                    DraftId = draft.Id,
                    FinancialAccountId = transaction.FinancialAccountId,
                    FromJarId = transaction.FromJarId,
                    CategoryId = transaction.CategoryId,
                    Type = transaction.Type,
                    TransactionsAmount = transaction.TransactionsAmount,
                    TransactionDate = transaction.TransactionDate
                });
            }

            job.Status = "Completed";
            job.Progress = 100;
            job.ParsedCount = responseTransactions.Count;
            job.FailedCount = 0;
            job.ErrorMessage = null;
            job.UpdatedAt = now;

            await _dbContext.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return new Response.ConfirmImportResponse
            {
                ImportJobId = job.Id,
                Status = job.Status,
                CreatedCount = responseTransactions.Count,
                Transactions = responseTransactions
            };
        }

        public async Task<Response.MessageResponse> DeleteImport(Guid id)
        {
            var job = await GetOwnedImportJob(id);
            _dbContext.ImportTransactionDrafts.RemoveRange(job.Drafts);
            _dbContext.ImportJobs.Remove(job);
            await _dbContext.SaveChangesAsync();

            DeleteFileIfExists(job.StoredFilePath);
            var ocrJsonPath = Path.Combine(
                GetOcrJsonFolderPath(),
                $"{Path.GetFileNameWithoutExtension(job.FileName)}_ocr.json");
            DeleteFileIfExists(ocrJsonPath);

            return new Response.MessageResponse
            {
                Message = "Import deleted"
            };
        }

        public async Task<Response.UploadedFileResponse> GetUploadedImage(string fileName)
        {
            var safeFileName = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(safeFileName) || safeFileName != fileName)
            {
                throw AppValidationException.BadRequest(
                    "Invalid file name.",
                    "fileName",
                    "INVALID_FILE_NAME");
            }

            var userId = GetCurrentUserId();
            var job = await _dbContext.ImportJobs
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.UserId == userId && item.FileName == safeFileName);

            if (job is null)
            {
                throw AppValidationException.NotFound(
                    "Uploaded image was not found.",
                    "fileName",
                    "UPLOAD_NOT_FOUND");
            }

            var storedFilePath = string.IsNullOrWhiteSpace(job.StoredFilePath)
                ? Path.Combine(GetUploadFolderPath(), safeFileName)
                : job.StoredFilePath;
            if (!File.Exists(storedFilePath))
            {
                throw AppValidationException.NotFound(
                    "Uploaded image was not found.",
                    "fileName",
                    "UPLOAD_NOT_FOUND");
            }

            return new Response.UploadedFileResponse
            {
                FileName = safeFileName,
                StoredFilePath = storedFilePath,
                ContentType = GetContentType(storedFilePath)
            };
        }

        private async Task<Response.ImportDraftResponse> UpdateDraft(
            ImportTransactionDraft draft,
            Request.UpdateImportDraftRequest request)
        {
            if (request.Type is not null
                && request.Type != TxEnums.TransactionType.Income
                && request.Type != TxEnums.TransactionType.Expense)
            {
                throw AppValidationException.BadRequest(ErrorMessages.InvalidDraftTransactionType, "type", ErrorCodes.InvalidTransactionType);
            }

            if (request.Amount.HasValue && request.Amount.Value <= 0)
            {
                throw AppValidationException.BadRequest(ErrorMessages.AmountMustBeGreaterThanZero, "amount", ErrorCodes.InvalidAmount);
            }

            if (request.EditedCategoryId.HasValue)
            {
                var categoryExists = await _dbContext.Categories.AnyAsync(category =>
                    category.Id == request.EditedCategoryId.Value
                    && category.IsActive
                    && category.DeletedAt == null);
                if (!categoryExists)
                {
                    throw AppValidationException.NotFound("Category not found.", "editedCategoryId", ErrorCodes.CategoryNotFound);
                }
            }

            if (request.EditedJarId.HasValue)
            {
                var userId = GetCurrentUserId();
                var jarExists = await _dbContext.Jars.AnyAsync(jar =>
                    jar.Id == request.EditedJarId.Value
                    && jar.UserId == userId);
                if (!jarExists)
                {
                    throw AppValidationException.NotFound("Jar not found.", "editedJarId", ErrorCodes.JarNotFound);
                }
            }

            if (request.TransactionDate.HasValue)
            {
                draft.TransactionDate = request.TransactionDate.Value;
            }

            if (request.Amount.HasValue)
            {
                draft.Amount = request.Amount.Value;
            }

            if (request.Type is not null)
            {
                draft.Type = request.Type;
            }

            if (request.EditedNote is not null)
            {
                draft.EditedNote = ServiceTextHelper.NormalizeOptionalText(request.EditedNote);
            }

            if (request.EditedCategoryId.HasValue)
            {
                draft.EditedCategoryId = request.EditedCategoryId.Value;
            }

            if (request.EditedJarId.HasValue)
            {
                draft.EditedJarId = request.EditedJarId.Value;
            }

            if (request.IsValid.HasValue)
            {
                draft.IsValid = request.IsValid.Value;
            }

            if (request.ValidationError is not null)
            {
                draft.ValidationError = ServiceTextHelper.NormalizeOptionalText(request.ValidationError);
            }

            draft.UpdatedAt = DateTimeOffset.UtcNow;
            if (draft.ImportJob is not null)
            {
                draft.ImportJob.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            await _dbContext.Entry(draft).Reference(item => item.EditedCategory).LoadAsync();
            await _dbContext.Entry(draft).Reference(item => item.EditedJar).LoadAsync();

            return MapDraft(draft);
        }

        private static void ValidateConfirmDraft(ImportTransactionDraft draft)
        {
            if (!draft.IsValid)
            {
                throw AppValidationException.BadRequest(
                    "Import draft is not valid.",
                    "draftIds",
                    "INVALID_IMPORT_DRAFT");
            }

            if (draft.Amount is null or <= 0)
            {
                throw AppValidationException.BadRequest(
                    "Draft amount is required.",
                    "amount",
                    "DRAFT_AMOUNT_REQUIRED");
            }

            if (draft.TransactionDate is null)
            {
                throw AppValidationException.BadRequest(
                    "Draft transaction date is required.",
                    "transactionDate",
                    "DRAFT_TRANSACTION_DATE_REQUIRED");
            }

            if (draft.Type is not TxEnums.TransactionType.Income and not TxEnums.TransactionType.Expense)
            {
                throw AppValidationException.BadRequest(
                    "Draft transaction type is invalid.",
                    "type",
                    "INVALID_TRANSACTION_TYPE");
            }
        }

        private async Task EnsureCategoryCanBeUsed(Guid userId, Guid categoryId)
        {
            var exists = await _dbContext.Categories.AnyAsync(category =>
                category.Id == categoryId
                && category.IsActive
                && category.DeletedAt == null
                && (category.IsDefault || category.OwnerUserId == userId));

            if (!exists)
            {
                throw AppValidationException.NotFound(
                    "Category not found.",
                    "editedCategoryId",
                    ErrorCodes.CategoryNotFound);
            }
        }

        private static Response.OcrPreviewResponse BuildPreview(
            string fileName,
            OcrService.OCRResult? ocrResult)
        {
            var receipt = ocrResult?.Receipt;
            var items = ExtractItems(ocrResult?.RawJson);
            var discount = items
                .Where(item => IsDiscountItem(item.Name))
                .Where(item => item.Amount.HasValue)
                .Sum(item => item.Amount!.Value);
            var subtotal = items
                .Where(item => !IsDiscountItem(item.Name))
                .Where(item => item.Amount.HasValue)
                .Sum(item => item.Amount!.Value);

            return new Response.OcrPreviewResponse
            {
                Id = Path.GetFileNameWithoutExtension(fileName),
                Status = ocrResult is { IsSuccess: true } ? "success" : "uploaded",
                ImageUrl = $"/api/v1/imports/images/{Uri.EscapeDataString(fileName)}",
                Transaction = new Response.OcrTransactionPreview
                {
                    MerchantName = receipt?.MerchantName,
                    Amount = receipt?.TotalAmount,
                    Date = receipt?.TransactionDate,
                    Type = TxEnums.TransactionType.Expense,
                    SuggestedCategoryId = receipt?.SuggestedCategoryId,
                    SuggestedCategoryName = receipt?.SuggestedCategoryName,
                    MatchedBy = receipt?.CategoryMatchedBy,
                    Note = string.IsNullOrWhiteSpace(receipt?.MerchantName)
                        ? null
                        : $"Hoa don {receipt.MerchantName}"
                },
                Items = items
                    .Where(item => !IsDiscountItem(item.Name))
                    .ToList(),
                Summary = new Response.OcrSummaryPreview
                {
                    Subtotal = subtotal > 0 ? subtotal : null,
                    Discount = discount > 0 ? discount : null,
                    Total = receipt?.TotalAmount
                },
                Warnings = receipt?.Warnings ?? []
            };
        }

        private Guid GetCurrentUserId()
        {
            return ServiceClaimHelper.GetRequiredUserId(_httpContext);
        }

        private async Task<Repository.Entity.FinancialAccount> GetImportFinancialAccount(Guid userId, Guid? financialAccountId)
        {
            var query = _dbContext.FinancialAccounts
                .Where(account => account.UserId == userId && account.IsActive);

            var financialAccount = financialAccountId.HasValue
                ? await query.FirstOrDefaultAsync(account => account.Id == financialAccountId.Value)
                : await query
                    .OrderByDescending(account => account.IsDefault)
                    .ThenBy(account => account.CreatedAt)
                    .FirstOrDefaultAsync();

            if (financialAccount is null)
            {
                throw AppValidationException.BadRequest(
                    "Financial account is required for import.",
                    "financialAccountId",
                    "FINANCIAL_ACCOUNT_REQUIRED");
            }

            return financialAccount;
        }

        private async Task<ImportJob> GetOwnedImportJob(Guid id)
        {
            var userId = GetCurrentUserId();
            var job = await _dbContext.ImportJobs
                .Include(item => item.Drafts)
                    .ThenInclude(item => item.EditedCategory)
                .Include(item => item.Drafts)
                    .ThenInclude(item => item.EditedJar)
                .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

            if (job is null)
            {
                throw AppValidationException.NotFound(ErrorMessages.ImportNotFound, "id", ErrorCodes.ImportNotFound);
            }

            return job;
        }

        private static Response.ImportJobSummaryResponse MapJobSummary(ImportJob job)
        {
            return new Response.ImportJobSummaryResponse
            {
                Id = job.Id,
                FinancialAccountId = job.FinancialAccountId,
                FileName = job.FileName,
                OriginalContentType = job.OriginalContentType,
                Status = job.Status,
                Progress = job.Progress,
                ParsedCount = job.ParsedCount,
                FailedCount = job.FailedCount,
                ErrorMessage = job.ErrorMessage,
                ImageUrl = $"/api/v1/imports/images/{Uri.EscapeDataString(job.FileName)}",
                UploadedAt = job.UploadedAt,
                UpdatedAt = job.UpdatedAt
            };
        }

        private static Response.ImportJobDetailResponse MapJobDetail(ImportJob job)
        {
            var summary = MapJobSummary(job);
            return new Response.ImportJobDetailResponse
            {
                Id = summary.Id,
                FinancialAccountId = summary.FinancialAccountId,
                FileName = summary.FileName,
                OriginalContentType = summary.OriginalContentType,
                Status = summary.Status,
                Progress = summary.Progress,
                ParsedCount = summary.ParsedCount,
                FailedCount = summary.FailedCount,
                ErrorMessage = summary.ErrorMessage,
                ImageUrl = summary.ImageUrl,
                UploadedAt = summary.UploadedAt,
                UpdatedAt = summary.UpdatedAt,
                Drafts = job.Drafts
                    .OrderBy(draft => draft.RowIndex)
                    .Select(MapDraft)
                    .ToList()
            };
        }

        private static Response.ImportDraftResponse MapDraft(ImportTransactionDraft draft)
        {
            return new Response.ImportDraftResponse
            {
                Id = draft.Id,
                RowIndex = draft.RowIndex,
                TransactionDate = draft.TransactionDate,
                Amount = draft.Amount,
                Type = draft.Type,
                RawDescription = draft.RawDescription,
                EditedNote = draft.EditedNote,
                IsValid = draft.IsValid,
                ValidationError = draft.ValidationError,
                EditedCategoryId = draft.EditedCategoryId,
                EditedCategoryName = draft.EditedCategory?.Name,
                EditedJarId = draft.EditedJarId,
                EditedJarName = draft.EditedJar?.Name,
                CreatedAt = draft.CreatedAt,
                UpdatedAt = draft.UpdatedAt
            };
        }

        private static List<Response.OcrItemPreview> ExtractItems(string? rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                return [];
            }

            try
            {
                using var document = JsonDocument.Parse(rawJson);
                var items = new List<Response.OcrItemPreview>();
                ExtractItems(document.RootElement, items);
                return items
                    .GroupBy(item => new { item.Name, item.Amount })
                    .Select(group => group.First())
                    .ToList();
            }
            catch (JsonException)
            {
                return [];
            }
        }

        private static void ExtractItems(JsonElement element, ICollection<Response.OcrItemPreview> items)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty("items", out var itemsElement)
                    && itemsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var itemElement in itemsElement.EnumerateArray())
                    {
                        if (TryParseItem(itemElement, out var item))
                        {
                            items.Add(item);
                        }
                    }
                }

                foreach (var property in element.EnumerateObject())
                {
                    if (property.NameEquals("items"))
                    {
                        continue;
                    }

                    ExtractItems(property.Value, items);
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    ExtractItems(item, items);
                }
            }
        }

        private static bool TryParseItem(JsonElement element, out Response.OcrItemPreview item)
        {
            item = null!;
            if (element.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            var name = GetString(element, "name") ?? GetString(element, "raw_text");
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var amount = GetDecimal(element, "line_total")
                         ?? GetDecimal(element, "amount")
                         ?? GetDecimal(element, "total");

            item = new Response.OcrItemPreview
            {
                Name = name.Trim(),
                Amount = amount
            };
            return true;
        }

        private static string? GetString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var property)
                   && property.ValueKind == JsonValueKind.String
                ? property.GetString()
                : null;
        }

        private static decimal? GetDecimal(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
            {
                return null;
            }

            return property.ValueKind switch
            {
                JsonValueKind.Number => property.GetDecimal(),
                JsonValueKind.String when decimal.TryParse(property.GetString(), out var value) => value,
                _ => null
            };
        }

        private static bool IsDiscountItem(string name)
        {
            var normalized = name.Trim().ToLowerInvariant();
            return normalized.Contains("giam")
                   || normalized.Contains("discount")
                   || normalized.Contains("chiet khau");
        }

        private static void DeleteFileIfExists(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return;
            }

            try
            {
                File.Delete(filePath);
            }
            catch (IOException)
            {
                // Keep DB delete independent from best-effort filesystem cleanup.
            }
            catch (UnauthorizedAccessException)
            {
                // Keep DB delete independent from best-effort filesystem cleanup.
            }
        }

        private static string GetUploadFolderPath()
        {
            var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (currentDirectory is not null)
            {
                var directServiceProject = Path.Combine(
                    currentDirectory.FullName,
                    "Personal_Finance_Management.Service");
                if (Directory.Exists(directServiceProject))
                {
                    return Path.Combine(directServiceProject, "import", "Upload");
                }

                var nestedServiceProject = Path.Combine(
                    currentDirectory.FullName,
                    "Personal_Finance_Management",
                    "Personal_Finance_Management.Service");
                if (Directory.Exists(nestedServiceProject))
                {
                    return Path.Combine(nestedServiceProject, "import", "Upload");
                }

                currentDirectory = currentDirectory.Parent;
            }

            return Path.Combine(
                Directory.GetCurrentDirectory(),
                "Personal_Finance_Management",
                "Personal_Finance_Management.Service",
                "import",
                "Upload");
        }

        private static string GetOcrJsonFolderPath()
        {
            return Path.Combine(GetUploadFolderPath(), "OcrResponses");
        }

        private static string GetContentType(string filePath)
        {
            return Path.GetExtension(filePath).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".bmp" => "image/bmp",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }
    }
}
