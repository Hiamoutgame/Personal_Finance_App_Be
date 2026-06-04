using System.Text.Json;

namespace Personal_Finance_Management.Api.Tests.Tests;

/// <summary>
/// === HƯỚNG DẪN: Admin Category Tests ===
///
/// Admin endpoint dùng [Authorize(Policy="Admin")], chỉ Admin role truy cập được.
///
/// Pattern viết test cho Admin-only endpoint:
/// 1. Test với Admin token → 200
/// 2. Test với User token → 403 (role-based forbidden)
/// 3. CRUD tương tự như endpoint thường, nhưng dùng Admin token
///
/// CreateAdminCategoryRequest cần: Name, Order.
/// Optional: Icon, Color.
/// </summary>
[TestFixture]
[NonParallelizable]
public class AdminCategoryControllerTests : ApiTestBase
{
    // ─── Access control ────────────────────────────────────────

    /// <summary>
    /// Test: Admin role gọi Admin endpoint → 200 OK
    /// </summary>
    [Test]
    public async Task GetAdminCategories_AsAdmin_Returns200()
    {
        var token = await GetAdminTokenAsync();

        var response = await ApiContext.GetAsync("/api/v1/admin/categories", new()
        {
            Headers = AuthHeaders(token)
        });

        await AssertOk(response, "Admin categories as admin");
    }

    /// <summary>
    /// Test: Normal user gọi Admin endpoint → 403 Forbidden
    ///
    /// Mẫu: Kiểm tra role-based authorization — user thường không được phép.
    /// </summary>
    [Test]
    public async Task GetAdminCategories_AsNormalUser_Returns403()
    {
        var token = await GetNormalUserTokenAsync();

        var response = await ApiContext.GetAsync("/api/v1/admin/categories", new()
        {
            Headers = AuthHeaders(token)
        });

        // User không có Admin policy → expect 403
        Assert.That((int)response.Status, Is.EqualTo(403),
            $"Expected 403 for user accessing admin endpoint. Got {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    // ─── CRUD with Admin ───────────────────────────────────────

    [Test]
    public async Task CreateAdminCategory_AsAdmin_Returns201()
    {
        var token = await GetAdminTokenAsync();
        var uniqueName = UniqueName("Test Admin Cat");

        var response = await ApiContext.PostAsync("/api/v1/admin/categories", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                name = uniqueName,
                icon = "admin-star",
                color = "#FFD700",
                order = 99
            }
        });

        await AssertStatus(response, 201, "Create admin category");
        var json = await response.JsonAsync<JsonElement>();
        var id = json.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
        if (id is not null) TrackId(id, "/api/v1/admin/categories/{id}");
    }

    [Test]
    public async Task CreateAdminCategory_AsNormalUser_Returns403()
    {
        var token = await GetNormalUserTokenAsync();

        var response = await ApiContext.PostAsync("/api/v1/admin/categories", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                name = UniqueName("Hacked Cat"),
                icon = "skull",
                color = "#FF0000",
                order = 1
            }
        });

        Assert.That((int)response.Status, Is.EqualTo(403),
            $"Expected 403 for user creating admin category. Got {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    [Test]
    public async Task UpdateAdminCategory_AsAdmin_Returns200()
    {
        var token = await GetAdminTokenAsync();
        var uniqueName = UniqueName("Test Update Admin Cat");

        var createResponse = await ApiContext.PostAsync("/api/v1/admin/categories", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                name = uniqueName,
                icon = "edit",
                color = "#00BCD4",
                order = 50
            }
        });
        await AssertStatus(createResponse, 201, "Create admin cat for update");
        var createJson = await createResponse.JsonAsync<JsonElement>();
        var id = createJson.GetProperty("id").GetString()!;
        TrackId(id, "/api/v1/admin/categories/{id}");

        var response = await ApiContext.PatchAsync($"/api/v1/admin/categories/{id}", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                name = uniqueName + " Updated",
                order = 55
            }
        });

        Assert.That(response.Ok, Is.True,
            $"Update admin category should succeed. Status: {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    [Test]
    public async Task DeleteAdminCategory_AsAdmin_Returns200()
    {
        var token = await GetAdminTokenAsync();
        var uniqueName = UniqueName("Test Delete Admin Cat");

        var createResponse = await ApiContext.PostAsync("/api/v1/admin/categories", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                name = uniqueName,
                icon = "trash",
                color = "#607D8B",
                order = 999
            }
        });
        await AssertStatus(createResponse, 201, "Create admin cat for delete");
        var createJson = await createResponse.JsonAsync<JsonElement>();
        var id = createJson.GetProperty("id").GetString()!;

        var response = await ApiContext.DeleteAsync($"/api/v1/admin/categories/{id}", new()
        {
            Headers = AuthHeaders(token)
        });

        Assert.That(response.Ok, Is.True,
            $"Delete admin category should succeed. Status: {(int)response.Status}. Body: {await response.TextAsync()}");
    }
}
