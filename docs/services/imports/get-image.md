# Imports — Get Uploaded Image

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /api/v1/imports/images/{fileName}` |
| Auth | Bearer User |
| Status thành công | `200 OK` (trả về tệp tin vật lý) |
| Status hiện tại (code) | `200 OK` (File Stream) |

## Mục đích

Trả về luồng dữ liệu tệp tin hình ảnh đã tải lên trước đó, giúp Front-End có thể hiển thị ảnh hóa đơn hoặc nguồn sao kê để đối soát dữ liệu.

## Request

*Không yêu cầu Request Body (Tên file truyền qua Route)*

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| fileName | string | ✅ | Tên tệp tin hình ảnh trên máy chủ |

## Response

```
Luồng dữ liệu nhị phân (binary stream) của hình ảnh hoặc tệp PDF
```

| Thuộc tính phản hồi | Giá trị |
|---------------------|---------|
| Content-Type | `image/jpeg` / `image/png` / `application/pdf` |

## Luồng xử lý

1. `ImportController` tiếp nhận yêu cầu kèm theo tên tệp tin.
2. Service thực hiện kiểm tra tính hợp lệ của tên tệp tin và ánh xạ tới thư mục lưu trữ thực tế trên máy chủ.
3. Trích xuất thông tin tệp tin vật lý.
4. Controller phản hồi dữ liệu dưới dạng tệp tin (`PhysicalFile`).

## Quy tắc nghiệp vụ

- **Ownership**: Đảm bảo tệp tin yêu cầu xem thuộc quyền sở hữu của người dùng hiện tại, tránh tình trạng khai thác đọc trộm tệp tin của người khác.
- **Validation**: Tên tệp tin phải được làm sạch (sanitize) để ngăn chặn lỗ hổng đi ngược thư mục (`../`).
- **Side effects**: Không tác động làm thay đổi dữ liệu trong cơ sở dữ liệu.
- **Security**: Áp dụng nghiêm ngặt cơ chế phòng chống lỗ hổng Path Traversal.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | Tệp tin hình ảnh không tồn tại trên hệ thống hoặc không có quyền truy cập |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/ImportController.cs` |
| Service | `Personal_Finance_Management.Service/import/Service.cs` |
| Storage | Thư mục lưu trữ `Personal_Finance_Management.Service/import/Upload/` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-FA-007 | Cơ chế phân quyền dựa trên tên tệp tin (fileName ownership) cần được kiểm toán chặt chẽ hơn để đảm bảo tính riêng tư dữ liệu | Người dùng có thể vô tình đoán được tên tệp tin của người khác nếu không phân quyền |

## Checklist

- [ ] Ngăn chặn triệt để các ký tự đặc biệt như `../` để chống tấn công Path Traversal.
- [ ] Tệp tin chỉ được phép truy xuất trong phân vùng thư mục upload được chỉ định.
- [ ] Cơ chế phân quyền truy cập tệp tin hoạt động chính xác.
