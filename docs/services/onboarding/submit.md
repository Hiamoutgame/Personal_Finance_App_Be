# Onboarding — Submit

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/onboarding` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Lưu thông tin khảo sát đầu vào (onboarding) và tự động khởi tạo các tài nguyên mặc định cho user mới, bao gồm: hồ sơ onboarding, nguồn tiền mặc định, thiết lập hũ, các hũ chi tiêu mặc định và bật cờ hoàn thành onboarding.

## Request

```json
{
  "monthlyIncome": 10000000,
  "occupationType": "string",
  "financialGoalTypes": ["string"],
  "budgetMethodPreference": "string",
  "ageRange": "string",
  "spendingChallenges": ["string"]
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| monthlyIncome | int | ✅ | Thu nhập hàng tháng |
| occupationType | string | ✅ | Loại nghề nghiệp |
| financialGoalTypes | mảng string | ✅ | Danh sách các mục tiêu tài chính mong muốn |
| budgetMethodPreference | string | ✅ | Phương pháp phân bổ ngân sách ưu tiên |
| ageRange | string | ✅ | Nhóm tuổi |
| spendingChallenges | mảng string | ✅ | Những khó khăn trong việc quản lý chi tiêu |

## Response

```json
{
  "recommendedMethod": "string",
  "recommendedCategories": [
    {
      "name": "string",
      "icon": "string"
    }
  ],
  "recommendedJars": [
    {
      "name": "string"
    }
  ],
  "defaultFinancialAccount": {
    "name": "string",
    "accountType": "string"
  }
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| recommendedMethod | string | Phương pháp quản lý chi tiêu được gợi ý |
| recommendedCategories | mảng object | Danh sách các danh mục được hệ thống đề xuất |
| recommendedJars | mảng object | Danh sách các hũ được đề xuất thiết lập |
| defaultFinancialAccount | object | Thông tin nguồn tiền mặc định được tạo |

## Luồng xử lý

1. `OnboardingController.FillOnboarding` nhận yêu cầu và gọi đến `Onboarding.IService.CreateOnboarding`.
2. Dịch vụ phân tích lấy thông tin người dùng hiện tại (current user) từ token.
3. Kiểm tra tính hợp lệ của request và tiến hành tạo mới hoặc cập nhật `OnboardingProfile`.
4. Tạo nguồn tiền mặc định với kiểu `Cash` (Tiền mặt), thiết lập cơ cấu phân bổ hũ (`JarSetup`) và tạo các hũ mặc định (`default jars`) theo phương pháp đã chọn.
5. Cập nhật trạng thái `Account.IsOnboardingCompleted = true` và lưu toàn bộ thay đổi vào database.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ tạo dữ liệu mặc định cho user hiện tại thực hiện thao tác.
- **Validation**: Đảm bảo các trường như thu nhập, phương pháp, mục tiêu và khó khăn gửi lên là hợp lệ.
- **Side effects**: Thêm mới (insert) và cập nhật (update) đồng thời nhiều bảng: `onboarding_profiles`, `financial_accounts`, `jar_setups`, `jars`, và cờ trạng thái trong `accounts`.
- **Security**: Không nhận hoặc tin tưởng trường `userId` nếu có truyền từ body.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 422 | VALIDATION_FAILED | Phương pháp ngân sách không hợp lệ |
| 409 | RESOURCE_CONFLICT | Trùng lặp hoặc xung đột tài nguyên mặc định nếu user gọi onboarding nhiều lần |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/OnboardingController.cs` |
| Service | `Personal_Finance_Management.Service/Onboarding/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Onboarding/Request.cs`, `Response.cs` |
| Entity | `OnboardingProfile`, `FinancialAccount`, `JarSetup`, `Jar`, `Account` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-ONBOARDING-001 | Tính đồng nhất dữ liệu (idempotency) khi user submit lại luồng onboarding cần được kiểm toán (audit) thêm | Có thể bị tạo trùng lặp các nguồn tài nguyên mặc định nếu gọi liên tục |

## Checklist

- [ ] Người dùng hoàn tất quá trình onboarding và được tạo sẵn các tài nguyên mặc định.
- [ ] Không tự động phát sinh giao dịch/số tiền nào khi onboarding.
- [ ] Phản hồi không làm rò rỉ cấu trúc của thực thể nội bộ (internal entity) ra ngoài.
