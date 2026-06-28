# Goals — List

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /api/v1/goals` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Trả về danh sách tất cả các mục tiêu tiết kiệm tài chính (saving goals) của người dùng hiện tại kèm theo tiến độ đã tính toán.

## Request

*Không yêu cầu Request Body*

## Response

```json
{
  "data": [
    {
      "id": "guid",
      "title": "string",
      "targetAmount": "decimal",
      "savedAmount": "decimal",
      "progressPercentage": "double",
      "dueDate": "datetime",
      "status": "string",
      "suggestedMonthlyContribution": "decimal",
      "linkedJarId": "guid | null"
    }
  ]
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| data | mảng object | Danh sách các mục tiêu tiết kiệm tương ứng của người dùng |
| id | guid | Định danh mục tiêu |
| title | string | Tiêu đề của mục tiêu |
| targetAmount | decimal | Số tiền cần đạt được |
| savedAmount | decimal | Số tiền đã tiết kiệm được |
| progressPercentage | double | Mức độ hoàn thành (%) |
| dueDate | datetime | Ngày tới hạn hoàn thành |
| status | string | Trạng thái hiện tại |
| suggestedMonthlyContribution | decimal | Số tiền gợi ý đóng góp hàng tháng |
| linkedJarId | guid hay null | Hũ chi tiêu đang liên kết (nếu có) |

## Luồng xử lý

1. `GoalController.GetGoals` nhận yêu cầu và gọi đến phương thức của Service.
2. Dịch vụ lấy ID của người dùng từ token bảo mật.
3. Truy vấn tất cả các mục tiêu dựa theo ID của người dùng và lọc theo trạng thái nếu có.
4. Tính toán tiến độ phần trăm và số tiền gợi ý đóng góp, ánh xạ sang DTO trả về.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ trả về thông tin danh sách các mục tiêu thuộc quyền sở hữu của chính người dùng hiện tại.
- **Validation**: Đảm bảo token truy cập hợp lệ.
- **Side effects**: Không tác động làm thay đổi dữ liệu trong cơ sở dữ liệu.
- **Security**: Đảm bảo người dùng không thể nhìn thấy thông tin mục tiêu hoặc liên kết hũ của những người dùng khác.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |

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
| DRIFT-002 | Mô hình thực hiện logic đóng góp và lấy dữ liệu chi tiết cho việc nạp tiền (contributions) đang trong trạng thái chờ | FE chỉ sử dụng các endpoint CRUD cho danh sách |

## Checklist

- [ ] Danh sách mục tiêu của người dùng khác không bị rò rỉ.
- [ ] Phần trăm tiến độ hiển thị đúng dựa trên lượng đã tiết kiệm.
- [ ] Danh sách thuộc tính enum trả về tương ứng.
