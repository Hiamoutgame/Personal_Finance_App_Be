# Financial Accounts — Update

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `PATCH /api/v1/financial-accounts/{id}` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Cập nhật các thông tin cơ bản như tên hiển thị, cờ mặc định hoặc cập nhật số dư thủ công cho nguồn tiền. Hệ thống không thiết kế endpoint cập nhật số dư riêng lẻ.

## Request

```json
{
  "name": "string | null",
  "currentBalance": "decimal | null",
  "isDefault": "boolean | null"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| name | string hay null | ❌ | Tên hiển thị mới |
| currentBalance | decimal hay null | ❌ | Cập nhật số dư thủ công |
| isDefault | boolean hay null | ❌ | Có gán cờ mặc định hay không |

## Response

```json
{
  "id": "guid",
  "name": "string",
  "currentBalance": "decimal",
  "isDefault": "boolean",
  "updatedAt": "datetimeOffset"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh nguồn tiền |
| name | string | Tên nguồn tiền |
| currentBalance | decimal | Số dư sau khi cập nhật |
| isDefault | boolean | Trạng thái mặc định |
| updatedAt | datetimeOffset | Thời điểm cập nhật cuối cùng |

## Luồng xử lý

1. Controller tiếp nhận yêu cầu cập nhật và gọi `UpdateFinancialAccount(id, request)`.
2. Service thực hiện tải thông tin nguồn tiền dựa trên ID và đối chiếu quyền sở hữu với người dùng hiện tại.
3. Nếu yêu cầu cập nhật số dư, tiến hành kiểm tra xem nguồn tiền có thuộc chế độ liên kết API (`LinkedApi`) hay không. Chặn hành động cập nhật số dư thủ công cho các nguồn tiền liên kết tự động.
4. Cập nhật các trường thông tin thay đổi, xử lý tính duy nhất của cờ mặc định và lưu vào cơ sở dữ liệu.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ cho phép chỉnh sửa nguồn tiền thuộc sở hữu của chính người dùng đang đăng nhập.
- **Validation**: Đặt `currentBalance = null` nếu không muốn cập nhật số dư. Chặn cập nhật số dư thủ công đối với các tài khoản liên kết API (`LinkedApi`).
- **Side effects**: Cập nhật thông tin tương ứng trong bảng `financial_accounts`, có thể gỡ bỏ cờ mặc định của nguồn tiền khác.
- **Security**: Không nhận hoặc cập nhật thông tin người sở hữu (`userId`) từ request body.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | Nguồn tiền không tồn tại hoặc không thuộc sở hữu của người dùng hiện tại |
| 422 | VALIDATION_FAILED | Người dùng cố tình cập nhật số dư thủ công cho tài khoản liên kết API tự động |

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
| DRIFT-FA-002 | Tài liệu API V2 yêu cầu cập nhật số dư phải tích hợp chung qua endpoint PATCH này thay vì sử dụng endpoint `/balance` riêng | Đảm bảo không sử dụng hoặc phát triển lại endpoint `/balance` cũ |

## Checklist

- [ ] Không hồi sinh hoặc sử dụng lại endpoint `/balance` cũ.
- [ ] Xác minh nghiêm ngặt quyền sở hữu dựa theo ID người dùng.
- [ ] Bảo đảm quy tắc chặn cập nhật số dư thủ công cho tài khoản liên kết tự động.
