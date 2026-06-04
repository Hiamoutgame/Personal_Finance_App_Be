using System.Text.Json;
using Helpers = Personal_Finance_Management.Api.Tests.Helpers;

namespace Personal_Finance_Management.Api.Tests;

/// <summary>
/// Base class for all Playwright API test fixtures.
///
/// Manages the Playwright lifecycle (IPlaywright + IAPIRequestContext),
/// provides cached JWT token helpers, and shared assertion utilities.
///
/// === CÁCH DÙNG ===
///
/// 1. Tạo class kế thừa ApiTestBase:
///
///     [TestFixture]
///     [NonParallelizable] // tránh conflict dữ liệu giữa các class
///     public class AuthTests : ApiTestBase
///     {
///         [Test]
///         public async Task Login_WithValidCredentials_ReturnsToken()
///         {
///             // Lấy token đã cache
///             var token = await GetNormalUserTokenAsync();
///
///             // Gọi API với token
///             var response = await ApiContext.GetAsync("/path", new()
///             {
///                 Headers = AuthHeaders(token)
///             });
///
///             // Assert
///             Assert.That(response.Ok, Is.True);
///         }
///     }
///
/// 2. Mỗi class test có context riêng, login 1 lần, token được cache.
///    Dữ liệu test tạo ra dùng prefix "TEST-" + GUID để tránh trùng.
///    Dùng TrackId() để ghi nhận resource cần cleanup trong OneTimeTearDown.
/// </summary>
public abstract class ApiTestBase
{
    // ─── Playwright ────────────────────────────────────────────

    protected IPlaywright Playwright { get; private set; } = null!;
    protected IAPIRequestContext ApiContext { get; private set; } = null!;

    // ─── Token cache ───────────────────────────────────────────

    protected string? NormalUserToken { get; private set; }
    protected string? AdminToken { get; private set; }

    // ─── Resource tracking for cleanup ─────────────────────────

    /// <summary>
    /// Track a resource ID for cleanup in OneTimeTearDown.
    /// Key = resource ID string, Value = delete-URL relative path (e.g. "/api/v1/goals/{id}").
    /// </summary>
    protected List<(string Id, string DeletePath)> CreatedResources { get; } = new();

    /// <summary>
    /// Register a resource for cleanup.
    /// </summary>
    /// <param name="id">The resource's GUID as string.</param>
    /// <param name="deletePath">The DELETE endpoint path. Use "{id}" as placeholder.</param>
    protected void TrackId(string id, string deletePath)
    {
        CreatedResources.Add((id, deletePath.Replace("{id}", id)));
    }

    // ─── Lifecycle ─────────────────────────────────────────────

    [OneTimeSetUp]
    public virtual async Task OneTimeSetUp()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        ApiContext = await Playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = Helpers.TestConfig.BaseUrl,
        });
    }

    [OneTimeTearDown]
    public virtual async Task OneTimeTearDown()
    {
        // Clean up tracked resources (best-effort)
        if (CreatedResources.Count > 0)
        {
            var token = NormalUserToken ?? AdminToken;
            if (token is not null)
            {
                foreach (var (_, path) in CreatedResources)
                {
                    try
                    {
                        await ApiContext.DeleteAsync(path, new() { Headers = AuthHeaders(token) });
                    }
                    catch
                    {
                        // Best-effort cleanup; ignore failures
                    }
                }
            }
            CreatedResources.Clear();
        }

        if (ApiContext is not null)
            await ApiContext.DisposeAsync();

        Playwright?.Dispose();
        Playwright = null!;
    }

    // ─── Auth helpers ──────────────────────────────────────────

    /// <summary>
    /// Log in as the seeded normal user and return a cached JWT.
    /// </summary>
    protected async Task<string> GetNormalUserTokenAsync()
    {
        if (NormalUserToken is not null) return NormalUserToken;
        NormalUserToken = await LoginAsync(
            Helpers.TestConfig.NormalUserEmail,
            Helpers.TestConfig.NormalUserPassword);
        return NormalUserToken;
    }

    /// <summary>
    /// Log in as the seeded admin user and return a cached JWT.
    /// </summary>
    protected async Task<string> GetAdminTokenAsync()
    {
        if (AdminToken is not null) return AdminToken;
        AdminToken = await LoginAsync(
            Helpers.TestConfig.AdminUserEmail,
            Helpers.TestConfig.AdminUserPassword);
        return AdminToken;
    }

    private async Task<string> LoginAsync(string email, string password)
    {
        var response = await ApiContext.PostAsync("/api/v1/auth/login", new()
        {
            DataObject = new { email, password }
        });

        Assert.That((int)response.Status, Is.EqualTo(200),
            $"Login failed for {email}. Body: {await response.TextAsync()}");

        var json = await response.JsonAsync<JsonElement>();
        var token = json.GetProperty("accessToken").GetString();

        Assert.That(token, Is.Not.Null.And.Not.Empty, "accessToken was null or empty in login response");
        return token;
    }

    // ─── Header helpers ────────────────────────────────────────

    /// <summary>
    /// Build a headers dictionary with the Authorization Bearer token.
    /// </summary>
    protected static Dictionary<string, string> AuthHeaders(string token) => new()
    {
        ["Authorization"] = $"Bearer {token}"
    };

    // ─── Assertion helpers ─────────────────────────────────────

    /// <summary>
    /// Assert the response has the exact expected HTTP status code.
    /// Returns the response for fluent chaining.
    /// </summary>
    /// <summary>
    /// Assert the response has the exact expected HTTP status code.
    /// Returns the parsed JSON body for fluent chaining.
    /// </summary>
    protected static async Task<JsonElement> AssertStatus(IAPIResponse response, int expectedStatus, string? context = null)
    {
        var prefix = context is not null ? $"[{context}] " : "";
        Assert.That((int)response.Status, Is.EqualTo(expectedStatus),
            $"{prefix}Expected {expectedStatus}, got {(int)response.Status}. Body: {await response.TextAsync()}");
        return await response.JsonAsync<JsonElement>();
    }

    /// <summary>
    /// Assert the response is 200 OK. Fluent — returns the parsed body.
    /// </summary>
    protected static async Task<JsonElement> AssertOk(IAPIResponse response, string? context = null)
    {
        Assert.That(response.Ok, Is.True,
            $"[{context}] Expected 2xx. Got {(int)response.Status}. Body: {await response.TextAsync()}");
        return await response.JsonAsync<JsonElement>();
    }

    /// <summary>
    /// Assert the response is 401 Unauthorized.
    /// </summary>
    protected static async Task AssertUnauthorized(IAPIResponse response, string? context = null)
    {
        await AssertStatus(response, 401, context);
    }

    /// <summary>
    /// Assert the response is 403 Forbidden.
    /// </summary>
    protected static async Task AssertForbidden(IAPIResponse response, string? context = null)
    {
        await AssertStatus(response, 403, context);
    }

    /// <summary>
    /// Assert the response is a JSON error with the expected error code string.
    /// </summary>
    protected static async Task AssertErrorCode(IAPIResponse response, string expectedCode)
    {
        var json = await response.JsonAsync<JsonElement>();
        if (json.TryGetProperty("code", out var code))
        {
            Assert.That(code.GetString(), Is.EqualTo(expectedCode),
                $"Expected error code '{expectedCode}' but got '{code.GetString()}'");
        }
    }

    /// <summary>
    /// Generate a unique test name prefix to avoid data collisions.
    /// </summary>
    protected static string UniqueName(string prefix = "TEST") =>
        $"{prefix}-{Guid.NewGuid():N}"[..Math.Min($"{prefix}-{Guid.NewGuid():N}".Length, 40)];
}
