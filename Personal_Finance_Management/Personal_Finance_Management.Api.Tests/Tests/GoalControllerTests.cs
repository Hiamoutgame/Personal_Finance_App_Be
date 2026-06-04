using System.Text.Json;

namespace Personal_Finance_Management.Api.Tests.Tests;

/// <summary>
/// === HƯỚNG DẪN: Goal Tests ===
///
/// Pattern y hệt FinancialAccount: CRUD + cleanup qua TrackId.
/// CreateGoalRequest cần: Title, TargetAmount, DueDate.
/// Optional: LinkedJarId, Note.
/// </summary>
[TestFixture]
[NonParallelizable]
public class GoalControllerTests : ApiTestBase
{
    [Test]
    public async Task GetGoals_WithoutAuth_Returns401()
    {
        var response = await ApiContext.GetAsync("/api/v1/goals");
        await AssertUnauthorized(response, "Goals without token");
    }

    [Test]
    public async Task GetGoals_AsNormalUser_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var response = await ApiContext.GetAsync("/api/v1/goals", new()
        {
            Headers = AuthHeaders(token)
        });

        await AssertOk(response, "Get goals");
    }

    [Test]
    public async Task CreateGoal_WithValidData_Returns201()
    {
        var token = await GetNormalUserTokenAsync();

        var response = await ApiContext.PostAsync("/api/v1/goals", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                title = UniqueName("Test Goal"),
                targetAmount = 50_000_000m,
                dueDate = DateTime.UtcNow.AddMonths(6).ToString("O"),
                note = "Test goal created by Playwright API test"
            }
        });

        var json = await AssertStatus(response, 201, "Create goal");

        var id = json.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
        if (id is not null) TrackId(id, "/api/v1/goals/{id}");
    }

    [Test]
    public async Task GetGoalById_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        // Tạo goal trước
        var createResponse = await ApiContext.PostAsync("/api/v1/goals", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                title = UniqueName("Test Get Goal"),
                targetAmount = 20_000_000m,
                dueDate = DateTime.UtcNow.AddMonths(3).ToString("O")
            }
        });
        var createJson = await AssertOk(createResponse, "Create for get-by-id"); // Note: CreateGoal returns 201 but AssertOk accepts 2xx
        // Actually: CreateGoal returns 201. Let's handle that.
        if ((int)createResponse.Status == 201)
        {
            createJson = await createResponse.JsonAsync<JsonElement>();
        }

        var id = createJson.GetProperty("id").GetString()!;
        TrackId(id, "/api/v1/goals/{id}");

        // GET by id
        var response = await ApiContext.GetAsync($"/api/v1/goals/{id}", new()
        {
            Headers = AuthHeaders(token)
        });

        await AssertOk(response, "Get goal by id");
    }

    [Test]
    public async Task UpdateGoal_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var createResponse = await ApiContext.PostAsync("/api/v1/goals", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                title = UniqueName("Test Update Goal"),
                targetAmount = 10_000_000m,
                dueDate = DateTime.UtcNow.AddYears(1).ToString("O")
            }
        });
        var createJson = createResponse.Ok
            ? await createResponse.JsonAsync<JsonElement>()
            : await AssertStatus(createResponse, 201, "Create for update");

        var id = createJson.GetProperty("id").GetString()!;
        TrackId(id, "/api/v1/goals/{id}");

        var response = await ApiContext.PatchAsync($"/api/v1/goals/{id}", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                title = "Updated Goal Title",
                targetAmount = 15_000_000m
            }
        });

        Assert.That(response.Ok, Is.True,
            $"Update should succeed. Status: {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    [Test]
    public async Task DeleteGoal_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var createResponse = await ApiContext.PostAsync("/api/v1/goals", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                title = UniqueName("Test Delete Goal"),
                targetAmount = 5_000_000m,
                dueDate = DateTime.UtcNow.AddMonths(1).ToString("O")
            }
        });
        var createJson = createResponse.Ok
            ? await createResponse.JsonAsync<JsonElement>()
            : await AssertStatus(createResponse, 201, "Create for delete");

        var id = createJson.GetProperty("id").GetString()!;

        var response = await ApiContext.DeleteAsync($"/api/v1/goals/{id}", new()
        {
            Headers = AuthHeaders(token)
        });

        Assert.That(response.Ok, Is.True,
            $"Delete should succeed. Status: {(int)response.Status}. Body: {await response.TextAsync()}");
    }
}
