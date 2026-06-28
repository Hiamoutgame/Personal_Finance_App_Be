# Goals — Delete/Cancel

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `DELETE /api/v1/goals/{id}` |
| Auth | Bearer User |
| Status thành công | `204 No Content` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Hủy bỏ (cancel) hoặc xóa một mục tiêu tiết kiệm tài chính của người dùng hiện tại, thường được xử lý bằng cách chuyển trạng thái (status) thành `Cancelled`.

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
| message | string | Tin nhắn thông báo xóa mục tiêu thành công |

## Luồng xử lý

1. `GoalController` tiếp nhận yêu cầu và gọi `DeleteGoal(id)`.
2. Service thực hiện tải thông tin mục tiêu lên dựa theo ID và đối chiếu quyền sở hữu của người dùng hiện tại.
3. Tiến hành xóa hoặc chuyển đổi trạng thái của mục tiêu thành `Cancelled` theo luật nghiệp vụ.
4. Trả về thông điệp thông báo kết quả.

## Quy tắc nghiệp vụ

- **Ownership**: Mục tiêu tiết kiệm yêu cầu xóa phải thuộc quyền sở hữu của người dùng hiện tại.
- **Validation**: Hệ thống có thể ngăn chặn hành động xóa đối với các mục tiêu đã hoàn thành (Completed) nếu quy tắc nghiệp vụ cấm chỉnh sửa.
- **Side effects**: Cập nhật trạng thái hoặc xóa dòng dữ liệu tương ứng trong bảng `goals`.
- **Security**: Đảm bảo hành động không ảnh hưởng đến số dư của các hũ liên quan nếu mô hình đóng góp mục tiêu chưa hoạt động chính thức.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực thiếu, hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | ID mục tiêu không tồn tại hoặc không thuộc quyền sở hữu của người dùng |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/GoalController.cs` |
| Service | `Personal_Finance_Management.Service/Goal/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Goal/Response.cs` |
| Entity | `Goal` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-002 | Vấn đề đồng bộ số tiền đóng góp mục tiêu | Cần đảm bảo lịch sử giao dịch liên quan không bị ảnh hưởng |
| DRIFT-011 | Mã trạng thái thành công mong muốn là 204 No Content nhưng hiện tại có thể đang trả về 200 OK kèm theo thông điệp | FE kiểm tra mã trạng thái trả về |

## Checklist

- [ ] Người dùng không thể xóa mục tiêu tiết kiệm của người dùng khác.
- [ ] Trạng thái mục tiêu và lịch sử hoạt động được cập nhật đúng.
- [ ] Refresh lại tiến trình hiển thị trên Dashboard.
