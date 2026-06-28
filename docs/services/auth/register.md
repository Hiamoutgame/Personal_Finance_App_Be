# Auth — Register

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/auth/register` |
| Auth | Public |
| Status thành công | `201 Created` |
| Status hiện tại (code) | `201 Created` |

## Mục đích

Tạo mới tài khoản người dùng, mã băm mật khẩu, gán vai trò mặc định và phát hành mã JWT access token để đăng nhập ngay sau đó.

## Request

```json
{
  "username": "string",
  "email": "string",
  "password": "string",
  "firstName": "string",
  "lastName": "string"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| username | string | ✅ | Tên đăng nhập của người dùng |
| email | string | ✅ | Địa chỉ email của người dùng |
| password | string | ✅ | Mật khẩu để bảo mật tài khoản |
| firstName | string | ✅ | Tên |
| lastName | string | ✅ | Họ |

## Response

```json
{
  "id": "guid",
  "username": "string",
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "isOnboardingCompleted": "boolean",
  "accessToken": "string"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh duy nhất của tài khoản được tạo |
| username | string | Tên đăng nhập |
| firstName | string | Tên |
| lastName | string | Họ |
| email | string | Email |
| isOnboardingCompleted | boolean | Trạng thái khảo sát đầu vào (mặc định false) |
| accessToken | string | Mã JWT dùng để truy cập vào các tính năng bảo mật |

## Luồng xử lý

1. `AuthController.Register` nhận dữ liệu đầu vào và chuyển tiếp đến `Auth.IService.Register`.
2. Hệ thống kiểm tra hợp lệ dữ liệu và tính duy nhất của email và username trong DB.
3. Nếu hợp lệ, hệ thống tạo đối tượng `Account`, tạo mã băm mật khẩu an toàn (PasswordHash), gán quyền `User` và trạng thái Active.
4. Thông tin được lưu vào DB.
5. Hệ thống khởi tạo JWT dựa trên các claim thông tin của người dùng.
6. Trả về kết quả đăng ký thành công kèm theo token để tiếp tục luồng Onboarding.

## Quy tắc nghiệp vụ

- **Ownership**: API công khai để tạo tài khoản sở hữu gốc (root ownership).
- **Validation**: Các trường thông tin yêu cầu không được để trống, email và tên đăng nhập phải duy nhất.
- **Side effects**: Ghi dữ liệu vào bảng `accounts` trong DB. Không tạo sẵn dữ liệu Onboarding hay các hũ (jars) mặc định vào lúc này.
- **Security**: Không được phép trả về mật khẩu gốc hay mã băm mật khẩu (`PasswordHash`) thông qua API.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 422 | VALIDATION_FAILED | Các dữ liệu đầu vào chưa đúng chuẩn |
| 409 | RESOURCE_EXISTS | Email hoặc tên đăng nhập đã được đăng ký trước đó |

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
| DRIFT-AUTH-003 | Cấu trúc phản hồi đăng ký có thể được làm gọn hoặc phân cấp rõ ràng hơn | Phía Front-End cần sử dụng cấu trúc phẳng như trên cho tới khi có thay đổi |

## Checklist

- [ ] Tạo được tài khoản người dùng mới và trả về access token.
- [ ] Xử lý đúng trường hợp email hoặc username bị trùng.
- [ ] Bảo mật thông tin quan trọng.