# Financial Accounts — Delete

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `DELETE /api/v1/financial-accounts/{id}` |
| Auth | Bearer User |
| Status thành công | `204 No Content` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Ngừng theo dõi hoặc xóa nhẹ (soft delete) một nguồn tiền của người dùng, bảo đảm giữ lại các dữ liệu lịch sử nếu nguồn tiền này đã có phát sinh giao dịch chi tiêu hoặc nhập sao kê.

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
| message | string | Tin nhắn thông báo xóa nguồn tiền thành công |

## Luồng xử lý

1. Controller tiếp nhận yêu cầu và gọi `DeleteFinancialAccount(id)`.
2. Service thực hiện tải dữ liệu nguồn tiền lên và xác minh quyền sở hữu đối với người dùng đang đăng nhập.
3. Kiểm tra xem nguồn tiền đã có liên kết với dữ liệu giao dịch (`Transaction`) hoặc lịch sử nhập sao kê (`ImportJob`) nào hay chưa.
4. Chuyển trạng thái hoạt động thành ẩn (soft deactivate/archive) hoặc xóa vật lý tùy thuộc điều kiện nghiệp vụ.

## Quy tắc nghiệp vụ

- **Ownership**: Nguồn tiền yêu cầu xóa phải thuộc quyền sở hữu của người dùng hiện tại.
- **Validation**: Hệ thống có thể ngăn chặn hành động xóa nếu đây là nguồn tiền duy nhất hoặc là nguồn tiền mặc định (default account) của người dùng.
- **Side effects**: Cập nhật trạng thái `isActive = false` hoặc xóa dòng dữ liệu tương ứng trong bảng `financial_accounts`.
- **Security**: Không làm hỏng hoặc mất lịch sử các giao dịch tài chính đã phát sinh của người dùng.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | Nguồn tiền không tồn tại hoặc không thuộc quyền sở hữu của người dùng hiện tại |
| 409 | DATA_ASSOCIATION_CONFLICT | Nguồn tiền đang chứa các dữ liệu liên quan không thể xóa vật lý |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/FinancialAccountController.cs` |
| Service | `Personal_Finance_Management.Service/FinancialAccount/Service.cs` |
| DTO | `Personal_Finance_Management.Service/FinancialAccount/Response.cs` |
| Entity | `FinancialAccount`, `Transaction`, `ImportJob` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-011 | Trạng thái thành công của API xóa nên là 204 No Content nhưng hiện tại có thể đang trả về 200 OK | FE nên linh hoạt xử lý mã HTTP status trả về |

## Checklist

- [ ] Không xóa vật lý (hard delete) dữ liệu nếu nguồn tiền đã có giao dịch hoặc lịch sử import đi kèm.
- [ ] Người dùng khác không thể tác động xóa nguồn tiền không thuộc sở hữu.
- [ ] Kích hoạt làm mới (refresh) lại giao diện danh sách và Dashboard sau khi xóa thành công.
