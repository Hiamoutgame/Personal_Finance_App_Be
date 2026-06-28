# Auth — Logout

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/auth/logout` |
| Auth | Public |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Cho phép người dùng thực hiện yêu cầu đăng xuất khỏi hệ thống và thực hiện xóa các thông tin lưu trữ cục bộ phía client.

## Request

*Không yêu cầu Request Body*

## Response

```json
{
  "message": "string"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| message | string | Tin nhắn thông báo đăng xuất thành công |

## Luồng xử lý

1. `AuthController.Logout` tiếp nhận yêu cầu và chuyển tiếp đến `Auth.IService.Logout`.
2. Dịch vụ trả về thông điệp đăng xuất thành công.
3. Phía Front-End thực hiện xóa token lưu giữ ở phía client bất kể API có phản hồi thành công hay gặp lỗi.

## Quy tắc nghiệp vụ

- **Ownership**: API công khai, không thực hiện kiểm tra quyền sở hữu người dùng.
- **Validation**: Không yêu cầu kiểm tra dữ liệu đầu vào.
- **Side effects**: Không tác động làm thay đổi dữ liệu trong cơ sở dữ liệu.
- **Security**: Phiên bản MVP sử dụng token dạng stateless nên không thiết lập cơ chế thu hồi token hoặc lưu vào danh sách đen (blacklist) trên máy chủ.

## Lỗi

*Không có lỗi nghiệp vụ đặc thù nào được ghi nhận tại endpoint này.*

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/AuthController.cs` |
| Service | `Personal_Finance_Management.Service/Auth/Service.cs` |
| DTO | *Không áp dụng DTO cụ thể* |
| Entity | *Không áp dụng* |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-AUTH-002 | Chưa thiết lập thuộc tính `[Authorize]` trên endpoint | Endpoint hiện có thể được gọi mà không cần token xác thực |

## Checklist

- [ ] Thực hiện phản hồi thông điệp đăng xuất thành công cho FE.
- [ ] Phía client thực hiện xóa dữ liệu token an toàn.