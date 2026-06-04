using System.Text.Json;

namespace Personal_Finance_Management.Api.Tests.Tests;

/// <summary>
/// === HƯỚNG DẪN: Reminder Tests ===
///
/// Reminder CRUD pattern. CreateReminderRequest cần:
/// Title, Amount, Frequency ("Monthly"/"Weekly"/"Yearly"), StartDate.
/// Optional: DayOfMonth, CategoryId, NotifyDaysBefore, Note.
/// </summary>
[TestFixture]
[NonParallelizable]
public class ReminderControllerTests : ApiTestBase
{
    [Test]
    public async Task GetReminders_WithoutAuth_Returns401()
    {
        var response = await ApiContext.GetAsync("/api/v1/reminders");
        await AssertUnauthorized(response, "Reminders without token");
    }

    [Test]
    public async Task GetReminders_AsNormalUser_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var response = await ApiContext.GetAsync("/api/v1/reminders", new()
        {
            Headers = AuthHeaders(token)
        });

        await AssertOk(response, "Get reminders");
    }

    [Test]
    public async Task CreateReminder_WithValidData_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var response = await ApiContext.PostAsync("/api/v1/reminders", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                title = UniqueName("Test Reminder"),
                amount = 500_000m,
                frequency = "Monthly",
                dayOfMonth = (short)15,
                startDate = DateTimeOffset.UtcNow.AddDays(7),
                notifyDaysBefore = (short)3,
                note = "Test reminder for Playwright API testing"
            }
        });

        var json = await AssertOk(response, "Create reminder");
        var id = json.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
        if (id is not null) TrackId(id, "/api/v1/reminders/{id}");
    }

    [Test]
    public async Task CreateReminder_WithoutFrequency_Returns400()
    {
        var token = await GetNormalUserTokenAsync();

        var response = await ApiContext.PostAsync("/api/v1/reminders", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                title = UniqueName("Bad Reminder"),
                amount = 100_000m,
                // Thiếu frequency
                startDate = DateTimeOffset.UtcNow
            }
        });

        Assert.That((int)response.Status, Is.EqualTo(400),
            $"Expected 400 for missing frequency. Got {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    [Test]
    public async Task UpdateReminder_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var createResponse = await ApiContext.PostAsync("/api/v1/reminders", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                title = UniqueName("Test Update Reminder"),
                amount = 300_000m,
                frequency = "Monthly",
                startDate = DateTimeOffset.UtcNow.AddDays(14)
            }
        });
        var createJson = await AssertOk(createResponse, "Create for update");
        var id = createJson.GetProperty("id").GetString()!;
        TrackId(id, "/api/v1/reminders/{id}");

        var response = await ApiContext.PatchAsync($"/api/v1/reminders/{id}", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                title = "Updated Reminder Title",
                amount = 450_000m
            }
        });

        Assert.That(response.Ok, Is.True,
            $"Update should succeed. Status: {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    [Test]
    public async Task DeleteReminder_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var createResponse = await ApiContext.PostAsync("/api/v1/reminders", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                title = UniqueName("Test Delete Reminder"),
                amount = 200_000m,
                frequency = "Weekly",
                startDate = DateTimeOffset.UtcNow
            }
        });
        var createJson = await AssertOk(createResponse, "Create for delete");
        var id = createJson.GetProperty("id").GetString()!;

        var response = await ApiContext.DeleteAsync($"/api/v1/reminders/{id}", new()
        {
            Headers = AuthHeaders(token)
        });

        Assert.That(response.Ok, Is.True,
            $"Delete should succeed. Status: {(int)response.Status}. Body: {await response.TextAsync()}");
    }
}
