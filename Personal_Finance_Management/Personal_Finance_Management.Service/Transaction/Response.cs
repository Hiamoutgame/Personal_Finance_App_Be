namespace Personal_Finance_Management.Service.Transaction;

public class Response
{
    // GET /api/v1/transactions
    public class GetTransactionsResult
    {
        public required List<GetTransactionResponse> data { get; set; }
        public required PaginationResponse pagination { get; set; }
    }

    public class GetTransactionResponse
    {
        public required Guid id { get; set; }
        public required string type { get; set; }
        public required decimal transactionsAmount { get; set; }
        public required string? note { get; set; }
        public required DateTimeOffset date { get; set; }
        public required TransactionFinancialAccountResponse financialAccount { get; set; }
        public TransactionJarResponse? jar { get; set; }
        public TransactionCategoryResponse? category { get; set; }
    }

    public class TransactionFinancialAccountResponse
    {
        public required Guid? id { get; set; }
        public required string? name { get; set; }
    }

    public class TransactionJarResponse
    {
        public required Guid? id { get; set; }
        public required string? name { get; set; }
    }

    public class TransactionCategoryResponse
    {
        public required Guid? id { get; set; }
        public required string? name { get; set; }
    }

    public class PaginationResponse
    {
        public required int page { get; set; }
        public required int pageSize { get; set; }
        public required int totalCount { get; set; }
        public required int totalPages { get; set; }
    }

    // POST /api/v1/transactions
    public class CreateTransactionResponse
    {
        public required Guid id { get; set; }
        public required Guid? financialAccountId { get; set; }
        public required string type { get; set; }
        public required decimal transactionsAmount { get; set; }
        public required DateTimeOffset date { get; set; }
    }

    // PATCH /api/v1/transactions/{id}
    public class UpdateTransactionResponse
    {
        public required Guid id { get; set; }
        public required string type { get; set; }
        public required decimal transactionsAmount { get; set; }
        public required DateTimeOffset date { get; set; }
    }

    // DELETE /api/v1/transactions/{id}
    public class DeleteTransactionResponse
    {
        public required string message { get; set; }
    }

    public class CassoTransactionsResponse
    {
        public required int receivedCount { get; set; }
        public required int createdCount { get; set; }
        public required int skippedCount { get; set; }
        public required string message { get; set; }
    }
}
