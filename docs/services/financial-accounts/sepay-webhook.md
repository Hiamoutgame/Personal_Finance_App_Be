# Financial Accounts — SePay Webhook

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/financial-accounts/sepay/webhook` |
| Auth | Public (Anonymous) + Header `Authorization: Apikey ...` |
| Status thành công | `200 OK` hoặc `201 Created` với cờ `success = true` |
| Status hiện tại (code) | `200 OK` (ẩn trên Swagger) |

## Mục đích

Nhận sự kiện biến động số dư (giao dịch thời gian thực) gửi từ hệ thống đối tác SePay, tự động tạo giao dịch tương ứng và loại bỏ trùng lặp.

## Request

*Yêu cầu xác thực qua Header và truyền dữ liệu trong Request Body*

### Headers

```
Authorization: Apikey <Sepay:WebhookApiKey>
```

### Request Body

```json
{
  "id": 123456789,
  "gateway": "string",
  "transactionDate": "2026-06-28 13:00:00",
  "accountNumber": "string",
  "subAccount": "string | null",
  "code": "string | null",
  "content": "string | null",
  "transferType": "in | out",
  "description": "string | null",
  "transferAmount": 150000,
  "accumulated": 10500000,
  "referenceCode": "string | null"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| id | long | ✅ | ID giao dịch từ hệ thống SePay |
| gateway | string | ✅ | Cổng ngân hàng xử lý |
| transactionDate | string | ✅ | Thời gian giao dịch (định dạng: yyyy-MM-dd HH:mm:ss) |
| accountNumber | string | ✅ | Số tài khoản nhận biến động |
| transferType | string | ✅ | Loại biến động (`in` - tiền vào, `out` - tiền ra) |
| transferAmount | decimal | ✅ | Số tiền biến động (luôn dương) |

## Response

```json
{
  "success": true,
  "receivedCount": 1,
  "createdCount": 1,
  "skippedCount": 0,
  "message": "string"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| success | boolean | Trạng thái xử lý sự kiện |
| receivedCount | int | Số lượng sự kiện nhận được |
| createdCount | int | Số giao dịch mới được tạo thành công |
| skippedCount | int | Số sự kiện bị bỏ qua |

## Luồng xử lý

1. Controller tiếp nhận yêu cầu từ SePay, đọc thông tin xác thực từ header `Authorization` và gọi `ProcessSepayWebhook`.
2. Dịch vụ tiến hành xác thực tính chính xác của khóa Webhook API Key.
3. Ánh xạ thuộc tính `transferType` tương ứng thành loại giao dịch của hệ thống (`in` -> `Income`, `out` -> `Expense`).
4. Tìm kiếm tài khoản liên kết (`FinancialAccount`) tương ứng với số tài khoản nhận biến động, lọc trùng lặp dựa trên trường `external transaction id`.
5. Tạo giao dịch mới, điều chỉnh lại số dư tài khoản/hũ và cập nhật thông tin hiển thị trên Dashboard.

## Quy tắc nghiệp vụ

- **Ownership**: Xác định tài khoản và quyền sở hữu dựa trên dữ liệu tài khoản gửi từ SePay, không sử dụng mã xác thực JWT của người dùng.
- **Validation**: Kiểm tra tính hợp lệ của Webhook API Key, thông tin số tiền và đảm bảo số tài khoản ngân hàng tồn tại trong hệ thống.
- **Side effects**: Thêm mới giao dịch, lọc trùng lặp và cập nhật lại số dư tài khoản.
- **Security**: Tuyệt đối không lưu nhật ký (log) các thông tin nhạy cảm của Header chứa khóa bảo mật.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Khóa Webhook API Key không khớp hoặc bị thiếu |
| 404 | NOT_FOUND | Không tìm thấy tài khoản ngân hàng tương ứng với số tài khoản nhận từ SePay |
| 422 | VALIDATION_FAILED | Dữ liệu sự kiện gửi từ SePay bị sai cấu trúc hoặc thiếu các trường bắt buộc |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/FinancialAccountController.cs` |
| Service | `Personal_Finance_Management.Service/BankSync/Service.cs` |
| DTO | `Personal_Finance_Management.Service/BankSync/Request.cs`, `Response.cs` |
| Entity | `FinancialAccount`, `Transaction` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-FA-006 | Các thông tin cảnh báo bảo mật về việc lộ lọt webhook key cần được kiểm tra kỹ lưỡng trong tệp cấu hình | Đảm bảo tính an toàn cho luồng tự động |

## Checklist

- [ ] Xác thực chính xác khóa Webhook API Key từ SePay gửi sang.
- [ ] Sự kiện trùng lặp (duplicate event) được lọc và bỏ qua chính xác.
- [ ] Ánh xạ đúng tính chất biến động tiền ra/tiền vào.
