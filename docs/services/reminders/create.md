# Reminders — Create

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/reminders` |
| Auth | Bearer User |
| Status thành công | `201 Created` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Tạo thông báo nhắc nhở thanh toán hoặc chi tiêu định kỳ cho người dùng hiện tại.

## Request

```json
{
  "title": "string",
  "amount": "decimal",
  "frequency": "string",
  "dayOfMonth": "short | null",
  "startDate": "datetimeOffset",
  "categoryId": "guid | null",
  "notifyDaysBefore": "short | null",
  "note": "string | null"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| title | string | ✅ | Tiêu đề của nhắc nhở |
| amount | decimal | ✅ | Số tiền dự kiến |
| frequency | string | ✅ | Tần suất lặp (ví dụ: `Monthly`, `Weekly`) |
| dayOfMonth | short hay null | ❌ | Ngày trong tháng để nhắc nhở |
| startDate | datetimeOffset | ✅ | Ngày bắt đầu áp dụng |
| categoryId | guid hay null | ❌ | Danh mục liên quan |
| notifyDaysBefore | short hay null | ❌ | Nhắc trước bao nhiêu ngày |
| note | string hay null | ❌ | Ghi chú thêm |

## Response

```json
{
  "id": "guid",
  "title": "string",
  "amount": "decimal",
  "frequency": "string",
  "nextDueDate": "datetimeOffset",
  "status": "string"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh duy nhất của nhắc nhở |
| title | string | Tiêu đề nhắc nhở |
| amount | decimal | Số tiền dự kiến |
| frequency | string | Tần suất lặp lại |
| nextDueDate | datetimeOffset | Ngày tới hạn kế tiếp |
| status | string | Trạng thái hiện tại |

## Luồng xử lý

1. `ReminderController` tiếp nhận yêu cầu và gọi `CreateReminder`.
2. Dịch vụ lấy ID của người dùng từ token.
3. Validate tần suất (frequency), ngày trong tháng và quyền sở hữu danh mục.
4. Thêm nhắc nhở vào cơ sở dữ liệu và tính toán ngày tới hạn kế tiếp (`nextDueDate`) để trả về.

## Quy tắc nghiệp vụ

- **Ownership**: Nhắc nhở mới phải thuộc quyền sở hữu của người dùng hiện tại.
- **Validation**: Số tiền phải > 0, tần suất phải thuộc danh sách cho phép (enum), ngày trong tháng từ 1-31, danh mục phải hợp lệ (mặc định hoặc của người dùng).
- **Side effects**: Thêm mới một bản ghi vào bảng `reminders`.
- **Security**: Không được phép tạo nhắc nhở cho người dùng khác.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực không hợp lệ |
| 422 | VALIDATION_FAILED | Số tiền không hợp lệ hoặc các dữ liệu ngày tháng sai cấu trúc |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/ReminderController.cs` |
| Service | `Personal_Finance_Management.Service/Reminder/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Reminder/Request.cs`, `Response.cs` |
| Entity | `Reminder`, `Category` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-011 | Mã trạng thái thành công cho việc tạo nên là 201 Created nhưng hiện tại đang trả về 200 OK | Cần đồng bộ mã trạng thái giữa code và tài liệu |

## Checklist

- [ ] Kiểm tra chính xác quyền sở hữu danh mục liên quan.
- [ ] Tính toán chính xác thời điểm tới hạn kế tiếp (Next due date).
- [ ] Trạng thái mặc định sau khi tạo là `Active`.
