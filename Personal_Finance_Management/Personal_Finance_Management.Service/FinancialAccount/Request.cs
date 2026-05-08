namespace Personal_Finance_Management.Service.FinancialAccount;

public class Request
{
    public class CreateManualFinancialAccountRequest
    {
        public string name { get; set; }
        public string accountType  { get; set; }
        public decimal currentBalance { get; set; }
        public string? currency  { get; set; }
        public bool isDefault { get; set; }
    }

    public class CreateLinkApiFinancialAccountRequest
    {
        public string bankName { get; set; }
        public string? bankCode { get; set; }
        public string accountNumber { get; set; }
        public string? accountHolderName { get; set; }
        public bool isDefault { get; set; }
    }

    public class UpdateFinancialAccountRequest
    {
        public string? name { get; set; }
        public decimal? currentBalance { get; set; }
        public bool? isDefault { get; set; }
    }
}
