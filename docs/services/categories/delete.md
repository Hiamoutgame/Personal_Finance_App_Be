# Categories — Delete Custom

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `DELETE /api/v1/categories/{id}` |
| Auth | Bearer User |
| Status thành công | `204 No Content` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Xóa hoặc vô hiệu hóa (soft delete) danh mục tự định nghĩa của người dùng hiện tại.

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
| message | string | Tin nhắn thông báo xóa thành công |

## Luồng xử lý

1. `CategoryController` tiếp nhận yêu cầu và gọi `DeleteCustomCategory(id)`.
2. Service tải danh mục lên theo ID và kiểm tra quyền sở hữu đối với người dùng hiện tại.
3. Nếu danh mục đã có giao dịch liên quan, tiến hành ẩn hoặc vô hiệu hóa (soft delete/deactivate) để tránh lỗi toàn vẹn dữ liệu.
4. Trả về thông điệp thông báo kết quả xóa.

## Quy tắc nghiệp vụ

- **Ownership**: Danh mục tự định nghĩa phải thuộc sở hữu của người dùng đang đăng nhập.
- **Validation**: Không cho phép xóa các danh mục mặc định của hệ thống.
- **Side effects**: Cập nhật trạng thái hoặc xóa dòng dữ liệu tương ứng trong bảng `categories`.
- **Security**: Đảm bảo hành động xóa không gây ảnh hưởng đến danh mục của người dùng khác.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token không hợp lệ |
| 403 | FORBIDDEN | Cố gắng xóa danh mục mặc định hệ thống |
| 404 | NOT_FOUND | Danh mục không tồn tại hoặc không thuộc quyền sở hữu của user |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/CategoryController.cs` |
| Service | `Personal_Finance_Management.Service/category/Service.cs` |
| DTO | `Personal_Finance_Management.Service/category/Response.cs` |
| Entity | `Category`, `Transaction` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-011 | Mã trạng thái thành công mong muốn là 204 No Content nhưng hiện tại có thể đang trả về 200 OK kèm theo message | FE cần kiểm tra mã trạng thái nhận về |

## Checklist

- [ ] Danh mục mặc định của hệ thống không bị xóa bởi người dùng thông thường.
- [ ] Danh mục đã gắn với các giao dịch lịch sử không bị xóa vật lý làm hỏng lịch sử tài chính.
- [ ] Kiểm tra quyền sở hữu nghiêm ngặt trước khi thực hiện xóa.
