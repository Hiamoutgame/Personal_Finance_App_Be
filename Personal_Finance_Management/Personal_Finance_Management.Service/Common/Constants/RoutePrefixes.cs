namespace Personal_Finance_Management.Service.Common.Constants;

public static class RoutePrefixes
{
    public const string ApiV1 = "api/v1";
    public const string Admin = ApiV1 + "/admin";
}

public static class Routes
{
    public const string Auth = RoutePrefixes.ApiV1 + "/auth";
    public const string User = RoutePrefixes.ApiV1 + "/user";
    public const string Onboarding = RoutePrefixes.ApiV1 + "/onboarding";
    public const string Categories = RoutePrefixes.ApiV1 + "/categories";
    public const string Jars = RoutePrefixes.ApiV1 + "/jars";
    public const string Transactions = RoutePrefixes.ApiV1 + "/transactions";
    public const string FinancialAccounts = RoutePrefixes.ApiV1 + "/financial-accounts";
    public const string Goals = RoutePrefixes.ApiV1 + "/goals";
    public const string SpendingLimits = RoutePrefixes.ApiV1 + "/spending-limits";
    public const string Reminders = RoutePrefixes.ApiV1 + "/reminders";
    public const string Notifications = RoutePrefixes.ApiV1 + "/notifications";
    public const string Dashboard = RoutePrefixes.ApiV1 + "/dashboard";
    public const string Imports = RoutePrefixes.ApiV1 + "/imports";
    public const string Ai = RoutePrefixes.ApiV1 + "/ai";

    public const string AdminUsers = RoutePrefixes.Admin + "/users";
    public const string AdminCategories = RoutePrefixes.Admin + "/categories";
    public const string AdminBroadcasts = RoutePrefixes.Admin + "/broadcasts";
    public const string AdminDashboard = RoutePrefixes.Admin + "/dashboard";
    public const string AdminAudits = RoutePrefixes.Admin + "/audits";
    public const string AdminAiSettings = RoutePrefixes.Admin + "/ai-settings";
}
