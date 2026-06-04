using System.Text.Json;

namespace Personal_Finance_Management.Api.Tests.Tests;

/// <summary>
/// === HƯỚNG DẪN: Category Tests (User + Admin) ===
///
/// Category có 2 controller:
/// - CategoryController: [Authorize(Policy="User")] — chỉ User role được dùng
/// - AdminCategoryController: [Authorize(Policy="Admin")] — chỉ Admin role được dùng
///
/// Pattern cho role-based authorization test:
/// 1. Test User endpoint với User token → 200
/// 2. Test User endpoint với Admin token → 403
/// 3. Test Admin endpoint với Admin token → 200
/// 4. Test Admin endpoint với User token → 403
///
/// === LƯU Ý QUAN TRỌNG: Category Route ===
/// - User: POST /api/v1/categories (CreateCategoryRequest: Name, Icon?, Color?)
/// - Admin: POST /api/v1/admin/categories (CreateAdminCategoryRequest: Name, Icon?, Color?, Order)
///
/// CreateCategoryRequest cần Name (required), Icon + Color (optional).
/// </summary>
[TestFixture]
[NonParallelizable]
public class CategoryControllerTests : ApiTestBase
{
    // ─── User Category: Access Control ─────────────────────────

    [Test]
    public async Task GetCategories_WithoutToken_Returns401()
    {
        var response = await ApiContext.GetAsync("/api/v1/categories");
        await AssertUnauthorized(response, "Categories without token");
    }

    /// <summary>
    /// Test: User role gọi Category endpoint → 200 OK
    ///
    /// Mẫu: Role-based access — đúng role thì được truy cập.
    /// </summary>
    [Test]
    public async Task GetCategories_AsNormalUser_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var response = await ApiContext.GetAsync("/api/v1/categories", new()
        {
            Headers = AuthHeaders(token)
        });

        await AssertOk(response, "Categories as normal user");
    }

    /// <summary>
    /// Test: Admin role gọi User Category endpoint → 403 Forbidden
    ///
    /// Mẫu: Test role-based 403 — sai role thì bị chặn.
    /// </summary>
    [Test]
    public async Task GetCategories_AsAdmin_Returns403()
    {
        var token = await GetAdminTokenAsync();

        var response = await ApiContext.GetAsync("/api/v1/categories", new()
        {
            Headers = AuthHeaders(token)
        });

        // Admin không có User policy → expect 403
        Assert.That((int)response.Status, Is.EqualTo(403).Or.EqualTo(401),
            $"Expected 403 for admin accessing user-only endpoint. Got {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    // ─── User Category: CRUD ───────────────────────────────────

    [Test]
    public async Task CreateCategory_AsNormalUser_Returns201()
    {
        var token = await GetNormalUserTokenAsync();
        var uniqueName = UniqueName("Test Category");

        var response = await ApiContext.PostAsync("/api/v1/categories", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                name = uniqueName,
                icon = "shopping-cart",
                color = "#E91E63"
            }
        });

        await AssertStatus(response, 201, "Create category as normal user");
        var json = await response.JsonAsync<JsonElement>();
        var id = json.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
        if (id is not null) TrackId(id, "/api/v1/categories/{id}");
    }

    [Test]
    public async Task UpdateCategory_AsNormalUser_Returns200()
    {
        var token = await GetNormalUserTokenAsync();
        var uniqueName = UniqueName("Test Update Cat");

        var createResponse = await ApiContext.PostAsync("/api/v1/categories", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                name = uniqueName,
                icon = "star",
                color = "#3F51B5"
            }
        });
        await AssertStatus(createResponse, 201, "Create category for update");
        var createJson = await createResponse.JsonAsync<JsonElement>();
        var id = createJson.GetProperty("id").GetString()!;
        TrackId(id, "/api/v1/categories/{id}");

        var response = await ApiContext.PatchAsync($"/api/v1/categories/{id}", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new { name = uniqueName + " Updated" }
        });

        Assert.That(response.Ok, Is.True,
            $"Update category should succeed. Status: {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    [Test]
    public async Task DeleteCategory_AsNormalUser_Returns200()
    {
        var token = await GetNormalUserTokenAsync();
        var uniqueName = UniqueName("Test Delete Cat");

        var createResponse = await ApiContext.PostAsync("/api/v1/categories", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                name = uniqueName,
                icon = "delete",
                color = "#F44336"
            }
        });
        await AssertStatus(createResponse, 201, "Create category for delete");
        var createJson = await createResponse.JsonAsync<JsonElement>();
        var id = createJson.GetProperty("id").GetString()!;

        var response = await ApiContext.DeleteAsync($"/api/v1/categories/{id}", new()
        {
            Headers = AuthHeaders(token)
        });

        Assert.That(response.Ok, Is.True,
            $"Delete category should succeed. Status: {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    [Test]
    public async Task CreateCategory_AsAdmin_Returns403()
    {
        var token = await GetAdminTokenAsync();

        var response = await ApiContext.PostAsync("/api/v1/categories", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new { name = UniqueName("Cat By Admin"), icon = "x", color = "#000000" }
        });

        // Admin role không có User policy → expect 403
        Assert.That((int)response.Status, Is.EqualTo(403).Or.EqualTo(401),
            $"Expected 403 for admin creating user category. Got {(int)response.Status}. Body: {await response.TextAsync()}");
    }
}
