# Financial Accounts — SePay Connect

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/financial-accounts/sepay/connect` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` (ẩn trên Swagger) |

## Mục đích

Khởi tạo phiên làm việc (OAuth session) để liên kết với ngân hàng qua đối tác SePay, và trả về đường dẫn ủy quyền (authorization URL) để FE chuyển hướng người dùng sang trang cấp quyền của SePay.

## Request

```json
{
  "returnUrl": "string | null",
  "isDefault": "boolean | null",
  "autoSync": "boolean | null"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| returnUrl | string hay null | ❌ | Đường dẫn quay trở lại của Front-End sau khi liên kết xong |
| isDefault | boolean hay null | ❌ | Đặt tài khoản liên kết làm mặc định sau khi tạo |
| autoSync | boolean hay null | ❌ | Tự động đồng bộ các giao dịch phát sinh sau này |

## Response

```json
{
  "sessionId": "guid",
  "authorizationUrl": "string",
  "expiresAt": "datetimeOffset"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| sessionId | guid | Định danh của phiên kết nối |
| authorizationUrl | string | Đường dẫn ủy quyền từ SePay dùng để redirect |
| expiresAt | datetimeOffset | Thời điểm hết hạn của phiên kết nối này |

## Luồng xử lý

1. `FinancialAccountController` nhận yêu cầu và gọi `BankConnection.IService.StartSepayConnection`.
2. Service lấy thông tin người dùng hiện tại và xác thực tính hợp lệ của `returnUrl` theo cấu hình.
3. Tạo đối tượng `BankConnectionSession` với trạng thái ban đầu là `Pending`.
4. Tạo cấu trúc đường dẫn ủy quyền tương thích của SePay và phản hồi về cho FE.

## Quy tắc nghiệp vụ

- **Ownership**: Phiên kết nối ngân hàng được gán chính xác cho người dùng hiện tại.
- **Validation**: Đường dẫn trả về (`returnUrl`) phải thuộc danh sách tiền tố được phép (allowed prefix) trong cấu hình hệ thống.
- **Side effects**: Thêm mới một dòng dữ liệu vào bảng `bank_connection_sessions`.
- **Security**: Tuyệt đối không để lộ mã khóa khách hàng (client secret) hay các thông tin kết nối nhạy cảm khác ra ngoài.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực không hợp lệ |
| 422 | VALIDATION_FAILED | Đường dẫn `returnUrl` không hợp lệ hoặc thiếu cấu hình SePay từ hệ thống |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/FinancialAccountController.cs` |
| Service | `Personal_Finance_Management.Service/BankConnection/Service.cs` |
| DTO | `Personal_Finance_Management.Service/BankConnection/Request.cs`, `Response.cs` |
| Entity | `BankConnectionSession` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-FA-004 | Sử dụng SePay làm nhà cung cấp chính thức thay thế cho đối tác Casso trước đó | Đồng bộ toàn bộ logic và tài liệu liên quan sang SePay |

## Checklist

- [ ] Phiên kết nối khởi tạo thành công với trạng thái `Pending`.
- [ ] Đường dẫn ủy quyền được cấu hình đúng cấu trúc và tham số bảo mật.
- [ ] Không để lộ thông tin bảo mật của nhà cung cấp.
