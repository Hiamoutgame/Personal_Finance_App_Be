namespace Personal_Finance_Management.Service.BankConnection;

public class Request
{
    public class StartCassoConnectionRequest
    {
        public string? returnUrl { get; set; }
        public bool? isDefault { get; set; }
        public bool? autoSync { get; set; }
    }
}
