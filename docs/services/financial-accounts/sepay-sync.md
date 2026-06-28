# Financial Accounts — SePay Sync

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/financial-accounts/{id}/sync` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` (ẩn trên Swagger) |

## Mục đích

Đồng bộ thủ công các giao dịch tài chính phát sinh từ hệ thống ngân hàng đối tác SePay về cho tài khoản liên kết (`LinkedApi`) cụ thể của người dùng.

## Request

```json
{
  "fromDate": "date | null",
  "toDate": "date | null",
  "page": "int | null",
  "pageSize": "int | null",
  "sort": "ASC | DESC | null",
  "triggerProviderSync": "boolean | null"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| fromDate | date hay null | ❌ | Bắt đầu đồng bộ từ ngày |
| toDate | date hay null | ❌ | Đồng bộ đến ngày |
| page | int hay null | ❌ | Chỉ số trang dữ liệu muốn đồng bộ từ SePay |
| pageSize | int hay null | ❌ | Số lượng bản ghi trên một trang đồng bộ |
| sort | string hay null | ❌ | Hướng sắp xếp dữ liệu trả về (`ASC` hoặc `DESC`) |
| triggerProviderSync | boolean hay null | ❌ | Bắt buộc hệ thống tải mới dữ liệu từ nhà cung cấp |

## Response

```json
{
  "receivedCount": 15,
  "createdCount": 5,
  "skippedCount": 10,
  "message": "string"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| receivedCount | int | Tổng số giao dịch nhận được từ SePay |
| createdCount | int | Số giao dịch mới được tạo thành công trong hệ thống |
| skippedCount | int | Số giao dịch bị bỏ qua (do trùng lặp hoặc đã tồn tại) |
| message | string | Tin nhắn mô tả chi tiết |

## Luồng xử lý

1. `FinancialAccountController` tiếp nhận yêu cầu và gọi `BankSync.IService.SyncLinkedAccount(id, request)`.
2. Service thực hiện tải thông tin nguồn tiền và xác thực quyền sở hữu với `ConnectionMode = LinkedApi`.
3. Lấy thông tin mã xác thực/API Key của đối tác SePay, tiến hành làm mới token (refresh token) nếu cần.
4. Lấy danh sách giao dịch tương ứng từ SePay và tiến hành lọc trùng lặp dựa trên mã định danh giao dịch (`external transaction id`).
5. Tạo mới các giao dịch hợp lệ, điều chỉnh số dư tài khoản/hũ và cập nhật trạng thái đồng bộ của tài khoản.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ thực hiện đồng bộ đối với tài khoản thuộc về người dùng hiện tại đang đăng nhập.
- **Validation**: Tài khoản liên kết phải đang hoạt động (`isActive = true`), khoảng thời gian đồng bộ phải hợp lệ.
- **Side effects**: Thêm các giao dịch mới vào bảng `transactions`, cập nhật lại số dư và lịch sử đồng bộ trên bảng `financial_accounts`.
- **Security**: Không để lộ thông tin cấu hình token của đối tác ngân hàng trong bất kỳ chuỗi phản hồi nào.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực không hợp lệ |
| 404 | NOT_FOUND | Tài khoản không tồn tại hoặc không thuộc quyền sở hữu của người dùng hiện tại |
| 502 | BAD_GATEWAY | Gặp lỗi khi gọi sang API hoặc làm mới token của đối tác SePay |

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
| DRIFT-FA-005 | Cơ chế lọc trùng lặp giao dịch (deduplication) cần được kiểm toán (audit) kỹ lưỡng hơn | Tránh việc bị tạo trùng lặp giao dịch trong DB nếu chạy đồng bộ nhiều lần |

## Checklist

- [ ] Các giao dịch trùng lặp bị bỏ qua chính xác (`skippedCount` tăng).
- [ ] Số dư tài khoản được cập nhật đúng tương ứng với các giao dịch mới được nạp.
- [ ] Xác minh quyền sở hữu tài khoản chặt chẽ.
