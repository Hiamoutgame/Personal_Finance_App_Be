namespace Personal_Finance_Management.Service.Casso;

public class CassoOptions
{
    public const string SectionName = "Casso";

    public string BaseUrl { get; set; } = "https://oauth.casso.vn/v2";
    public string AuthorizationUrl { get; set; } = "https://oauth.casso.vn/auth/authorize";
    public string TokenUrl { get; set; } = "https://oauth.casso.vn/auth/token";
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? RedirectUri { get; set; }
    public string Scope { get; set; } = "webhook transaction";
    public string? ApiKey { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public string? WebhookSecureToken { get; set; }
    public string? TokenEncryptionKey { get; set; }
}
