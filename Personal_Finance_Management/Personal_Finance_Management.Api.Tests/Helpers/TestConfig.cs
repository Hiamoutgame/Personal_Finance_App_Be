using Microsoft.Extensions.Configuration;

namespace Personal_Finance_Management.Api.Tests.Helpers;

/// <summary>
/// Provides test configuration loaded from appsettings.test.json.
/// Values can be overridden via environment variables for CI/CD.
/// </summary>
public static class TestConfig
{
    private static readonly IConfigurationRoot _config;

    static TestConfig()
    {
        _config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables("TEST_")
            .Build();
    }

    /// <summary>
    /// Base URL of the running API. Override with env var TEST_BASEURL.
    /// </summary>
    public static string BaseUrl =>
        Environment.GetEnvironmentVariable("TEST_BASEURL") ??
        _config["BaseUrl"] ??
        "http://localhost:5284";

    public static string NormalUserEmail =>
        Environment.GetEnvironmentVariable("TEST_NORMALUSEREMAIL") ??
        _config["TestUsers:NormalUser:Email"]!;

    public static string NormalUserPassword =>
        Environment.GetEnvironmentVariable("TEST_NORMALUSERPASSWORD") ??
        _config["TestUsers:NormalUser:Password"]!;

    public static string AdminUserEmail =>
        Environment.GetEnvironmentVariable("TEST_ADMINUSEREMAIL") ??
        _config["TestUsers:AdminUser:Email"]!;

    public static string AdminUserPassword =>
        Environment.GetEnvironmentVariable("TEST_ADMINUSERPASSWORD") ??
        _config["TestUsers:AdminUser:Password"]!;
}
