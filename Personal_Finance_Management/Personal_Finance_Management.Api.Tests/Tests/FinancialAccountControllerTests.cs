using System.Text.Json;

namespace Personal_Finance_Management.Api.Tests.Tests;

/// <summary>
/// === HƯỚNG DẪN: FinancialAccount Tests (CRUD pattern) ===
///
/// Đây là pattern CRUD đầy đủ: Create → Read → Update → Delete
/// Mỗi test tạo resource, sau đó cleanup qua mechanism TrackId().
///
/// Pattern viết CRUD test:
/// 1. Test GET list (có token, không token)
/// 2. Test POST create (valid data, missing fields)
/// 3. Test PATCH update (id tồn tại, id không tồn tại)
/// 4. Test DELETE (id tồn tại, id không tồn tại)
///
/// Mỗi resource tạo ra được gắn prefix "TEST-" + GUID để tránh trùng.
/// Dùng TrackId() để tự động cleanup trong OneTimeTearDown.
/// </summary>
[TestFixture]
[NonParallelizable]
public class FinancialAccountControllerTests : ApiTestBase
{
    // ─── GET list ──────────────────────────────────────────────

    [Test]
    public async Task GetFinancialAccounts_WithoutAuth_Returns401()
    {
        var response = await ApiContext.GetAsync("/api/v1/financial-accounts");
        await AssertUnauthorized(response, "Financial accounts without token");
    }

    [Test]
    public async Task GetFinancialAccounts_AsNormalUser_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var response = await ApiContext.GetAsync("/api/v1/financial-accounts", new()
        {
            Headers = AuthHeaders(token)
        });

        var json = await AssertOk(response, "Financial accounts");
        Assert.That(json.ValueKind, Is.EqualTo(JsonValueKind.Array),
            "Financial accounts response should be an array");
    }

    // ─── Create ────────────────────────────────────────────────

    /// <summary>
    /// Test: Tạo manual financial account → 200 + response có id
    ///
    /// Mẫu: POST với dữ liệu hợp lệ, kiểm tra response có id, track để cleanup.
    /// </summary>
    [Test]
    public async Task CreateManualAccount_WithValidData_Returns200()
    {
        var token = await GetNormalUserTokenAsync();
        var uniqueName = UniqueName("Test Cash Account");

        var response = await ApiContext.PostAsync("/api/v1/financial-accounts/Manual", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                name = uniqueName,
                accountType = "Cash",
                currentBalance = 1_000_000,
                currency = "VND",
                isDefault = false
            }
        });

        var json = await AssertOk(response, "Create manual account");
        Assert.That(json.TryGetProperty("id", out var idProp), Is.True,
            $"Response must have 'id'. Got: {await response.TextAsync()}");

        var id = idProp.GetString();
        Assert.That(id, Is.Not.Null.And.Not.Empty);

        // Track để cleanup tự động
        TrackId(id!, "/api/v1/financial-accounts/{id}");
    }

    /// <summary>
    /// Test: Tạo account với balance = 0 (edge case hợp lệ)
    /// </summary>
    [Test]
    public async Task CreateManualAccount_WithZeroBalance_Returns200()
    {
        var token = await GetNormalUserTokenAsync();
        var uniqueName = UniqueName("Test Zero Balance");

        var response = await ApiContext.PostAsync("/api/v1/financial-accounts/Manual", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                name = uniqueName,
                accountType = "Cash",
                currentBalance = 0,
                currency = "VND",
                isDefault = false
            }
        });

        var json = await AssertOk(response, "Create zero-balance account");
        var id = json.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
        if (id is not null) TrackId(id, "/api/v1/financial-accounts/{id}");
    }

    // ─── Update ────────────────────────────────────────────────

    [Test]
    public async Task UpdateAccount_WithValidData_Returns200()
    {
        var token = await GetNormalUserTokenAsync();
        var uniqueName = UniqueName("Test Update Account");

        // Tạo account trước
        var createResponse = await ApiContext.PostAsync("/api/v1/financial-accounts/Manual", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                name = uniqueName,
                accountType = "Cash",
                currentBalance = 500_000,
                currency = "VND",
                isDefault = false
            }
        });
        var createJson = await AssertOk(createResponse, "Create for update");
        var id = createJson.GetProperty("id").GetString()!;
        TrackId(id, "/api/v1/financial-accounts/{id}");

        // Update
        var response = await ApiContext.PatchAsync($"/api/v1/financial-accounts/{id}", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                name = uniqueName + " - Updated",
                currentBalance = 750_000
            }
        });

        // Có thể trả về 200 hoặc 204 tùy API
        Assert.That(response.Ok, Is.True,
            $"Update should succeed. Status: {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    [Test]
    public async Task UpdateAccount_NonExistent_Returns404()
    {
        var token = await GetNormalUserTokenAsync();
        var fakeId = Guid.NewGuid();

        var response = await ApiContext.PatchAsync($"/api/v1/financial-accounts/{fakeId}", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new { name = "Does Not Exist" }
        });

        Assert.That((int)response.Status, Is.EqualTo(404).Or.EqualTo(400),
            $"Expected 404/400 for non-existent. Got {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    // ─── Delete ────────────────────────────────────────────────

    [Test]
    public async Task DeleteAccount_WithValidId_Returns200()
    {
        var token = await GetNormalUserTokenAsync();
        var uniqueName = UniqueName("Test Delete Account");

        // Tạo trước
        var createResponse = await ApiContext.PostAsync("/api/v1/financial-accounts/Manual", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                name = uniqueName,
                accountType = "Cash",
                currentBalance = 100_000,
                currency = "VND",
                isDefault = false
            }
        });
        var createJson = await AssertOk(createResponse, "Create for delete");
        var id = createJson.GetProperty("id").GetString()!;

        // Xóa
        var response = await ApiContext.DeleteAsync($"/api/v1/financial-accounts/{id}", new()
        {
            Headers = AuthHeaders(token)
        });

        Assert.That(response.Ok, Is.True,
            $"Delete should succeed. Status: {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    [Test]
    public async Task DeleteAccount_NonExistent_Returns404()
    {
        var token = await GetNormalUserTokenAsync();
        var fakeId = Guid.NewGuid();

        var response = await ApiContext.DeleteAsync($"/api/v1/financial-accounts/{fakeId}", new()
        {
            Headers = AuthHeaders(token)
        });

        Assert.That((int)response.Status, Is.EqualTo(404).Or.EqualTo(400),
            $"Expected 404/400 for non-existent. Got {(int)response.Status}. Body: {await response.TextAsync()}");
    }
}
