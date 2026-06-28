# Financial Accounts — Create LinkApi Manual Mapping

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/financial-accounts/LinkApi` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` (ẩn khỏi Swagger) |

## Mục đích

Tạo tài khoản liên kết (linked account) bằng thông tin người dùng nhập trực tiếp. Luồng chính thống cho Front-End nên là sử dụng SePay OAuth qua route `sepay/connect`; do đó endpoint này chủ yếu dành cho mục đích mapping dữ liệu cũ (legacy-internal style).

## Request

```json
{
  "bankName": "string",
  "bankCode": "string | null",
  "accountNumber": "string",
  "accountHolderName": "string | null",
  "isDefault": "boolean"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| bankName | string | ✅ | Tên ngân hàng liên kết |
| bankCode | string hay null | ❌ | Mã ngân hàng |
| accountNumber | string | ✅ | Số tài khoản |
| accountHolderName | string hay null | ❌ | Tên chủ tài khoản |
| isDefault | boolean | ✅ | Có gán cờ mặc định hay không |

## Response

```json
{
  "id": "guid",
  "name": "string",
  "accountType": "string",
  "connectionMode": "string",
  "providerName": "string",
  "maskedAccountNumber": "string",
  "currentBalance": "decimal",
  "currency": "string",
  "syncStatus": "string",
  "isDefault": "boolean",
  "isActive": "boolean"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh duy nhất của nguồn tiền |
| connectionMode | string | Thể hiện là `LinkedApi` |
| providerName | string | Tên của nhà cung cấp dịch vụ đồng bộ |
| maskedAccountNumber | string | Số tài khoản đã được che đi một phần để bảo mật |

## Luồng xử lý

1. Controller hành động (action) hiện bị đánh dấu bỏ qua trên Swagger (`ApiExplorerSettings(IgnoreApi = true)`) nhưng vẫn có thể gọi được.
2. Service tạo một đối tượng nguồn tiền (`FinancialAccount`) với thuộc tính `ConnectionMode = LinkedApi`.
3. Nhà cung cấp đồng bộ mặc định (provider) được lấy theo cấu hình hiện tại (hiện là SePay).
4. Cấu trúc phản hồi chỉ trả về các thông tin an toàn (public data) hoặc đã được che giấu (masked).

## Quy tắc nghiệp vụ

- **Ownership**: Nguồn tiền tạo ra phải được gán vào quyền sở hữu của người dùng hiện tại.
- **Validation**: Số tài khoản và các thông tin ngân hàng phải hợp lệ.
- **Side effects**: Thêm mới một dòng dữ liệu vào bảng `financial_accounts` với dạng liên kết API.
- **Security**: Tuyệt đối không lưu trữ các thẻ thông tin nhạy cảm của nhà cung cấp (raw sensitive provider token) qua endpoint này trừ phi có đi qua luồng chuẩn OAuth.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 409 | RESOURCE_CONFLICT | Xung đột thiết lập cờ mặc định hoặc trùng lặp số tài khoản |
| 422 | VALIDATION_FAILED | Các thông tin về số tài khoản hoặc ngân hàng cung cấp bị sai định dạng |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/FinancialAccountController.cs` |
| Service | `Personal_Finance_Management.Service/FinancialAccount/Service.cs` |
| DTO | `Personal_Finance_Management.Service/FinancialAccount/Request.cs`, `Response.cs` |
| Entity | `FinancialAccount` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-008 | Trạng thái hiển thị công khai (visibility) đối với các endpoint bị ẩn | FE nên dùng luồng xác thực chuẩn SePay OAuth thay cho mapping thủ công này |

## Checklist

- [ ] Không làm lộ số tài khoản đầy đủ ra ngoài nếu cấu hình yêu cầu che giấu (masked).
- [ ] Tuân thủ chặt chẽ việc gắn cờ quyền sở hữu tài khoản cho đúng user hiện tại.
