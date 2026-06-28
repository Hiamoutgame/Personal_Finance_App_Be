# Goals — Detail

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /api/v1/goals/{id}` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Trả về thông tin chi tiết đầy đủ của một mục tiêu tiết kiệm nhằm hiển thị thông tin hoặc biểu đồ tiến độ.

## Request

*Không yêu cầu Request Body (ID truyền qua Route)*

## Response

```json
{
  "id": "guid",
  "title": "string",
  "targetAmount": "decimal",
  "savedAmount": "decimal",
  "progressPercentage": "double",
  "dueDate": "datetime",
  "daysRemaining": "int",
  "status": "string",
  "suggestedMonthlyContribution": "decimal",
  "linkedJarId": "guid | null"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh duy nhất của mục tiêu |
| title | string | Tiêu đề của mục tiêu |
| targetAmount | decimal | Số tiền cần đạt được |
| savedAmount | decimal | Số tiền đã tiết kiệm |
| progressPercentage | double | Mức độ hoàn thành (%) |
| dueDate | datetime | Ngày tới hạn hoàn thành |
| daysRemaining | int | Số ngày còn lại để hoàn thành |
| status | string | Trạng thái hiện tại của mục tiêu |
| suggestedMonthlyContribution | decimal | Số tiền hệ thống gợi ý cần đóng góp hàng tháng |
| linkedJarId | guid hay null | Hũ chi tiêu đang được liên kết (nếu có) |

## Luồng xử lý

1. `GoalController` tiếp nhận yêu cầu từ ID thông qua route và gọi `GetGoalById(id)`.
2. Dịch vụ phân tích lấy ID của người dùng từ token.
3. Tiến hành tải thông tin mục tiêu theo ID và lọc theo quyền sở hữu của người dùng.
4. Truy vấn thêm thông tin từ hũ liên kết (nếu có).
5. Tính toán tiến độ, thời gian còn lại (days remaining) và mức đóng góp hàng tháng được gợi ý.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ lấy và trả về chi tiết đối với mục tiêu tiết kiệm thuộc sở hữu của người dùng hiện tại.
- **Validation**: Yêu cầu định dạng GUID hợp lệ.
- **Side effects**: Không tác động làm thay đổi dữ liệu trong cơ sở dữ liệu.
- **Security**: Đảm bảo không làm rò rỉ (leak) dữ liệu hũ ngân sách liên kết không thuộc phạm vi quyền hạn.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực thiếu hoặc không hợp lệ |
| 404 | NOT_FOUND | Mục tiêu không tồn tại hoặc không thuộc quyền sở hữu của người dùng hiện tại |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/GoalController.cs` |
| Service | `Personal_Finance_Management.Service/Goal/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Goal/Response.cs` |
| Entity | `Goal`, `Jar` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-002 | Lịch sử giao dịch đóng góp (contribution history) vẫn đang chờ được triển khai logic hoàn chỉnh | Chưa lấy được lịch sử đóng góp vào mục tiêu cụ thể |

## Checklist

- [ ] Phản hồi lỗi mã 404 cho các mục tiêu bị người dùng truy cập trái phép.
- [ ] Thông tin của hũ liên kết được đảm bảo an toàn, không rò rỉ ra ngoài.
- [ ] Thông tin tiến độ tương thích với mức hiển thị tổng hợp ở danh sách (list endpoint).
