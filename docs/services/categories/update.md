# Categories — Update Custom

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `PATCH /api/v1/categories/{id}` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Cập nhật thông tin chi tiết (tên, biểu tượng, màu sắc) cho một danh mục tự định nghĩa của người dùng hiện tại.

## Request

```json
{
  "name": "string | null",
  "icon": "string | null",
  "color": "string | null"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| name | string hay null | ❌ | Tên mới của danh mục |
| icon | string hay null | ❌ | Mã biểu tượng mới |
| color | string hay null | ❌ | Mã màu hiển thị mới |

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
| id | guid | Định danh của danh mục |
| name | string | Tên danh mục đã cập nhật |
| icon | string hay null | Mã biểu tượng đã cập nhật |
| color | string hay null | Mã màu đã cập nhật |

## Luồng xử lý

1. `CategoryController` tiếp nhận request và gọi `UpdateCustomCategory(id, request)`.
2. Service tải dữ liệu danh mục từ cơ sở dữ liệu lên dựa theo ID và kiểm tra quyền sở hữu với user hiện tại.
3. Chặn các yêu cầu nếu danh mục đó là danh mục mặc định của hệ thống.
4. Cập nhật các trường thông tin thay đổi và tiến hành lưu vào cơ sở dữ liệu.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ cho phép chỉnh sửa danh mục khi trường `OwnerUserId` khớp với user hiện tại.
- **Validation**: Tên, màu sắc, biểu tượng phải hợp lệ theo quy tắc hệ thống; chặn cập nhật đối với danh mục có cờ mặc định.
- **Side effects**: Cập nhật dữ liệu tương ứng trong bảng `categories`.
- **Security**: Danh mục mặc định chỉ có thể được quản lý bởi Quản trị viên (Admin).

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token không hợp lệ hoặc đã hết hạn |
| 403 | FORBIDDEN | Người dùng cố ý cập nhật các danh mục mặc định của hệ thống |
| 404 | NOT_FOUND | Danh mục không tồn tại hoặc không thuộc sở hữu của người dùng |
| 422 | VALIDATION_FAILED | Tên hoặc dữ liệu nhập vào không hợp lệ |

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
| DRIFT-CATEGORY-002 | Chưa ghi nhận sai lệch nào về nghiệp vụ so với cấu trúc hiện tại | Hệ thống hoạt động như hợp đồng đề ra |

## Checklist

- [ ] Không cho phép cập nhật danh mục của người dùng khác.
- [ ] Không cho phép cập nhật các danh mục mặc định.
- [ ] Các giao dịch lịch sử gắn với danh mục này sẽ hiển thị thông tin mới (như tên) nếu tham chiếu được tải lại.