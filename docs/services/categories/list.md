# Categories — List

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /api/v1/categories` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Lấy danh sách các danh mục mặc định của hệ thống kèm theo danh mục tự định nghĩa của riêng người dùng để Front-End tạo danh sách thả xuống (dropdown) hoặc hiển thị.

## Request

*Không yêu cầu Request Body*

## Response

```json
{
  "defaultCategories": [
    {
      "id": "guid",
      "name": "string",
      "icon": "string | null",
      "color": "string | null"
    }
  ],
  "customCategories": [
    {
      "id": "guid",
      "name": "string",
      "icon": "string | null",
      "color": "string | null"
    }
  ]
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| defaultCategories | mảng object | Danh sách các danh mục dùng chung của hệ thống |
| customCategories | mảng object | Danh sách các danh mục người dùng tự định nghĩa |

## Luồng xử lý

1. `CategoryController.GetCategories` nhận yêu cầu và gọi đến `category.IService.GetCategories`.
2. Dịch vụ phân tích lấy ID của người dùng hiện tại từ token.
3. Truy vấn các danh mục hệ thống có thuộc tính `IsDefault=true` và các danh mục tự định nghĩa có thuộc tính `OwnerUserId=currentUser`.
4. Lọc và chỉ trả về các danh mục đang hoạt động (không bị đánh dấu đã xóa) theo luật của hệ thống.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ trả về các danh mục tự định nghĩa thuộc về user đang truy vấn.
- **Validation**: Đảm bảo token truy cập hợp lệ.
- **Side effects**: Không tác động làm thay đổi dữ liệu trong cơ sở dữ liệu.
- **Security**: Đảm bảo người dùng không thể nhìn thấy danh mục tự định nghĩa của những người dùng khác.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/CategoryController.cs` |
| Service | `Personal_Finance_Management.Service/category/Service.cs` |
| DTO | `Personal_Finance_Management.Service/category/Response.cs` |
| Entity | `Category` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-CATEGORY-001 | Tính năng phân trang (pagination) chưa được áp dụng cho danh sách này | Hiện tại trả về tất cả danh mục trong một lần gọi |

## Checklist

- [ ] Các danh mục mặc định luôn hiển thị cho mọi người dùng.
- [ ] Các danh mục tự định nghĩa chỉ xuất hiện cho đúng chủ sở hữu tương ứng.
- [ ] Không làm rò rỉ (leak) những danh mục đã bị vô hiệu hóa (soft delete).
