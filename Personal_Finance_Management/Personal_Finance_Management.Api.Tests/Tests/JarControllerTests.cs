using System.Text.Json;

namespace Personal_Finance_Management.Api.Tests.Tests;

/// <summary>
/// === HƯỚNG DẪN: Jar Tests ===
///
/// Jar có pattern CRUD như Goals/FinancialAccounts.
/// CreateJarRequest cần: name, color, icon.
/// </summary>
[TestFixture]
[NonParallelizable]
public class JarControllerTests : ApiTestBase
{
    [Test]
    public async Task GetJars_WithoutAuth_Returns401()
    {
        var response = await ApiContext.GetAsync("/api/v1/jars");
        await AssertUnauthorized(response, "Jars without token");
    }

    [Test]
    public async Task GetJars_AsNormalUser_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var response = await ApiContext.GetAsync("/api/v1/jars", new()
        {
            Headers = AuthHeaders(token)
        });

        await AssertOk(response, "Get jars");
    }

    [Test]
    public async Task CreateJar_WithValidData_Returns200()
    {
        var token = await GetNormalUserTokenAsync();
        var uniqueName = UniqueName("Test Jar");

        var response = await ApiContext.PostAsync("/api/v1/jars", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                name = uniqueName,
                color = "#FF5722",
                icon = "wallet"
            }
        });

        var json = await AssertOk(response, "Create jar");
        var id = json.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
        if (id is not null) TrackId(id, "/api/v1/jars/{id}");
    }

    [Test]
    public async Task UpdateJar_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var createResponse = await ApiContext.PostAsync("/api/v1/jars", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                name = UniqueName("Test Update Jar"),
                color = "#2196F3",
                icon = "bank"
            }
        });
        var createJson = await AssertOk(createResponse, "Create for update");
        var id = createJson.GetProperty("id").GetString()!;
        TrackId(id, "/api/v1/jars/{id}");

        var response = await ApiContext.PatchAsync($"/api/v1/jars/{id}", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                name = "Updated Jar Name",
                color = "#4CAF50"
            }
        });

        Assert.That(response.Ok, Is.True,
            $"Update should succeed. Status: {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    [Test]
    public async Task DeleteJar_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var createResponse = await ApiContext.PostAsync("/api/v1/jars", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                name = UniqueName("Test Delete Jar"),
                color = "#9C27B0",
                icon = "trash"
            }
        });
        var createJson = await AssertOk(createResponse, "Create for delete");
        var id = createJson.GetProperty("id").GetString()!;

        var response = await ApiContext.DeleteAsync($"/api/v1/jars/{id}", new()
        {
            Headers = AuthHeaders(token)
        });

        Assert.That(response.Ok, Is.True,
            $"Delete should succeed. Status: {(int)response.Status}. Body: {await response.TextAsync()}");
    }
}
