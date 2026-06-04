using System.Text.Json;

namespace Personal_Finance_Management.Api.Tests.Tests;

/// <summary>
/// === HƯỚNG DẪN VIẾT PLAYWIGHT TEST: Auth Tests ===
///
/// Playwright API test là dùng IAPIRequestContext để gọi HTTP request
/// vào API đang chạy, kiểm tra status code, JSON response body.
///
/// Pattern cơ bản:
/// 1. Tạo class kế thừa ApiTestBase
/// 2. Đánh dấu [TestFixture] và [NonParallelizable]
/// 3. Mỗi test là 1 async Task method với attribute [Test]
/// 4. Gọi API qua ApiContext.PostAsync/GetAsync/PatchAsync/DeleteAsync
/// 5. Dùng Assert.That(...) của NUnit hoặc FluentAssertions để kiểm tra
///
/// Ví dụ nhỏ nhất:
///   [Test]
///   public async Task Login_BasicExample()
///   {
///       var response = await ApiContext.PostAsync("/api/v1/auth/login", new()
///       {
///           DataObject = new { email = "...", password = "..." }
///       });
///       Assert.That(response.Ok, Is.True);
///       await AssertOk(response); // fluent helper từ ApiTestBase
///   }
/// </summary>
[TestFixture]
[NonParallelizable]
public class AuthTests : ApiTestBase
{
    // ─── Login tests ───────────────────────────────────────────

    /// <summary>
    /// Test: Đăng nhập với đúng credentials → 200 + có accessToken
    ///
    /// Mẫu: Kiểm tra happy path của 1 API trả về JSON.
    /// </summary>
    [Test]
    public async Task Login_WithValidCredentials_ReturnsTokenAndUserInfo()
    {
        var response = await ApiContext.PostAsync("/api/v1/auth/login", new()
        {
            DataObject = new
            {
                email = Helpers.TestConfig.NormalUserEmail,
                password = Helpers.TestConfig.NormalUserPassword
            }
        });

        // Cách 1: assert bằng helper có sẵn
        var json = await AssertOk(response, "Login");

        // Cách 2: kiểm tra các field trong JSON response
        Assert.That(json.TryGetProperty("accessToken", out var token), Is.True,
            "Response phải chứa accessToken");
        Assert.That(token.GetString(), Is.Not.Null.And.Not.Empty);
        Assert.That(json.TryGetProperty("email", out _), Is.True);
    }

    /// <summary>
    /// Test: Login sai password → 401 (Unauthorized)
    ///
    /// Mẫu: Kiểm tra error status code với helper AssertStatus.
    /// </summary>
    [Test]
    public async Task Login_WithInvalidPassword_Returns401()
    {
        var response = await ApiContext.PostAsync("/api/v1/auth/login", new()
        {
            DataObject = new
            {
                email = Helpers.TestConfig.NormalUserEmail,
                password = "WrongPassword123@"
            }
        });

        await AssertStatus(response, 401, "Login with wrong password");
    }

    /// <summary>
    /// Test: Login với email không tồn tại → 401
    /// </summary>
    [Test]
    public async Task Login_WithNonExistentEmail_Returns401()
    {
        var response = await ApiContext.PostAsync("/api/v1/auth/login", new()
        {
            DataObject = new
            {
                email = $"nonexistent-{Guid.NewGuid():N}@test.com",
                password = "SomePassword123@"
            }
        });

        await AssertStatus(response, 401, "Login with non-existent email");
    }

    /// <summary>
    /// Test: Login thiếu trường email → 400 (Bad Request) - validation error
    ///
    /// Mẫu: Kiểm tra validation error response JSON.
    /// </summary>
    [Test]
    public async Task Login_WithMissingEmail_Returns400()
    {
        var response = await ApiContext.PostAsync("/api/v1/auth/login", new()
        {
            DataObject = new
            {
                password = "SomePassword123@"
                // Thiếu "email"
            }
        });

        Assert.That((int)response.Status, Is.EqualTo(400),
            $"Expected 400, got {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    /// <summary>
    /// Test: Login thiếu trường password → 400
    /// </summary>
    [Test]
    public async Task Login_WithMissingPassword_Returns400()
    {
        var response = await ApiContext.PostAsync("/api/v1/auth/login", new()
        {
            DataObject = new
            {
                email = Helpers.TestConfig.NormalUserEmail
                // Thiếu "password"
            }
        });

        Assert.That((int)response.Status, Is.EqualTo(400),
            $"Expected 400, got {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    // ─── Register tests ────────────────────────────────────────

    /// <summary>
    /// Test: Register user mới với dữ liệu hợp lệ → 201 Created + accessToken
    ///
    /// Mẫu: Kiểm tra POST tạo resource, response trả về 201.
    /// </summary>
    [Test]
    public async Task Register_WithValidData_ReturnsCreated()
    {
        var uniqueEmail = $"test-register-{Guid.NewGuid():N}@test.com";

        var response = await ApiContext.PostAsync("/api/v1/auth/register", new()
        {
            DataObject = new
            {
                username = $"testuser_{Guid.NewGuid():N}"[..20],
                email = uniqueEmail,
                password = "TestPass123@",
                firstName = "Test",
                lastName = "Register"
            }
        });

        await AssertStatus(response, 201, "Register new user");

        var json = await response.JsonAsync<JsonElement>();
        Assert.That(json.TryGetProperty("accessToken", out var token), Is.True);
        Assert.That(token.GetString(), Is.Not.Null.And.Not.Empty);
    }

    /// <summary>
    /// Test: Register với email đã tồn tại → 400
    ///
    /// Mẫu: Kiểm tra conflict/business logic error.
    /// </summary>
    [Test]
    public async Task Register_WithDuplicateEmail_Returns400()
    {
        var response = await ApiContext.PostAsync("/api/v1/auth/register", new()
        {
            DataObject = new
            {
                username = "dupetestuser",
                email = Helpers.TestConfig.NormalUserEmail, // Email đã tồn tại
                password = "TestPass123@",
                firstName = "Test",
                lastName = "Dup"
            }
        });

        Assert.That((int)response.Status, Is.EqualTo(400).Or.EqualTo(409),
            $"Expected 400/409, got {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    /// <summary>
    /// Test: Register với email sai định dạng → 400 validation error
    /// </summary>
    [Test]
    public async Task Register_WithInvalidEmail_Returns400()
    {
        var response = await ApiContext.PostAsync("/api/v1/auth/register", new()
        {
            DataObject = new
            {
                username = "testuser2",
                email = "not-an-email",
                password = "TestPass123@",
                firstName = "Test",
                lastName = "BadEmail"
            }
        });

        Assert.That((int)response.Status, Is.EqualTo(400),
            $"Expected 400, got {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    // ─── Cache test (verify GetNormalUserTokenAsync works) ─────

    /// <summary>
    /// Test: Kiểm tra cơ chế cache token.
    /// Gọi GetNormalUserTokenAsync 2 lần → cả 2 lần đều trả về token giống nhau.
    /// </summary>
    [Test]
    public async Task GetNormalUserToken_CachesToken()
    {
        var token1 = await GetNormalUserTokenAsync();
        var token2 = await GetNormalUserTokenAsync();

        Assert.That(token1, Is.EqualTo(token2), "Token should be cached and identical");
        Assert.That(token1, Is.Not.Null.And.Not.Empty);
    }
}
