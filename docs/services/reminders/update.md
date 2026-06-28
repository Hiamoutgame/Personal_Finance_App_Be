# Reminders — Update

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `PATCH /api/v1/reminders/{id}` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Cập nhật các trường thông tin hoặc thay đổi trạng thái hoạt động (`Active`, `Paused`, `Completed`, `Cancelled`) của một nhắc nhở chi tiêu.

## Request

```json
{
  "title": "string | null",
  "amount": "decimal | null",
  "frequency": "string | null",
  "dayOfMonth": "int | null",
  "status": "Active | Paused | Completed | Cancelled | null",
  "notifyDaysBefore": "int | null",
  "note": "string | null"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| title | string hay null | ❌ | Tiêu đề nhắc nhở mới |
| amount | decimal hay null | ❌ | Số tiền mới |
| frequency | string hay null | ❌ | Tần suất nhắc nhở mới |
| dayOfMonth | int hay null | ❌ | Ngày trong tháng mới |
| status | string hay null | ❌ | Trạng thái mới |
| notifyDaysBefore | int hay null | ❌ | Số ngày nhắc trước mới |
| note | string hay null | ❌ | Ghi chú mới |

## Response

```json
{
  "id": "guid",
  "title": "string",
  "frequency": "string",
  "nextDueDate": "datetimeOffset",
  "status": "string"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh nhắc nhở |
| title | string | Tiêu đề |
| frequency | string | Tần suất nhắc |
| nextDueDate | datetimeOffset | Ngày tới hạn kế tiếp mới được tính lại |
| status | string | Trạng thái hiện tại |

## Luồng xử lý

1. `ReminderController` tiếp nhận yêu cầu và gọi `UpdateReminder(id, request)`.
2. Service tải thông tin nhắc nhở dựa trên ID và kiểm tra quyền sở hữu đối với người dùng hiện tại.
3. Kiểm tra tính hợp lệ của các trường dữ liệu cập nhật và quá trình chuyển đổi trạng thái (status transition).
4. Tiến hành cập nhật thông tin nhắc nhở và tính toán lại ngày tới hạn kế tiếp.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ cho phép chỉnh sửa nhắc nhở thuộc về người dùng hiện tại.
- **Validation**: Đảm bảo tần suất, ngày trong tháng và các trạng thái thuộc danh mục cho phép.
- **Side effects**: Cập nhật thông tin tương ứng trong bảng `reminders`.
- **Security**: Chặn không cho cập nhật các trường liên quan đến danh mục hay người dùng ngoài phạm vi cho phép.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | Nhắc nhở không tồn tại hoặc không thuộc sở hữu của người dùng hiện tại |
| 422 | VALIDATION_FAILED | Số tiền hoặc tần suất cập nhật không hợp lệ |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/ReminderController.cs` |
| Service | `Personal_Finance_Management.Service/Reminder/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Reminder/Request.cs`, `Response.cs` |
| Entity | `Reminder` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-REMINDER-003 | Tiến trình chạy nền (background job) phát thông báo cần được đảm bảo sẽ bỏ qua các nhắc nhở có trạng thái Paused hoặc Cancelled | Tránh việc vẫn phát thông báo khi người dùng đã tạm dừng hoặc hủy |

## Checklist

- [ ] Quyền sở hữu được kiểm tra chặt chẽ trước khi thay đổi.
- [ ] Ngày tới hạn kế tiếp được tính toán lại chính xác sau khi cập nhật thông tin.
- [ ] Trạng thái Cancelled/Paused không tiếp tục phát sinh thông báo.
