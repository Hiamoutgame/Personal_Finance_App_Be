# Reminders — List

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /api/v1/reminders` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Trả về danh sách các nhắc nhở chi tiêu của người dùng hiện tại kèm theo thời gian tới hạn kế tiếp đã được tính toán.

## Request

*Không yêu cầu Request Body*

## Response

```json
{
  "data": [
    {
      "id": "guid",
      "title": "string",
      "amount": "decimal",
      "frequency": "string",
      "nextDueDate": "datetimeOffset",
      "status": "string"
    }
  ]
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh duy nhất của nhắc nhở |
| title | string | Tiêu đề nhắc nhở |
| amount | decimal | Số tiền dự kiến |
| frequency | string | Tần suất lặp lại |
| nextDueDate | datetimeOffset | Ngày tới hạn tiếp theo được tính toán |
| status | string | Trạng thái nhắc nhở |

## Luồng xử lý

1. `ReminderController.GetReminders` tiếp nhận yêu cầu và gọi Service.
2. Dịch vụ phân tích lấy ID của người dùng từ token.
3. Truy vấn danh sách các nhắc nhở thuộc về người dùng đó.
4. Tính toán thời gian tới hạn kế tiếp (`nextDueDate`) dựa theo cấu hình tần suất và ngày lặp, rồi trả về thông tin.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ trả về các nhắc nhở do chính người dùng hiện tại thiết lập.
- **Validation**: Yêu cầu token xác thực hợp lệ.
- **Side effects**: Không tác động làm thay đổi dữ liệu trong cơ sở dữ liệu.
- **Security**: Không để lộ thông tin của các người dùng khác.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |

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
| DRIFT-REMINDER-001 | Giá trị `NextDueDate` không được lưu trữ trong DB mà tính toán động tại Service | Đảm bảo tính nhất quán nhưng cần tối ưu hóa hiệu suất khi truy xuất |
| DRIFT-REMINDER-002 | Thiết lập của các tác vụ chạy ngầm để phát thông báo (reminder notification job) chưa được chốt bản triển khai | Có thể ảnh hưởng đến luồng gửi cảnh báo thực tế |

## Checklist

- [ ] Thời điểm tới hạn kế tiếp được tính đúng theo múi giờ và cấu hình lặp.
- [ ] Trả về đúng các giá trị trạng thái (status).
- [ ] Tuân thủ chặt chẽ quyền sở hữu người dùng.
