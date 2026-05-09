namespace Personal_Finance_Management.Service.FinancialAccount;

public class Response
{
    public class GetFinancialAccountResult
    {
        public required List<GetFinancialAccountResponse> data { get; set; }
    }
    public class GetFinancialAccountResponse
    {
        public required Guid id { get; set; }
        public required string name { get; set; }
        public required string accountType { get; set; }
        public required string connectionMode { get; set; }
        public required string? providerName { get; set; }
        public required string? maskedAccountNumber  { get; set; }
        public required string currency  { get; set; }
        public required decimal currentBalance  { get; set; }
        public required string syncStatus   { get; set; }
        public required bool isDefault { get; set; }
        public required bool isActive { get; set; } 
    }

    public class CreateManualFinancialAccountResponse
    {
        public required Guid id { get; set; }
        public required string name { get; set; }
        public required string accountType { get; set; }
        public required string connectionMode { get; set; }
        public required decimal currentBalance  { get; set; }
        public required string currency { get; set; }
        public required bool isDefault { get; set; }
        public required bool isActive { get; set; }
    }

    public class CreateLinkApiFinancialAccountResponse
    {
        public required Guid id { get; set; }
        public required string name { get; set; }
        public required string accountType { get; set; }
        public required string connectionMode { get; set; }
        public required string providerName { get; set; }
        public required string maskedAccountNumber { get; set; }
        public required decimal currentBalance { get; set; }
        public required string currency { get; set; }
        public required string syncStatus { get; set; }
        public required bool isDefault { get; set; }
        public required bool isActive { get; set; }
    }

    public class UpdateFinancialAccountResponse
    {
        public required Guid id { get; set; }
        public required string name { get; set; }
        public required decimal currentBalance  { get; set; }
        public required bool isDefault { get; set; }
        public required DateTimeOffset updatedAt { get; set; }
    }

    public class DeleteFinancialAccountResponse
    {
        public required string message { get; set; }
    }
}
