# Jars — Update

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `PATCH /api/v1/jars/{id}` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Cập nhật các thông tin cơ bản (tên, màu sắc, biểu tượng) của một hũ ngân sách. Không cập nhật trực tiếp số dư (`balance`) của hũ thông qua endpoint này.

## Request

```json
{
  "name": "string | null",
  "color": "string | null",
  "icon": "string | null"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| name | string hay null | ❌ | Tên mới của hũ ngân sách |
| color | string hay null | ❌ | Màu hiển thị mới |
| icon | string hay null | ❌ | Biểu tượng hiển thị mới |

## Response

```json
{
  "id": "guid",
  "name": "string",
  "color": "string",
  "icon": "string",
  "status": "string"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh của hũ |
| name | string | Tên hũ sau khi cập nhật |
| color | string | Màu sắc sau khi cập nhật |
| icon | string | Biểu tượng sau khi cập nhật |
| status | string | Trạng thái hiện tại |

## Luồng xử lý

1. `JarController` nhận yêu cầu và gọi đến dịch vụ `UpdateJar(id, request)`.
2. Service thực hiện tải thông tin hũ dựa trên ID và kiểm tra quyền sở hữu đối với người dùng hiện tại.
3. Kiểm tra tính hợp lệ của các trường dữ liệu cập nhật.
4. Cập nhật các trường thông tin thay đổi và thực hiện lưu vào cơ sở dữ liệu.

## Quy tắc nghiệp vụ

- **Ownership**: Hũ ngân sách cần chỉnh sửa phải thuộc sở hữu của người dùng hiện tại.
- **Validation**: Không cho phép cập nhật thông tin đối với các hũ đã bị lưu trữ (archived jar) nếu nghiệp vụ yêu cầu cấm chỉnh sửa.
- **Side effects**: Cập nhật thông tin trong bảng `jars`.
- **Security**: Đảm bảo không cho phép nhận hoặc cập nhật trực tiếp giá trị số dư hoặc phần trăm từ request body.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | Hũ không tồn tại hoặc không thuộc sở hữu của người dùng |
| 422 | VALIDATION_FAILED | Tên hoặc dữ liệu khác truyền lên không hợp lệ |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/JarController.cs` |
| Service | `Personal_Finance_Management.Service/Jar/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Jar/Request.cs`, `Response.cs` |
| Entity | `Jar` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-JAR-002 | Chưa ghi nhận sai lệch nào so với luồng nghiệp vụ hiện tại | Hoạt động bình thường |

## Checklist

- [ ] Quyền sở hữu được kiểm tra chặt chẽ.
- [ ] Không cho phép thay đổi trực tiếp trường số dư (`balance`) hoặc phần trăm (`percentage`).
- [ ] Phản hồi khớp chính xác với DTO public.
