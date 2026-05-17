namespace Personal_Finance_Management.Service.BankSync;

public class Response
{
    public class CassoTransactionsResponse
    {
        public required int receivedCount { get; set; }
        public required int createdCount { get; set; }
        public required int skippedCount { get; set; }
        public required string message { get; set; }
    }
}
