using System.Text.Json;

namespace Personal_Finance_Management.Service.Transaction;

public class Request
{
    // GET /api/v1/transactions (query params)
    public class GetTransactionsRequest
    {
        public int pageIndex { get; set; } = 1;
        public int pageSize { get; set; } = 20;
        public Guid? financialAccountId { get; set; }
        public string? type { get; set; }
        public Guid? jarId { get; set; }
        public Guid? categoryId { get; set; }
        public DateOnly? fromDate { get; set; }
        public DateOnly? toDate { get; set; }
        public string? keyword { get; set; }
        public string? sortBy { get; set; }
        public string? sortDir { get; set; }
    }

    // POST /api/v1/transactions
    public class CreateTransactionRequest
    {
        public Guid? financialAccountId { get; set; }
        public string type { get; set; }
        public decimal transactionsAmount { get; set; }
        public Guid? categoryId { get; set; }
        public Guid? fromJarId { get; set; }
        public Guid? toJarId { get; set; }
        public string? note { get; set; }
        public DateTimeOffset date { get; set; }
    }

    // PATCH /api/v1/transactions/{id}
    public class UpdateTransactionRequest
    {
        public decimal? transactionsAmount { get; set; }
        public Guid? categoryId { get; set; }
        public string? note { get; set; }
    }

    public class CassoWebhookRequest
    {
        public int error { get; set; }
        public JsonElement data { get; set; }
    }

    public class CassoSyncTransactionsRequest
    {
        public Guid financialAccountId { get; set; }
        public DateOnly? fromDate { get; set; }
        public DateOnly? toDate { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 20;
        public string? sort { get; set; } = "ASC";
    }
}
