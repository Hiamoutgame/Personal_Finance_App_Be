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

    public static class Casso
    {
        public const string Section = "Casso";
        public const string WebhookSecureToken = "Casso:WebhookSecureToken";
        public const string TimeoutSeconds = "Casso:TimeoutSeconds";
    }

    public static class CasooOptions
    {
        public const string Section = "CasooOptions";
        public const string SecureToken = "CasooOptions:SecureToken";
        public const string TimeoutSeconds = "CasooOptions:TimeoutSeconds";
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
