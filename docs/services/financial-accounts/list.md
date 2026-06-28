# Financial Accounts — List

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /api/v1/financial-accounts` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Trả về danh sách tất cả các nguồn tiền người dùng hiện tại đang theo dõi trong FinJar. Đây là hệ thống theo dõi phi lưu ký (non-custodial), hệ thống chỉ ghi chép thông tin và không lưu giữ tiền thật của người dùng.

## Request

*Không yêu cầu Request Body*

## Response

```json
{
  "data": [
    {
      "id": "guid",
      "name": "string",
      "accountType": "string",
      "connectionMode": "string",
      "providerName": "string | null",
      "maskedAccountNumber": "string | null",
      "currency": "string",
      "currentBalance": "decimal",
      "syncStatus": "string",
      "isDefault": "boolean",
      "isActive": "boolean"
    }
  ]
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| data | mảng object | Danh sách các nguồn tiền của người dùng |
| id | guid | Định danh nguồn tiền |
| name | string | Tên hiển thị |
| accountType | string | Loại nguồn tiền (ví dụ: Cash, Bank, EWallet) |
| connectionMode | string | Chế độ liên kết (`Manual` hoặc `LinkedApi`) |
| providerName | string hay null | Tên của nhà cung cấp dịch vụ liên kết |
| maskedAccountNumber | string hay null | Số tài khoản ngân hàng đã ẩn bớt ký tự |
| currency | string | Đơn vị tiền tệ (ví dụ: VND) |
| currentBalance | decimal | Số dư hiện có |
| syncStatus | string | Trạng thái đồng bộ (ví dụ: Synced) |
| isDefault | boolean | Đánh dấu là nguồn tiền mặc định |
| isActive | boolean | Trạng thái hoạt động |

## Luồng xử lý

1. `FinancialAccountController.GetFinancialAccount` tiếp nhận yêu cầu và gọi đến `FinancialAccount.IService.GetUserFinancialAccount`.
2. Dịch vụ phân tích lấy thông tin ID người dùng hiện tại từ token.
3. Thực hiện truy vấn danh sách nguồn tiền tương ứng của người dùng từ cơ sở dữ liệu.
4. Ánh xạ thông tin sang DTO trả về, loại bỏ các trường thông tin nhạy cảm của nhà cung cấp (API token, client secrets).

## Quy tắc nghiệp vụ

- **Ownership**: Bắt buộc phải lọc danh sách theo đúng ID của người dùng hiện tại.
- **Validation**: Đảm bảo token truy cập hợp lệ.
- **Side effects**: Không tác động làm thay đổi dữ liệu trong cơ sở dữ liệu.
- **Security**: Không bao giờ để lộ các mã token gốc (`AccessTokenRef`) hoặc client secrets của liên kết trong phản hồi.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/FinancialAccountController.cs` |
| Service | `Personal_Finance_Management.Service/FinancialAccount/Service.cs` |
| DTO | `Personal_Finance_Management.Service/FinancialAccount/Response.cs` |
| Entity | `FinancialAccount`, `BankConnectionSession` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-FA-001 | Tính năng phân trang (pagination) hiện không được áp dụng cho danh sách nguồn tiền này | Trả về toàn bộ danh sách trong một yêu cầu |

## Checklist

- [ ] Chỉ hiển thị các nguồn tiền thuộc sở hữu của người dùng hiện tại.
- [ ] Bảo mật thông tin các token liên kết, không trả về trong phản hồi.
- [ ] Phân biệt rõ ràng giữa hai loại nguồn tiền thủ công (Manual) và liên kết API (LinkedApi).
