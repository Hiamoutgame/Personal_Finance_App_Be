# Limits — Delete

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `DELETE /api/v1/limits/{id}` |
| Auth | Bearer User |
| Status thành công | `204 No Content` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Xóa hoặc vô hiệu hóa (deactivate) một hạn mức chi tiêu của người dùng hiện tại.

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
| message | string | Tin nhắn thông báo xóa hạn mức thành công |

## Luồng xử lý

1. `LimitController` tiếp nhận yêu cầu và gọi `DeleteLimit(id)`.
2. Dịch vụ tải thông tin hạn mức và đối chiếu quyền sở hữu với người dùng hiện tại.
3. Thực hiện xóa vật lý hoặc vô hiệu hóa hạn mức.
4. Trả về trạng thái phản hồi kết quả thành công.

## Quy tắc nghiệp vụ

- **Ownership**: Hạn mức chi tiêu phải thuộc đúng quyền sở hữu của người dùng hiện tại.
- **Validation**: Đảm bảo ID được truyền lên là hợp lệ.
- **Side effects**: Cập nhật trạng thái hoặc xóa dòng dữ liệu tương ứng trong bảng thiết lập hạn mức.
- **Security**: Đảm bảo không thể tác động đến hạn mức của bất kỳ người dùng nào khác.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực thiếu, hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | ID của hạn mức không tồn tại hoặc không thuộc quyền sở hữu của người dùng |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/LimitController.cs` |
| Service | `Personal_Finance_Management.Service/Limit/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Limit/Response.cs` |
| Entity | `SpendingLimit` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-011 | Mã trạng thái thành công mong muốn là 204 No Content nhưng hiện tại có thể đang trả về 200 OK | FE cần dựa vào kết quả phản hồi thay vì chỉ kiểm tra mã HTTP status |

## Checklist

- [ ] Hạn mức bị xóa không còn kích hoạt bất kỳ thông báo (notification) nào.
- [ ] Tuân thủ nghiêm ngặt kiểm tra quyền sở hữu.
- [ ] Trạng thái trả về phải hiển thị rõ thông báo cho phía Front-End.
