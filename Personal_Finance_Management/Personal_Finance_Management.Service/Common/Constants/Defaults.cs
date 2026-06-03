namespace Personal_Finance_Management.Service.Common.Constants;

public static class PaginationDefaults
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}

public static class IntegrationDefaults
{
    public const int SepayTimeoutSeconds = 30;
    public const int GoogleAiTimeoutSeconds = 300;
    public const int BroadcastDispatchIntervalSeconds = 60;
}

public static class AiDefaults
{
    public const double Temperature = 0.7;
    public const int MaxTokens = 1000;
}

public static class CurrencyDefaults
{
    public const string Vnd = "VND";
}
