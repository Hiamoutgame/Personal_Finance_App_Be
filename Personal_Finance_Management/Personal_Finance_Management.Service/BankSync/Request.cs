using System.Text.Json;

namespace Personal_Finance_Management.Service.BankSync;

public class Request
{
    public class SyncLinkedAccountRequest
    {
        public DateOnly? fromDate { get; set; }
        public DateOnly? toDate { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 50;
        public string? sort { get; set; } = "ASC";
        public bool triggerProviderSync { get; set; }
    }

    public class SepayWebhookRequest
    {
        public long id { get; set; }
        public string? gateway { get; set; }
        public string? transactionDate { get; set; }
        public string? accountNumber { get; set; }
        public string? subAccount { get; set; }
        public string? code { get; set; }
        public string? content { get; set; }
        public string? transferType { get; set; }
        public string? description { get; set; }
        public decimal transferAmount { get; set; }
        public decimal? accumulated { get; set; }
        public string? referenceCode { get; set; }
        public JsonElement? raw { get; set; }
    }
}
