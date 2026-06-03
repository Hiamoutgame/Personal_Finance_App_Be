namespace Personal_Finance_Management.Service.BankConnection;

public class Response
{
    public class StartSepayConnectionResponse
    {
        public required bool success { get; set; }
        public required string message { get; set; }
        public required string connectionMode { get; set; }
        public Guid? sessionId { get; set; }
        public string? authorizationUrl { get; set; }
        public DateTimeOffset? expiresAt { get; set; }
        public Guid? financialAccountId { get; set; }
        public required List<Guid> financialAccountIds { get; set; }
    }

    public class SepayCallbackResponse
    {
        public required bool success { get; set; }
        public required string message { get; set; }
        public Guid? financialAccountId { get; set; }
        public string? redirectUrl { get; set; }
    }
}
