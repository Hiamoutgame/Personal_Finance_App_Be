# Jars — Archive

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `DELETE /api/v1/jars/{id}` |
| Auth | Bearer User |
| Status thành công | `204 No Content` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Lưu trữ (archive) hoặc vô hiệu hóa hũ ngân sách của người dùng. Nên sử dụng khái niệm "archive" thay vì xóa vật lý vì lịch sử số dư (balance) và lịch sử giao dịch cần được bảo toàn.

## Request

*Không yêu cầu Request Body (ID truyền qua Route)*

## Response

```json
{
  "message": "string"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| message | string | Tin nhắn thông báo lưu trữ thành công |

## Luồng xử lý

1. `JarController` tiếp nhận yêu cầu và gọi `DeleteJar(id)`.
2. Service thực hiện tải thông tin hũ theo ID và kiểm tra quyền sở hữu đối với người dùng hiện tại.
3. Kiểm tra số dư hiện tại của hũ và các giao dịch liên kết.
4. Chuyển đổi trạng thái hũ thành lưu trữ (archive/soft delete) theo quy định.

## Quy tắc nghiệp vụ

- **Ownership**: Hũ ngân sách yêu cầu phải thuộc sở hữu của người dùng hiện tại.
- **Validation**: Hệ thống có thể ngăn chặn hành động lưu trữ nếu số dư trong hũ vẫn khác 0.
- **Side effects**: Cập nhật trạng thái hũ hoặc đặt cờ xóa trong bảng `jars`.
- **Security**: Không xóa vật lý làm mất lịch sử dòng tiền của hệ thống.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 400 | JAR_BALANCE_NOT_ZERO | Số dư trong hũ khác 0 và không được phép lưu trữ |
| 401 | UNAUTHORIZED | Token không hợp lệ hoặc đã hết hạn |
| 404 | NOT_FOUND | Hũ không tồn tại hoặc không thuộc sở hữu của người dùng |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/JarController.cs` |
| Service | `Personal_Finance_Management.Service/Jar/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Jar/Response.cs` |
| Entity | `Jar`, `Transaction` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-011 | Mã trạng thái thành công mong muốn là 204 No Content nhưng hiện tại đang trả về 200 OK kèm theo message | FE cần kiểm tra mã trạng thái nhận về |

## Checklist

- [ ] Không cho phép người dùng lưu trữ hũ của người dùng khác.
- [ ] Xử lý đúng nghiệp vụ chặn lưu trữ nếu số dư hũ khác 0.
- [ ] Bảo toàn nguyên vẹn lịch sử giao dịch.
