# User — Setup Gate

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /api/v1/user/me/setup` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Trả về thông tin chi tiết về trạng thái hoàn thành onboarding và số lượng các thực thể hiện có của người dùng, giúp FE thực hiện vai trò cổng điều hướng (route guard) tương ứng cho người dùng vào onboarding hoặc dashboard.

## Request

*Không yêu cầu Request Body*

## Response

```json
{
  "isOnboardingCompleted": "boolean",
  "monthlyIncome": "decimal | null",
  "budgetMethod": "string",
  "defaultFinancialAccountId": "guid | null",
  "jarCount": "int",
  "financialAccountCount": "int",
  "limitCount": "int",
  "activeGoalCount": "int"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| isOnboardingCompleted | boolean | Trạng thái đã hoàn thành khảo sát onboarding |
| monthlyIncome | decimal hay null | Thu nhập hàng tháng được ghi nhận lúc onboarding |
| budgetMethod | string | Phương pháp phân bổ ngân sách đã chọn |
| defaultFinancialAccountId | guid hay null | ID nguồn tiền mặc định hiện tại |
| jarCount | int | Số lượng hũ chi tiêu hiện có của người dùng |
| financialAccountCount | int | Số lượng nguồn tiền hiện tại của người dùng |
| limitCount | int | Số lượng hạn mức chi tiêu được thiết lập |
| activeGoalCount | int | Số lượng mục tiêu tài chính đang hoạt động |

## Luồng xử lý

1. `UserController.ViewSetup` tiếp nhận yêu cầu và chuyển tiếp đến `User.IService.ViewSetup`.
2. Dịch vụ phân tích thông tin ID người dùng từ JWT.
3. Thực hiện truy vấn hồ sơ onboarding, tìm nguồn tiền mặc định và đếm số lượng các thực thể tương ứng (hũ, hạn mức, mục tiêu, nguồn tiền) thuộc về người dùng này.
4. Tổng hợp các thông tin trên và phản hồi lại về phía client.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ thực hiện truy vấn và đếm các thực thể thuộc quyền sở hữu của chính người dùng hiện tại (lọc theo UserId).
- **Validation**: Đảm bảo token cung cấp chứa ID người dùng hợp lệ.
- **Side effects**: Không tác động làm thay đổi dữ liệu trong cơ sở dữ liệu.
- **Security**: Không nhận thông tin UserId trực tiếp từ query string hay request body để tránh lỗ hổng bảo mật.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Người dùng thiếu token xác thực hoặc token không hợp lệ |
| 404 | NOT_FOUND | Tài khoản của người dùng không tồn tại trong hệ thống |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/UserController.cs` |
| Service | `Personal_Finance_Management.Service/User/Service.cs` |
| DTO | `Personal_Finance_Management.Service/User/Response.cs` |
| Entity | `Account`, `OnboardingProfile`, `FinancialAccount`, `Jar`, `SpendingLimit`, `Goal` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-USER-003 | Ý nghĩa các bộ đếm (counts logic) có thể cần được đảm bảo giữ đồng bộ với màn hình onboarding/dashboard | Có thể sai lệch nhẹ số liệu nếu logic truy vấn đếm khác nhau |

## Checklist

- [ ] Trả về trạng thái `isOnboardingCompleted = false` cho tài khoản chưa hoàn tất onboarding.
- [ ] Các bộ đếm chính xác theo đúng quyền sở hữu của người dùng.
- [ ] FE nhận đủ thông tin để quyết định luồng chuyển hướng.
