using Microsoft.AspNetCore.Http;

namespace Personal_Finance_Management.Service.import
{
    public class Request
    {
        public class ImportData
        {
            public required IFormFile File { get; set; }
            public Guid? FinancialAccountId { get; set; }
            public string? BankCode { get; set; }
            public string? Layout { get; set; }
            public bool RunOcr { get; set; } = true;
            public bool IncludeDebug { get; set; } = false;
        }

        public class GetImportsRequest
        {
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 20;
            public string? Status { get; set; }
            public Guid? FinancialAccountId { get; set; }
        }

        public class UpdateImportDraftRequest
        {
            public DateTimeOffset? TransactionDate { get; set; }
            public decimal? Amount { get; set; }
            public string? Type { get; set; }
            public string? EditedNote { get; set; }
            public Guid? EditedCategoryId { get; set; }
            public Guid? EditedJarId { get; set; }
            public bool? IsValid { get; set; }
            public string? ValidationError { get; set; }
        }

        public class ConfirmImportRequest
        {
            public Guid? FinancialAccountId { get; set; }
            public Guid? FromJarId { get; set; }
            public List<Guid>? DraftIds { get; set; }
        }
    }
}
