using Personal_Finance_Management.Service.ocr;

namespace Personal_Finance_Management.Service.import
{
    public class Response
    {
        public class ImportImageResponse
        {
            public Guid? Id { get; set; }
            public Guid? FinancialAccountId { get; set; }
            public string? Status { get; set; }
            public string Message { get; set; } = "Imported file successfully";
            public required string FileName { get; set; }
            public required string OriginalFileName { get; set; }
            public required string StoredFilePath { get; set; }
            public string? ContentType { get; set; }
            public long SizeInBytes { get; set; }
            public string? OcrJsonFileName { get; set; }
            public string? StoredOcrJsonPath { get; set; }
            public string? RawOcrJson { get; set; }
            public OCRResult? OcrResult { get; set; }
            public ReceiptExtractionResult? Receipt { get; set; }
            public OcrPreviewResponse? Preview { get; set; }
        }

        public class ImportJobListResponse
        {
            public List<ImportJobSummaryResponse> Data { get; set; } = [];
            public PaginationResponse Pagination { get; set; } = new();
        }

        public class ImportJobSummaryResponse
        {
            public Guid Id { get; set; }
            public Guid FinancialAccountId { get; set; }
            public required string FileName { get; set; }
            public string? OriginalContentType { get; set; }
            public required string Status { get; set; }
            public int Progress { get; set; }
            public int ParsedCount { get; set; }
            public int FailedCount { get; set; }
            public string? ErrorMessage { get; set; }
            public string ImageUrl { get; set; } = string.Empty;
            public DateTimeOffset UploadedAt { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
        }

        public class ImportJobDetailResponse : ImportJobSummaryResponse
        {
            public List<ImportDraftResponse> Drafts { get; set; } = [];
        }

        public class ImportDraftResponse
        {
            public Guid Id { get; set; }
            public int RowIndex { get; set; }
            public DateTimeOffset? TransactionDate { get; set; }
            public decimal? Amount { get; set; }
            public string? Type { get; set; }
            public string? RawDescription { get; set; }
            public string? EditedNote { get; set; }
            public bool IsValid { get; set; }
            public string? ValidationError { get; set; }
            public Guid? EditedCategoryId { get; set; }
            public string? EditedCategoryName { get; set; }
            public Guid? EditedJarId { get; set; }
            public string? EditedJarName { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
        }

        public class PaginationResponse
        {
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalCount { get; set; }
            public int TotalPages { get; set; }
        }

        public class MessageResponse
        {
            public required string Message { get; set; }
        }

        public class ConfirmImportResponse
        {
            public Guid ImportJobId { get; set; }
            public required string Status { get; set; }
            public int CreatedCount { get; set; }
            public List<ConfirmedTransactionResponse> Transactions { get; set; } = [];
            public string Message { get; set; } = "Import confirmed";
        }

        public class ConfirmedTransactionResponse
        {
            public Guid Id { get; set; }
            public Guid DraftId { get; set; }
            public Guid? FinancialAccountId { get; set; }
            public Guid? FromJarId { get; set; }
            public Guid? CategoryId { get; set; }
            public required string Type { get; set; }
            public decimal TransactionsAmount { get; set; }
            public DateTimeOffset TransactionDate { get; set; }
        }

        public class OcrPreviewResponse
        {
            public required string Id { get; set; }
            public required string Status { get; set; }
            public required string ImageUrl { get; set; }
            public required OcrTransactionPreview Transaction { get; set; }
            public List<OcrItemPreview> Items { get; set; } = [];
            public required OcrSummaryPreview Summary { get; set; }
            public List<string> Warnings { get; set; } = [];
        }

        public class OcrTransactionPreview
        {
            public string? MerchantName { get; set; }
            public decimal? Amount { get; set; }
            public DateTimeOffset? Date { get; set; }
            public string Type { get; set; } = "Expense";
            public Guid? SuggestedCategoryId { get; set; }
            public string? SuggestedCategoryName { get; set; }
            public string? MatchedBy { get; set; }
            public string? Note { get; set; }
        }

        public class OcrItemPreview
        {
            public required string Name { get; set; }
            public decimal? Amount { get; set; }
        }

        public class OcrSummaryPreview
        {
            public decimal? Subtotal { get; set; }
            public decimal? Discount { get; set; }
            public decimal? Total { get; set; }
        }

        public class UploadedFileResponse
        {
            public required string FileName { get; set; }
            public required string StoredFilePath { get; set; }
            public required string ContentType { get; set; }
        }
    }
}
