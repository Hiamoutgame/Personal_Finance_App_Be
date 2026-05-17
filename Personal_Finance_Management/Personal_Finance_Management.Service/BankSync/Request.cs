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

    public class CassoWebhookRequest
    {
        public int error { get; set; }
        public JsonElement data { get; set; }
    }
}
