# Categories — Create Custom

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/categories` |
| Auth | Bearer User |
| Status thành công | `201 Created` |
| Status hiện tại (code) | `201 Created` |

## Mục đích

Tạo danh mục chi tiêu tự định nghĩa (custom category) cho người dùng hiện tại.

## Request

```json
{
  "name": "string",
  "icon": "string | null",
  "color": "string | null"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| name | string | ✅ | Tên danh mục tự định nghĩa |
| icon | string hay null | ❌ | Mã biểu tượng của danh mục |
| color | string hay null | ❌ | Mã màu hiển thị danh mục |

## Response

```json
{
  "id": "guid",
  "name": "string",
  "icon": "string | null",
  "color": "string | null"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh của danh mục mới được tạo |
| name | string | Tên danh mục |
| icon | string hay null | Mã biểu tượng |
| color | string hay null | Mã màu |

## Luồng xử lý

1. `CategoryController` tiếp nhận request và gọi `CreateCustomCategory` của service.
2. Service lấy ID người dùng từ token.
3. Validate và chuẩn hóa các thông tin: tên, biểu tượng, mã màu.
4. Chèn (Insert) một bản ghi `Category` mới vào cơ sở dữ liệu với `OwnerUserId = currentUser` và `IsDefault = false`.

## Quy tắc nghiệp vụ

- **Ownership**: Danh mục mới được tạo thuộc quyền sở hữu của chính người dùng thực hiện yêu cầu.
- **Validation**: Tên danh mục là bắt buộc, có thể kiểm tra tính trùng lặp tên nếu service có hỗ trợ.
- **Side effects**: Thêm mới một dòng dữ liệu vào bảng `categories`.
- **Security**: Người dùng không thể tạo các danh mục mặc định hệ thống (default category) thông qua endpoint này.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token hết hạn hoặc không hợp lệ |
| 422 | VALIDATION_FAILED | Tên danh mục để trống hoặc định dạng màu không đúng |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/CategoryController.cs` |
| Service | `Personal_Finance_Management.Service/category/Service.cs` |
| DTO | `Personal_Finance_Management.Service/category/Request.cs`, `Response.cs` |
| Entity | `Category` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-011 | Cần kiểm tra xem mã trạng thái trả về của API tạo mới đã chuẩn hóa thành 201 Created chưa | Front-End cần xử lý mã trạng thái tương ứng |

## Checklist

- [ ] Danh mục được tạo có cờ `IsDefault = false` và thuộc quyền sở hữu của user hiện tại.
- [ ] Không tạo nhầm thành danh mục mặc định của hệ thống.
- [ ] Phản hồi thành công không trả về thực thể EF (EF Entity) trực tiếp.