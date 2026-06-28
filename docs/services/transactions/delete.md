# Transactions — Delete

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `DELETE /api/v1/transactions/{id}` |
| Auth | Bearer User |
| Status thành công | `204 No Content` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Thực hiện xóa nhẹ (soft delete) một giao dịch tài chính và khôi phục (hoàn tác) lại tất cả các tác động về số dư đã thực hiện trước đó.

## Request

*Không yêu cầu Request Body (ID truyền qua Route)*

## Response

```json
{
  "message": "string"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| message | string | Tin nhắn thông báo xóa giao dịch thành công |

## Luồng xử lý

1. `TransactionsController` gọi `DeleteTransaction(id)`.
2. Service thực hiện tải thông tin giao dịch theo ID và xác minh quyền sở hữu đối với người dùng hiện tại, đồng thời đảm bảo giao dịch này chưa bị xóa.
3. Tiến hành hoàn tác số tiền (hoặc đảo ngược tác động số tiền) đối với các bảng `FinancialAccount` và `Jar` tương ứng.
4. Gán thuộc tính xóa `IsDeleted = true`, `DeletedAt = DateTimeOffset.UtcNow`.
5. Lưu trữ tất cả thay đổi đồng bộ (atomic).

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ xóa giao dịch thuộc sở hữu của người dùng hiện tại.
- **Validation**: Tùy chỉnh nghiệp vụ có thể chặn không cho xóa các giao dịch liên kết từ sao kê (imported) hoặc hóa đơn (linked) tùy quy định.
- **Side effects**: Cập nhật trạng thái xóa trong bảng `transactions` và cập nhật lại số dư các tài khoản/hũ.
- **Security**: Không xóa hoàn toàn (hard delete) dữ liệu nếu nghiệp vụ yêu cầu kiểm toán dòng tiền.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | Giao dịch không tồn tại hoặc không thuộc quyền sở hữu của người dùng hiện tại |
| 409 | BALANCE_RESTORE_CONFLICT | Xảy ra lỗi xung đột số dư khi hoàn tác (ví dụ: làm số dư tài khoản/hũ bị âm quá mức cho phép) |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/TransactionsController.cs` |
| Service | `Personal_Finance_Management.Service/Transaction/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Transaction/Response.cs` |
| Entity | `Transaction`, `FinancialAccount`, `Jar` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-011 | Mã trạng thái thành công mong muốn là 204 No Content nhưng hiện tại có thể đang trả về 200 OK kèm thông điệp | FE kiểm tra mã trạng thái trả về |

## Checklist

- [ ] Giao dịch không bị xóa hoàn toàn khỏi DB nếu nghiệp vụ yêu cầu soft delete.
- [ ] Số dư tài khoản và hũ được khôi phục chính xác.
- [ ] Người dùng không thể xóa giao dịch của người dùng khác.
