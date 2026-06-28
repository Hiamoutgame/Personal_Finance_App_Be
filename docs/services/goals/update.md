# Goals — Update

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `PATCH /api/v1/goals/{id}` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Cập nhật các thông tin cơ bản (tiêu đề, hạn định, mô tả) hoặc thay đổi hũ ngân sách liên kết của mục tiêu tiết kiệm.

## Request

```json
{
  "title": "string | null",
  "targetAmount": "decimal | null",
  "dueDate": "datetime | null",
  "linkedJarId": "guid | null",
  "note": "string | null"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| title | string hay null | ❌ | Tiêu đề mới của mục tiêu |
| targetAmount | decimal hay null | ❌ | Số tiền cần đạt được mới |
| dueDate | datetime hay null | ❌ | Ngày tới hạn hoàn thành mới |
| linkedJarId | guid hay null | ❌ | ID hũ liên kết mới (truyền null để gỡ liên kết) |
| note | string hay null | ❌ | Ghi chú mới |

## Response

```json
{
  "id": "guid",
  "title": "string",
  "targetAmount": "decimal",
  "dueDate": "datetime",
  "status": "string"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh mục tiêu |
| title | string | Tiêu đề mục tiêu sau khi cập nhật |
| targetAmount | decimal | Số tiền mục tiêu sau khi cập nhật |
| dueDate | datetime | Ngày tới hạn sau khi cập nhật |
| status | string | Trạng thái hiện tại |

## Luồng xử lý

1. `GoalController` nhận yêu cầu cập nhật và gọi đến `UpdateGoal(id, request)`.
2. Service thực hiện tải thông tin mục tiêu lên từ DB và xác minh quyền sở hữu với người dùng hiện tại.
3. Validate các trường thông tin cập nhật gửi lên bao gồm hũ liên kết mới.
4. Cập nhật các trường thông tin thay đổi, tính toán lại tiến độ và lưu vào cơ sở dữ liệu.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ cho phép chỉnh sửa mục tiêu tiết kiệm thuộc về người dùng đang đăng nhập, hũ ngân sách liên kết mới cũng phải thuộc sở hữu của người dùng này.
- **Validation**: Số tiền mục tiêu mới phải > 0, ngày tới hạn mới phải ở tương lai.
- **Side effects**: Cập nhật thông tin trong bảng `goals`.
- **Security**: Không được phép thay đổi số tiền hiện tại đã tích lũy (`savedAmount`) trực tiếp từ phía client thông qua endpoint cập nhật này.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | Mục tiêu hoặc hũ liên kết mới không tồn tại hoặc sai quyền sở hữu |
| 422 | VALIDATION_FAILED | Số tiền mục tiêu mới <= 0 hoặc ngày tới hạn không hợp lệ |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/GoalController.cs` |
| Service | `Personal_Finance_Management.Service/Goal/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Goal/Request.cs`, `Response.cs` |
| Entity | `Goal`, `Jar` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-002 | Mô hình thực hiện nạp tiền đóng góp mục tiêu (contributions) đang trong trạng thái chờ | Tránh để client tự cập nhật giá trị `savedAmount` |

## Checklist

- [ ] Quyền sở hữu được kiểm tra chặt chẽ trước khi lưu thay đổi.
- [ ] Số tiền tích lũy (`savedAmount`) không bị thay đổi tùy tiện từ phía client.
- [ ] Tiến độ và trạng thái được tính toán đồng bộ nhất quán.
