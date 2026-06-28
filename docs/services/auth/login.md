# Auth — Login

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/auth/login` |
| Auth | Public |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Xác thực email và mật khẩu, cấp mã JWT access token cho người dùng (User/Admin) nếu thông tin đăng nhập chính xác và tài khoản không bị cấm (Banned).

## Request

```json
{
  "email": "string",
  "password": "string"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| email | string | ✅ | Email đăng nhập của tài khoản |
| password | string | ✅ | Mật khẩu tài khoản |

## Response

```json
{
  "id": "guid",
  "username": "string",
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "role": "string",
  "isOnboardingCompleted": "boolean",
  "accessToken": "string"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh duy nhất của tài khoản |
| username | string | Tên đăng nhập |
| firstName | string | Tên |
| lastName | string | Họ |
| email | string | Email đăng ký |
| role | string | Vai trò người dùng (User/Admin) |
| isOnboardingCompleted | boolean | Trạng thái đã hoàn thành khảo sát đầu vào |
| accessToken | string | Mã JWT dùng để xác thực cho các yêu cầu sau |

## Luồng xử lý

1. `AuthController.Login` nhận email/password từ request body và chuyển đến `Auth.IService.Login`.
2. Service tìm kiếm tài khoản (`Account`) tương ứng với email nhận được trong DB.
3. Kiểm tra xem tài khoản có bị cấm hoạt động (`status = "Banned"`) hay không.
4. Kiểm tra và so khớp mã băm mật khẩu (`PasswordHash`) bằng BCrypt.
5. Nếu khớp, tạo mã JWT access token chứa các thông tin claim cần thiết.
6. Trả về thông tin tài khoản và mã token tương ứng.

## Quy tắc nghiệp vụ

- **Ownership**: API công khai, không kiểm tra quyền sở hữu người dùng.
- **Validation**: Kiểm tra tính hợp lệ của định dạng email và đảm bảo các trường bắt buộc không bị bỏ trống.
- **Side effects**: Không tác động làm thay đổi dữ liệu trong cơ sở dữ liệu.
- **Security**: Không trả về dữ liệu nhạy cảm hoặc mật khẩu băm. Chặn mọi yêu cầu đăng nhập đối với các tài khoản có trạng thái Banned.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | INVALID_CREDENTIALS | Email hoặc mật khẩu không chính xác |
| 403 | ACCOUNT_BANNED | Tài khoản đang có trạng thái Banned |
| 422 | VALIDATION_FAILED | Các trường yêu cầu không hợp lệ hoặc bị trống |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/AuthController.cs` |
| Service | `Personal_Finance_Management.Service/Auth/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Auth/Request.cs`, `Response.cs` |
| Entity | `Account`, `Role` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-AUTH-001 | Cấu trúc phản hồi thực tế phẳng hơn so với cấu trúc lồng ghép mong muốn trong API V2 | Phía Front-End cần đọc cấu trúc phản hồi dạng phẳng hiện tại |

## Checklist

- [ ] Xác thực email/password thành công và trả về token hợp lệ.
- [ ] Chặn tài khoản bị cấm (Banned) đăng nhập.
- [ ] Không làm lộ mã băm mật khẩu hoặc thông tin bảo mật khác.