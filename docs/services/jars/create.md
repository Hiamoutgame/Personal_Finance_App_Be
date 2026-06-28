# Jars — Create

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/jars` |
| Auth | Bearer User |
| Status thành công | `201 Created` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Tạo mới một hũ ngân sách cho người dùng hiện tại.

## Request

```json
{
  "name": "string",
  "color": "string",
  "icon": "string"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| name | string | ✅ | Tên hũ ngân sách |
| color | string | ✅ | Mã màu hiển thị |
| icon | string | ✅ | Mã biểu tượng |

## Response

```json
{
  "id": "guid",
  "name": "string",
  "balance": "decimal",
  "status": "string"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh của hũ mới |
| name | string | Tên hũ |
| balance | decimal | Số dư ban đầu của hũ (mặc định là 0) |
| status | string | Trạng thái hiện tại (vd: Active) |

## Luồng xử lý

1. `JarController` nhận yêu cầu và chuyển đến `CreateJar` trong Service.
2. Dịch vụ xác định ID người dùng hiện tại từ thông tin JWT.
3. Kiểm tra tính hợp lệ của các thông tin: tên, màu sắc, biểu tượng.
4. Chèn một đối tượng `Jar` mới vào DB với số dư mặc định và trạng thái hoạt động (active).

## Quy tắc nghiệp vụ

- **Ownership**: Hũ mới được tạo và gán cho người dùng hiện tại.
- **Validation**: Tên hũ là thông tin bắt buộc. Tuyệt đối không lưu các trường liên quan đến phần trăm phân bổ (`Percentage`) trong DB vì chúng được tính toán dựa trên cấu hình dịch vụ.
- **Side effects**: Thêm bản ghi mới vào bảng `jars`.
- **Security**: Ngăn chặn người dùng tạo hũ thay cho người dùng khác.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 422 | VALIDATION_FAILED | Thiếu tên hoặc cấu trúc màu/biểu tượng không đúng |

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
| DRIFT-011 | API hiện tại trả về mã 200 thay vì 201 cho yêu cầu tạo mới | FE cần dựa vào JSON trả về để xác định kết quả |

## Checklist

- [ ] Không lưu trường `Percentage` không cần thiết.
- [ ] Hũ mới được phân bổ quyền sở hữu đúng cho current user.
- [ ] Dữ liệu số dư (`balance`) không được phép thiết lập từ trực tiếp phía client thông qua các hàm Create.
