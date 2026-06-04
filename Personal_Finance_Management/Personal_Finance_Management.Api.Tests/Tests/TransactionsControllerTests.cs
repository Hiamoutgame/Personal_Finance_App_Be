using System.Text.Json;

namespace Personal_Finance_Management.Api.Tests.Tests;

/// <summary>
/// === HƯỚNG DẪN: Transactions Tests (CRUD + prerequisite data) ===
///
/// Transaction là endpoint phức tạp nhất vì cần có FinancialAccount
/// trước khi tạo transaction. Pattern cho trường hợp này:
///
/// 1. Dùng [OneTimeSetUp] để tạo prerequisite data (FinancialAccount)
/// 2. Dùng [OneTimeTearDown] để cleanup prerequisite data
/// 3. Mỗi test tạo transaction riêng và cleanup qua TrackId()
///
/// TransactionRequest cần: financialAccountId, type ("Income"/"Expense"),
/// transactionsAmount, categoryId (optional), date.
/// </summary>
[TestFixture]
[NonParallelizable]
public class TransactionsControllerTests : ApiTestBase
{
    private string _prerequisiteAccountId = null!;

    public override async Task OneTimeSetUp()
    {
        await base.OneTimeSetUp();

        // Tạo 1 financial account dùng chung cho tất cả transaction test
        var token = await GetNormalUserTokenAsync();
        var uniqueName = UniqueName("Test Account For Tx");

        var response = await ApiContext.PostAsync("/api/v1/financial-accounts/Manual", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                name = uniqueName,
                accountType = "Cash",
                currentBalance = 10_000_000,
                currency = "VND",
                isDefault = false
            }
        });

        var json = await AssertOk(response, "Create prerequisite account");
        _prerequisiteAccountId = json.GetProperty("id").GetString()!;
    }

    public override async Task OneTimeTearDown()
    {
        // Xóa prerequisite account (sẽ cascade/handle bởi API)
        if (!string.IsNullOrEmpty(_prerequisiteAccountId))
        {
            var token = NormalUserToken ?? await GetNormalUserTokenAsync();
            try
            {
                await ApiContext.DeleteAsync($"/api/v1/financial-accounts/{_prerequisiteAccountId}", new()
                {
                    Headers = AuthHeaders(token)
                });
            }
            catch { /* best effort */ }
        }

        await base.OneTimeTearDown();
    }

    // ─── GET list ──────────────────────────────────────────────

    [Test]
    public async Task GetTransactions_WithoutAuth_Returns401()
    {
        var response = await ApiContext.GetAsync("/api/v1/transactions");
        await AssertUnauthorized(response, "Transactions without token");
    }

    [Test]
    public async Task GetTransactions_WithAuth_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var response = await ApiContext.GetAsync("/api/v1/transactions", new()
        {
            Headers = AuthHeaders(token)
        });

        await AssertOk(response, "Get transactions");
    }

    [Test]
    public async Task GetTransactions_WithPagination_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var response = await ApiContext.GetAsync("/api/v1/transactions?pageIndex=1&pageSize=5", new()
        {
            Headers = AuthHeaders(token)
        });

        var json = await AssertOk(response, "Get transactions with pagination");
        TestContext.WriteLine($"Response type: {json.ValueKind}");
    }

    // ─── Create ────────────────────────────────────────────────

    [Test]
    public async Task CreateTransaction_WithValidData_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var response = await ApiContext.PostAsync("/api/v1/transactions", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                financialAccountId = _prerequisiteAccountId,
                type = "Expense",
                transactionsAmount = 50_000m,
                note = UniqueName("Test Expense Tx"),
                date = DateTimeOffset.UtcNow
            }
        });

        var json = await AssertOk(response, "Create transaction");
        var id = json.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
        if (id is not null) TrackId(id, "/api/v1/transactions/{id}");
    }

    [Test]
    public async Task CreateTransaction_IncomeType_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var response = await ApiContext.PostAsync("/api/v1/transactions", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                financialAccountId = _prerequisiteAccountId,
                type = "Income",
                transactionsAmount = 100_000m,
                note = UniqueName("Test Income Tx"),
                date = DateTimeOffset.UtcNow
            }
        });

        var json = await AssertOk(response, "Create income transaction");
        var id = json.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
        if (id is not null) TrackId(id, "/api/v1/transactions/{id}");
    }

    [Test]
    public async Task CreateTransaction_WithMissingRequiredFields_Returns400()
    {
        var token = await GetNormalUserTokenAsync();

        // Thiếu type
        var response = await ApiContext.PostAsync("/api/v1/transactions", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                financialAccountId = _prerequisiteAccountId,
                transactionsAmount = 50_000m,
                note = "Missing type field",
                date = DateTimeOffset.UtcNow
            }
        });

        Assert.That((int)response.Status, Is.EqualTo(400),
            $"Expected 400 for missing type. Got {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    // ─── Get by ID ─────────────────────────────────────────────

    [Test]
    public async Task GetTransactionById_NonExistent_Returns404()
    {
        var token = await GetNormalUserTokenAsync();
        var fakeId = Guid.NewGuid();

        var response = await ApiContext.GetAsync($"/api/v1/transactions/{fakeId}", new()
        {
            Headers = AuthHeaders(token)
        });

        Assert.That((int)response.Status, Is.EqualTo(404),
            $"Expected 404 for non-existent. Got {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    // ─── Update ────────────────────────────────────────────────

    [Test]
    public async Task UpdateTransaction_WithValidData_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        // Tạo transaction trước
        var createResponse = await ApiContext.PostAsync("/api/v1/transactions", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                financialAccountId = _prerequisiteAccountId,
                type = "Expense",
                transactionsAmount = 30_000m,
                note = UniqueName("Test Update Tx"),
                date = DateTimeOffset.UtcNow
            }
        });
        var createJson = await AssertOk(createResponse, "Create for update");
        var id = createJson.GetProperty("id").GetString()!;
        TrackId(id, "/api/v1/transactions/{id}");

        // Update
        var response = await ApiContext.PatchAsync($"/api/v1/transactions/{id}", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                transactionsAmount = 45_000m,
                note = "Updated note"
            }
        });

        Assert.That(response.Ok, Is.True,
            $"Update should succeed. Status: {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    [Test]
    public async Task UpdateTransaction_NonExistent_Returns404()
    {
        var token = await GetNormalUserTokenAsync();
        var fakeId = Guid.NewGuid();

        var response = await ApiContext.PatchAsync($"/api/v1/transactions/{fakeId}", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new { transactionsAmount = 99_000m }
        });

        Assert.That((int)response.Status, Is.EqualTo(404),
            $"Expected 404 for non-existent. Got {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    // ─── Delete ────────────────────────────────────────────────

    [Test]
    public async Task DeleteTransaction_WithValidId_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        // Tạo transaction
        var createResponse = await ApiContext.PostAsync("/api/v1/transactions", new()
        {
            Headers = AuthHeaders(token),
            DataObject = new
            {
                financialAccountId = _prerequisiteAccountId,
                type = "Expense",
                transactionsAmount = 10_000m,
                note = UniqueName("Test Delete Tx"),
                date = DateTimeOffset.UtcNow
            }
        });
        var createJson = await AssertOk(createResponse, "Create for delete");
        var id = createJson.GetProperty("id").GetString()!;

        // Xóa
        var response = await ApiContext.DeleteAsync($"/api/v1/transactions/{id}", new()
        {
            Headers = AuthHeaders(token)
        });

        Assert.That(response.Ok, Is.True,
            $"Delete should succeed. Status: {(int)response.Status}. Body: {await response.TextAsync()}");
    }

    [Test]
    public async Task DeleteTransaction_NonExistent_Returns404()
    {
        var token = await GetNormalUserTokenAsync();
        var fakeId = Guid.NewGuid();

        var response = await ApiContext.DeleteAsync($"/api/v1/transactions/{fakeId}", new()
        {
            Headers = AuthHeaders(token)
        });

        Assert.That((int)response.Status, Is.EqualTo(404),
            $"Expected 404 for non-existent. Got {(int)response.Status}. Body: {await response.TextAsync()}");
    }
}
