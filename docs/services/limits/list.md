# Limits — List

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /api/v1/limits` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Trả về danh sách các mức giới hạn chi tiêu được thiết lập bởi người dùng hiện tại kèm theo thông tin tiến độ (spend/progress) và trạng thái hiện tại.

## Request

*Không yêu cầu Request Body*

## Response

```json
{
  "data": [
    {
      "id": "guid",
      "targetType": "Jar | Category",
      "targetId": "guid",
      "targetName": "string",
      "limitAmount": "decimal",
      "period": "string",
      "alertAtPercentage": "decimal",
      "currentSpent": "decimal",
      "currentPercentage": "double",
      "status": "string"
    }
  ]
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh duy nhất của giới hạn chi tiêu |
| targetType | string | Loại đối tượng áp dụng (`Jar` hoặc `Category`) |
| targetId | guid | ID của hũ hoặc danh mục liên quan |
| targetName | string | Tên hiển thị của hũ hoặc danh mục |
| limitAmount | decimal | Giá trị hạn mức tiền được thiết lập |
| period | string | Chu kỳ chi tiêu (`Daily` hoặc `Monthly`) |
| alertAtPercentage | decimal | Ngưỡng cảnh báo chi tiêu (%) |
| currentSpent | decimal | Số tiền hiện tại đã chi trong chu kỳ |
| currentPercentage | double | Tiến độ chi tiêu hiện tại (%) |
| status | string | Trạng thái của hạn mức |

## Luồng xử lý

1. `LimitController.GetLimits` tiếp nhận yêu cầu và gọi `Limit.IService.GetLimits`.
2. Service lấy ID người dùng từ JWT.
3. Truy vấn các hạn mức của người dùng này, liên kết dữ liệu với thông tin hũ/danh mục tương ứng.
4. Tính toán số tiền đã chi tiêu (`currentSpent`) và tiến độ (`currentPercentage`) trong chu kỳ tương ứng và trả về thông tin.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ lấy và trả về các hạn mức chi tiêu thuộc sở hữu của người dùng hiện tại.
- **Validation**: Yêu cầu token xác thực hợp lệ.
- **Side effects**: Không tác động làm thay đổi dữ liệu trong cơ sở dữ liệu.
- **Security**: Đảm bảo không làm rò rỉ dữ liệu về hạn mức hoặc các thực thể của người dùng khác.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/LimitController.cs` |
| Service | `Personal_Finance_Management.Service/Limit/Service.cs`, `SpendingLimitEvaluator.cs` |
| DTO | `Personal_Finance_Management.Service/Limit/Response.cs` |
| Entity | `SpendingLimit`, `Jar`, `Category`, `Transaction` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-LIMIT-001 | Endpoint xem chi tiết `GET /api/v1/spending-limits/{id}` vẫn còn trong trạng thái chờ và chưa có Controller | Không ảnh hưởng đến danh sách nhưng thiếu khả năng xem lẻ một hạn mức |

## Checklist

- [ ] Các hạn mức của người dùng khác không hiển thị trong danh sách.
- [ ] Số tiền đã chi tiêu được tính toán đúng dựa trên dữ liệu giao dịch đã ghi.
- [ ] Xác minh quyền sở hữu hũ và danh mục đích chính xác.
