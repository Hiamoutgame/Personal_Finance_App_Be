# User — Update Me

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `PATCH /api/v1/user/me` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Cập nhật một số thông tin hồ sơ cá nhân của người dùng đang đăng nhập.

## Request

```json
{
  "firstName": "string | null",
  "lastName": "string | null",
  "phone": "string | null",
  "avatarUrl": "string | null"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| firstName | string hay null | ❌ | Tên mới |
| lastName | string hay null | ❌ | Họ mới |
| phone | string hay null | ❌ | Số điện thoại mới |
| avatarUrl | string hay null | ❌ | Đường dẫn ảnh đại diện mới |

## Response

```json
{
  "id": "guid",
  "fullName": "string",
  "phone": "string",
  "avatarUrl": "string"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh duy nhất của người dùng |
| fullName | string | Họ và tên đầy đủ sau khi cập nhật |
| phone | string | Số điện thoại sau khi cập nhật |
| avatarUrl | string | Ảnh đại diện sau khi cập nhật |

## Luồng xử lý

1. `UserController.UpdateUserProfile` nhận dữ liệu đầu vào và chuyển tiếp đến `User.IService.UpdateUserProfile`.
2. Dịch vụ phân tích ID người dùng từ mã JWT.
3. Tải thông tin tài khoản người dùng, tiến hành kiểm tra tính hợp lệ của các dữ liệu mới gửi lên.
4. Cập nhật các trường thông tin thay đổi vào DB và lưu lại.
5. Phản hồi thông tin hồ sơ mới được cập nhật về phía client.

## Quy tắc nghiệp vụ

- **Ownership**: Người dùng chỉ được quyền cập nhật hồ sơ cá nhân của chính mình.
- **Validation**: Kiểm tra tính hợp lệ và chuẩn hóa dữ liệu văn bản/số điện thoại/ảnh đại diện nếu cần thiết.
- **Side effects**: Cập nhật thông tin tương ứng trong bảng `accounts`.
- **Security**: Không cho phép thay đổi các trường vai trò (role), trạng thái tài khoản (status), mật khẩu hoặc email thông qua endpoint cập nhật hồ sơ chung này.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | Tài khoản của người dùng không tồn tại trong hệ thống |
| 422 | VALIDATION_FAILED | Số điện thoại hoặc dữ liệu khác không hợp lệ |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/UserController.cs` |
| Service | `Personal_Finance_Management.Service/User/Service.cs` |
| DTO | `Personal_Finance_Management.Service/User/Request.cs`, `Response.cs` |
| Entity | `Account` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-USER-004 | Dữ liệu phản hồi sau khi ghi (response write) có thể nên lược bớt các trường không cần thiết hoặc khớp sát DTO | Phía Front-End sử dụng đúng cấu trúc phản hồi hiện tại |

## Checklist

- [ ] Chặn không cho phép đổi các thuộc tính vai trò, trạng thái, mật khẩu.
- [ ] Chỉ cho phép người dùng thay đổi thông tin của chính họ.
- [ ] Phản hồi dữ liệu khớp với định dạng công khai.
