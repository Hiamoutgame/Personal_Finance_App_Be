using System.Text.Json;

namespace Personal_Finance_Management.Api.Tests.Tests;

/// <summary>
/// === HƯỚNG DẪN: Limit Tests ===
///
/// Limit (spending limit) cần targetType + targetId (track 1 resource như jar).
/// CreateLimitRequest cần: TargetType, TargetId, LimitAmount, Period, AlertAtPercentage.
///
/// Pattern: Vì limit cần TargetId (ví dụ id của 1 jar), ta tạo 1 jar
/// trong [OneTimeSetUp] làm prerequisite, rồi dùng jarId đó cho các test limit.
/// </summary>
[TestFixture]
[NonParallelizable]
public class LimitControllerTests : ApiTestBase
{
    private string _prerequisiteJarId = null!;

    public override async Task OneTimeSetUp()
    {
        await base.OneTimeSetUp();

        // Tạo 1 jar để làm target cho limit
        var token = await GetNormalUserTokenAsync();
        var uniqueName = UniqueName("Test Limit Jar");

        var response = await ApiContext.PostAsync("/api/v1/jars", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new { name = uniqueName, color = "#FF9800", icon = "target" }
        });

        var json = await AssertOk(response, "Create prerequisite jar for limit");
        _prerequisiteJarId = json.GetProperty("id").GetString()!;
    }

    public override async Task OneTimeTearDown()
    {
        if (!string.IsNullOrEmpty(_prerequisiteJarId) && NormalUserToken is not null)
        {
            try
            {
                await ApiContext.DeleteAsync($"/api/v1/jars/{_prerequisiteJarId}", new()
                {
                    Headers = AuthHeaders(NormalUserToken)
                });
            }
            catch { /* best effort */ }
        }
        await base.OneTimeTearDown();
    }

    [Test]
    public async Task GetLimits_WithoutAuth_Returns401()
    {
        var response = await ApiContext.GetAsync("/api/v1/limits");
        await AssertUnauthorized(response, "Limits without token");
    }

    [Test]
    public async Task GetLimits_AsNormalUser_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var response = await ApiContext.GetAsync("/api/v1/limits", new()
        {
            Headers = AuthHeaders(token)
        });

        await AssertOk(response, "Get limits");
    }

    [Test]
    public async Task CreateLimit_WithValidData_Returns201()
    {
        var token = await GetNormalUserTokenAsync();

        var response = await ApiContext.PostAsync("/api/v1/limits", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                targetType = "Jar",
                targetId = _prerequisiteJarId,
                limitAmount = 5_000_000m,
                period = "Monthly",
                alertAtPercentage = 80m
            }
        });

        var json = await AssertStatus(response, 201, "Create limit");

        var id = json.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
        if (id is not null) TrackId(id, "/api/v1/limits/{id}");
    }

    [Test]
    public async Task CreateLimit_WithoutTargetId_Returns400()
    {
        var token = await GetNormalUserTokenAsync();

        var response = await ApiContext.PostAsync("/api/v1/limits", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                targetType = "Jar",
                // Thiếu targetId
                limitAmount = 1_000_000m,
                period = "Monthly",
                alertAtPercentage = 50m
            }
        });

        Assert.That((int)response.Status, Is.EqualTo(400),
            $"Expected 400 for missing targetId. Got {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    [Test]
    public async Task UpdateLimit_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var createResponse = await ApiContext.PostAsync("/api/v1/limits", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                targetType = "Jar",
                targetId = _prerequisiteJarId,
                limitAmount = 3_000_000m,
                period = "Weekly",
                alertAtPercentage = 70m
            }
        });
        Assert.That(createResponse.Ok || (int)createResponse.Status == 201);
        var createJson = await createResponse.JsonAsync<JsonElement>();
        var id = createJson.GetProperty("id").GetString()!;
        TrackId(id, "/api/v1/limits/{id}");

        var response = await ApiContext.PatchAsync($"/api/v1/limits/{id}", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                limitAmount = 4_500_000m,
                alertAtPercentage = 90m
            }
        });

        Assert.That(response.Ok, Is.True,
            $"Update should succeed. Status: {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    [Test]
    public async Task DeleteLimit_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var createResponse = await ApiContext.PostAsync("/api/v1/limits", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                targetType = "Jar",
                targetId = _prerequisiteJarId,
                limitAmount = 2_000_000m,
                period = "Monthly",
                alertAtPercentage = 60m
            }
        });
        Assert.That(createResponse.Ok || (int)createResponse.Status == 201);
        var createJson = await createResponse.JsonAsync<JsonElement>();
        var id = createJson.GetProperty("id").GetString()!;

        var response = await ApiContext.DeleteAsync($"/api/v1/limits/{id}", new()
        {
            Headers = AuthHeaders(token)
        });

        Assert.That(response.Ok, Is.True,
            $"Delete should succeed. Status: {(int)response.Status}. Body: {await response.TextAsync()}");
    }
}
