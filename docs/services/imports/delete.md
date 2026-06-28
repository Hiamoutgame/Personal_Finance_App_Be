# Imports — Delete

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `DELETE /api/v1/imports/{id}` |
| Auth | Bearer User |
| Status thành công | `204 No Content` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Hủy bỏ (cancel) hoặc xóa đi một tiến trình nhập dữ liệu (import job) cùng các dữ liệu nháp của nó, giúp người dùng dọn dẹp lịch sử nhập.

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
| message | string | Tin nhắn thông báo xóa tiến trình thành công |

## Luồng xử lý

1. `ImportController` tiếp nhận yêu cầu và gọi đến phương thức `DeleteImport(id)`.
2. Service thực hiện tải thông tin tiến trình nhập theo ID và đối chiếu quyền sở hữu với người dùng hiện tại.
3. Xác minh trạng thái hiện tại của tiến trình có cho phép xóa hay không.
4. Tiến hành ẩn (soft delete) hoặc hủy bỏ (cancel) tiến trình nhập cùng với các bản nháp giao dịch trực thuộc.
5. Thực hiện dọn dẹp tệp tin vật lý (nếu có logic dọn dẹp an toàn).

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ xóa các tiến trình nhập dữ liệu thuộc sở hữu của người dùng hiện tại.
- **Validation**: Tùy chỉnh quy định không cho phép xóa tiến trình đã hoàn tất (`Completed`) nếu nó đã sinh ra các giao dịch thực tế, trừ khi hệ thống có cơ chế khôi phục toàn diện (restore).
- **Side effects**: Cập nhật trạng thái xóa đối với bản ghi trong bảng `import_jobs` và `import_transaction_drafts`.
- **Security**: Tuyệt đối không xóa bất kỳ tệp tin nào ngoài thư mục upload được chỉ định để tránh lỗ hổng bảo mật truy cập đường dẫn (path traversal).

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | Tiến trình không tồn tại hoặc sai quyền sở hữu |
| 409 | INVALID_JOB_STATUS | Trạng thái hiện tại của tiến trình không cho phép xóa |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/ImportController.cs` |
| Service | `Personal_Finance_Management.Service/import/Service.cs` |
| DTO | `Personal_Finance_Management.Service/import/Response.cs` |
| Entity | `ImportJob`, `ImportTransactionDraft` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-011 | API hiện đang phản hồi mã 200 OK thay vì chuẩn 204 No Content | Phía FE cần hiển thị thông báo `message` trả về |

## Checklist

- [ ] Các bản nháp đi kèm được dọn dẹp hoặc đánh dấu xóa đúng theo tiến trình nhập.
- [ ] Tuân thủ nghiêm ngặt việc chống các lỗ hổng xóa tệp tin ngoài thư mục.
- [ ] Cấm người dùng tự ý xóa dữ liệu của tài khoản khác.
