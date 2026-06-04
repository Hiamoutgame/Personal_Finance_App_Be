using System.Text.Json;

namespace Personal_Finance_Management.Api.Tests.Tests;

/// <summary>
/// === HƯỚNG DẪN: Dashboard Tests (Read-only endpoint) ===
///
/// Dashboard là endpoint GET đơn giản nhất vì chỉ cần token và không tạo/xóa dữ liệu.
/// Đây là pattern để viết test cho mọi GET endpoint:
///
/// 1. Test không có token → 401
/// 2. Test có token → 200
/// 3. Kiểm tra shape của JSON response (các field có tồn tại)
/// </summary>
[TestFixture]
[NonParallelizable]
public class DashboardControllerTests : ApiTestBase
{
    /// <summary>
    /// Test: GET /api/v1/dashboard không gửi token → 401
    ///
    /// Mẫu: Mọi [Authorize] endpoint đều phải test trường hợp thiếu token.
    /// </summary>
    [Test]
    public async Task GetDashboard_WithoutAuth_Returns401()
    {
        // Gọi API không kèm Authorization header
        var response = await ApiContext.GetAsync("/api/v1/dashboard");

        await AssertUnauthorized(response, "Dashboard without token");
    }

    /// <summary>
    /// Test: GET /api/v1/dashboard với token hợp lệ → 200 + JSON có cấu trúc đúng
    ///
    /// Mẫu: Gọi API có token, kiểm tra các field quan trọng trong response.
    /// </summary>
    [Test]
    public async Task GetDashboard_AsNormalUser_Returns200()
    {
        var token = await GetNormalUserTokenAsync();

        var response = await ApiContext.GetAsync("/api/v1/dashboard", new()
        {
            Headers = AuthHeaders(token)
        });

        var json = await AssertOk(response, "Dashboard");

        // Kiểm tra response có các section chính
        // Tùy API, có thể có balanceSummary, financialAccounts, ...
        Assert.That(json.ValueKind, Is.EqualTo(JsonValueKind.Object),
            "Dashboard response should be a JSON object");

        // Ghi log cấu trúc response để tham khảo
        TestContext.WriteLine($"Dashboard response keys: {string.Join(", ", json.EnumerateObject().Select(p => p.Name))}");
    }

    /// <summary>
    /// Test: Kiểm tra response dashboard có thể parse thành công.
    ///
    /// Mẫu: Kiểm tra JSON structure của response.
    /// </summary>
    [Test]
    public async Task GetDashboard_ResponseIsValidJson()
    {
        var token = await GetNormalUserTokenAsync();

        var response = await ApiContext.GetAsync("/api/v1/dashboard", new()
        {
            Headers = AuthHeaders(token)
        });

        var json = await AssertOk(response, "Dashboard valid JSON");

        // Verify có thể đọc các field (nếu API trả về các field này)
        var propertyNames = json.EnumerateObject().Select(p => p.Name).ToList();
        Assert.That(propertyNames, Is.Not.Empty, "Dashboard response must not be empty");

        // Ghi ra để debug
        TestContext.WriteLine($"Dashboard properties: {string.Join(", ", propertyNames)}");
    }
}
