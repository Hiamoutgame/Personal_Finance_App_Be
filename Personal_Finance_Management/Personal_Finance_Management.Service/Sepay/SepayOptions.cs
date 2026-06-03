namespace Personal_Finance_Management.Service.Sepay;

public class SepayOptions
{
    public const string SectionName = "Sepay";

    public string BaseUrl { get; set; } = "https://bankhub-api.sepay.vn/v1";
    public string AuthorizationUrl { get; set; } = "https://my.sepay.vn/oauth/authorize";
    public string TokenUrl { get; set; } = "https://my.sepay.vn/oauth/token";
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? RedirectUri { get; set; }
    public string Scope { get; set; } = "bank-account:read transaction:read";
    public string? ApiKey { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public string? WebhookApiKey { get; set; }
    public string? TokenEncryptionKey { get; set; }
}
