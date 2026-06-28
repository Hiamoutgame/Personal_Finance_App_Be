# User — Get Me

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /api/v1/user/me` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Lấy thông tin hồ sơ (profile) cơ bản của người dùng đang đăng nhập để hiển thị trên giao diện hoặc sử dụng trong việc phân quyền.

## Request

*Không yêu cầu Request Body*

## Response

```json
{
  "id": "guid",
  "userName": "string",
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "phone": "string | null",
  "avatarUrl": "string | null",
  "preferredCurrency": "string",
  "isOnboardingCompleted": "boolean"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh duy nhất của người dùng |
| userName | string | Tên đăng nhập |
| firstName | string | Tên |
| lastName | string | Họ |
| email | string | Địa chỉ email |
| phone | string hay null | Số điện thoại (nếu có) |
| avatarUrl | string hay null | Đường dẫn ảnh đại diện (nếu có) |
| preferredCurrency | string | Loại tiền tệ ưu tiên sử dụng |
| isOnboardingCompleted | boolean | Trạng thái đã hoàn thành thiết lập khảo sát đầu vào |

## Luồng xử lý

1. `UserController.GetUserInfor` nhận yêu cầu và gọi đến phương thức `User.IService.GetUserInfor`.
2. Dịch vụ lấy ID của người dùng từ các claim lưu trong JWT.
3. Dịch vụ dùng ID này để truy xuất thông tin tài khoản (`Account`) tương ứng.
4. Ánh xạ các thông tin cần thiết vào đối tượng DTO và trả về cho người dùng.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ truy xuất và hiển thị dữ liệu hồ sơ của chính người dùng hiện tại (current user) đang thực hiện yêu cầu.
- **Validation**: Đảm bảo token được cung cấp hợp lệ và chứa thông tin claim ID của người dùng.
- **Side effects**: Không tác động làm thay đổi dữ liệu trong cơ sở dữ liệu.
- **Security**: Không trả về các thông tin mật khẩu hoặc các trường dữ liệu mang tính bảo mật chỉ dành riêng cho quyền quản trị viên.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực thiếu, hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | Tài khoản tương ứng với ID không còn tồn tại trong hệ thống |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/UserController.cs` |
| Service | `Personal_Finance_Management.Service/User/Service.cs` |
| DTO | `Personal_Finance_Management.Service/User/Response.cs` |
| Entity | `Account` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-USER-001 | Route hiện tại trong code đang là `/api/v1/user` thay vì `/api/v1/user/me` | Cần cập nhật lại code controller để đồng bộ với contract |
| DRIFT-USER-002 | Tên trường `userName` cần được cân nhắc chuyển thành `username` chuẩn camelCase | Có thể gây mâu thuẫn naming convention ở phía Front-End |

## Checklist

- [ ] Trả về thông tin chính xác của người dùng đang đăng nhập.
- [ ] Ẩn các thông tin nhạy cảm.
- [ ] Route được thiết lập đúng là `/api/v1/user/me`.
