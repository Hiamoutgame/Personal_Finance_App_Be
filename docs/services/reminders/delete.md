# Reminders — Delete/Cancel

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `DELETE /api/v1/reminders/{id}` |
| Auth | Bearer User |
| Status thành công | `204 No Content` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Hủy bỏ (cancel) hoặc xóa nhắc nhở của người dùng hiện tại, thường được xử lý bằng cách chuyển trạng thái (status) thành `Cancelled`.

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
| message | string | Tin nhắn thông báo hủy nhắc nhở thành công |

## Luồng xử lý

1. `ReminderController` tiếp nhận yêu cầu và gọi `DeleteReminder(id)`.
2. Dịch vụ tải thông tin nhắc nhở tương ứng với người dùng hiện tại.
3. Chuyển trạng thái của nhắc nhở thành `Cancelled` hoặc vô hiệu hóa theo luật của hệ thống.
4. Trả về thông điệp phản hồi.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ thao tác trên các nhắc nhở thuộc quyền sở hữu của người dùng hiện tại.
- **Validation**: Đảm bảo nhắc nhở đang ở trạng thái có thể hủy.
- **Side effects**: Cập nhật trạng thái hoặc xóa dòng dữ liệu tương ứng trong bảng `reminders`.
- **Security**: Những tác vụ chạy ngầm (background jobs) sinh ra thông báo phải tự động bỏ qua các nhắc nhở đã bị `Cancelled`.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | ID nhắc nhở không tồn tại hoặc không thuộc sở hữu của người dùng |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/ReminderController.cs` |
| Service | `Personal_Finance_Management.Service/Reminder/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Reminder/Response.cs` |
| Entity | `Reminder` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-011 | Trạng thái xóa mong muốn là 204 No Content nhưng hiện tại có thể đang trả về 200 OK kèm thông điệp | FE xử lý cẩn thận mã trạng thái |

## Checklist

- [ ] Các nhắc nhở của người dùng khác không bị ảnh hưởng.
- [ ] Nhắc nhở đã bị hủy (Cancelled) không tiếp tục sinh thông báo rác cho người dùng.
- [ ] Thông báo kết quả trả về rõ ràng cho Front-End.
