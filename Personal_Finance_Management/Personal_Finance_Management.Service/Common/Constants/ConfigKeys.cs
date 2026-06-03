namespace Personal_Finance_Management.Service.Common.Constants;

public static class ConfigKeys
{
    public static class Jwt
    {
        public const string Section = "Jwt";
        public const string Secret = "Jwt:Secret";
        public const string Issuer = "Jwt:Issuer";
        public const string Audience = "Jwt:Audience";
        public const string ExpiryMinutes = "Jwt:ExpiryMinutes";
    }

    public static class Sepay
    {
        public const string Section = "Sepay";
        public const string ConnectionMode = "Sepay:ConnectionMode";
        public const string ClientId = "Sepay:ClientId";
        public const string ClientSecret = "Sepay:ClientSecret";
        public const string RedirectUri = "Sepay:RedirectUri";
        public const string DefaultReturnUrl = "Sepay:DefaultReturnUrl";
        public const string AllowedReturnUrlPrefix = "Sepay:AllowedReturnUrlPrefix";
        public const string Scope = "Sepay:Scope";
        public const string BaseUrl = "Sepay:BaseUrl";
        public const string AuthorizationUrl = "Sepay:AuthorizationUrl";
        public const string TokenUrl = "Sepay:TokenUrl";
        public const string ApiKey = "Sepay:ApiKey";
        public const string WebhookApiKey = "Sepay:WebhookApiKey";
        public const string TokenEncryptionKey = "Sepay:TokenEncryptionKey";
        public const string TimeoutSeconds = "Sepay:TimeoutSeconds";
    }

    public static class GoogleAi
    {
        public const string Section = "GoogleAI";
        public const string Temperature = "GoogleAI:Temperature";
        public const string MaxTokens = "GoogleAI:MaxTokens";
        public const string IsEnabled = "GoogleAI:IsEnabled";
        public const string TimeoutSeconds = "GoogleAI:TimeoutSeconds";
    }

    public static class BroadcastDispatch
    {
        public const string Section = "BroadcastDispatch";
        public const string IntervalSeconds = "BroadcastDispatch:IntervalSeconds";
    }

    public static class SeedAccounts
    {
        public const string Section = "SeedAccountsOptions";
    }
}
