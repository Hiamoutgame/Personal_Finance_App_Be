# Notifications — List

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /api/v1/notifications` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Lấy danh sách các thông báo (inbox) của người dùng hiện tại, hỗ trợ phân trang và lọc theo loại/trạng thái thông báo.

## Request

*Truyền qua tham số query (Query Params)*

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| type | string hay null | ❌ | Lọc theo loại thông báo |
| status | string hay null | ❌ | Lọc theo trạng thái (đã đọc/chưa đọc) |
| pageSize | int | ❌ | Số lượng thông báo trên một trang |
| pageIndex | int | ❌ | Chỉ số trang muốn lấy (bắt đầu từ 0 hoặc 1) |

## Response

```json
{
  "items": [
    {
      "id": "guid",
      "type": "string",
      "title": "string",
      "body": "string",
      "isRead": "boolean",
      "occurredAt": "datetimeOffset"
    }
  ],
  "totalItems": "int",
  "pageSize": "int",
  "pageIndex": "int",
  "unreadCount": "int"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| items | mảng object | Danh sách chi tiết các thông báo |
| totalItems | int | Tổng số thông báo phù hợp |
| pageSize | int | Số thông báo trên một trang |
| pageIndex | int | Chỉ số trang hiện tại |
| unreadCount | int | Tổng số thông báo chưa đọc của user |

## Luồng xử lý

1. `NotificationController.GetNotifications` tiếp nhận các tham số filter và gọi service tương ứng.
2. Dịch vụ lấy ID của người dùng từ token bảo mật.
3. Truy vấn bảng thông báo dựa theo điều kiện lọc và ID của người dùng hiện tại.
4. Trả về kết quả phân trang kèm số lượng thông báo chưa đọc.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ trả về các thông báo đích danh gửi cho người dùng hiện tại.
- **Validation**: Kiểm tra loại và trạng thái xem có hợp lệ (nếu được khai báo bằng Enum).
- **Side effects**: Không tác động làm thay đổi dữ liệu trong cơ sở dữ liệu.
- **Security**: Không để lộ nội dung thông báo của người dùng khác.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực thiếu, hết hạn hoặc không hợp lệ |
| 422 | VALIDATION_FAILED | Các tham số phân trang cung cấp không hợp lệ |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/NotificationController.cs` |
| Service | `Personal_Finance_Management.Service/Notification/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Notification/Response.cs` |
| Entity | `Notification` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-005 | Cấu trúc phân trang hiện tại đang dùng `pageIndex/pageSize` thay vì cấu trúc chuẩn | Cần thống nhất định dạng phân trang chuẩn cho tất cả các danh sách |

## Checklist

- [ ] Số lượng thông báo chưa đọc (unread count) tính đúng cho người dùng hiện tại.
- [ ] Tính năng lọc hoạt động ổn định và không phát sinh lỗi (crash) khi không truyền tham số.
- [ ] Nội dung thông báo không chứa thông tin cơ sở dữ liệu bảo mật ẩn.
