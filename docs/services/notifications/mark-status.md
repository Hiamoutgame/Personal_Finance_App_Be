# Notifications — Mark Status

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `PATCH /api/v1/notifications/status` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Đánh dấu trạng thái đã đọc (read) hoặc chưa đọc (unread) cho danh sách thông báo được chọn, hoặc cho toàn bộ thông báo của người dùng hiện tại.

## Request

```json
{
  "ids": ["guid"],
  "isRead": true,
  "markAll": false
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| ids | mảng guid | ❌ | Danh sách ID thông báo cần cập nhật |
| isRead | boolean | ✅ | Trạng thái muốn thiết lập (true: đã đọc, false: chưa đọc) |
| markAll | boolean | ✅ | Đặt true để áp dụng cho tất cả thông báo của người dùng |

## Response

```json
{
  "updatedCount": 10,
  "unreadCount": 5
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| updatedCount | int | Số lượng thông báo đã được cập nhật trạng thái thành công |
| unreadCount | int | Số lượng thông báo chưa đọc còn lại của người dùng |

## Luồng xử lý

1. `NotificationController` tiếp nhận request và gọi `UpdateStatus(request)`.
2. Service lấy ID người dùng từ JWT.
3. Nếu `markAll = true`, tiến hành cập nhật trạng thái đọc cho toàn bộ thông báo của user đó.
4. Nếu `ids` có giá trị và `markAll = false`, lọc và cập nhật trạng thái đọc cho các thông báo tương ứng thuộc về user.
5. Tính toán lại và trả về số lượng thông báo đã được cập nhật kèm số lượng chưa đọc mới nhất.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ cập nhật thông báo thuộc sở hữu của người dùng hiện tại.
- **Validation**: Kiểm tra tính hợp lệ của mảng ID, trạng thái boolean và ngữ nghĩa của biến `markAll`.
- **Side effects**: Cập nhật giá trị trường `isRead` của các bản ghi tương ứng trong bảng `notifications`.
- **Security**: Các ID thông báo thuộc về người dùng khác gửi lên sẽ bị bỏ qua hoặc trả về lỗi không tìm thấy (tùy thuộc thiết lập nghiệp vụ).

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực không hợp lệ |
| 422 | VALIDATION_FAILED | Dữ liệu đầu vào không đúng định dạng hoặc thiếu các trường yêu cầu |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/NotificationController.cs` |
| Service | `Personal_Finance_Management.Service/Notification/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Notification/Request.cs`, `Response.cs` |
| Entity | `Notification` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-NOTIFICATION-001 | Cần làm rõ nghiệp vụ khi truyền danh sách `ids` trống đồng thời biến `markAll` bằng false | Tránh việc cập nhật nhầm hoặc không cập nhật gì mà vẫn báo thành công |

## Checklist

- [ ] Người dùng không được phép đánh dấu thông báo của người dùng khác.
- [ ] Số lượng thông báo chưa đọc phản hồi về sau khi cập nhật là chính xác.
- [ ] Logic hoạt động chính xác cho cả hai chế độ chọn lọc và áp dụng tất cả (markAll).
