# Limits — Update

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `PATCH /api/v1/limits/{id}` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Cập nhật giá trị hạn mức chi tiêu (`limitAmount`) và ngưỡng cảnh báo phần trăm (`alertAtPercentage`) của một thiết lập hạn mức.

## Request

```json
{
  "limitAmount": "decimal | null",
  "alertAtPercentage": "decimal | null"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| limitAmount | decimal hay null | ❌ | Giá trị giới hạn tiền mới |
| alertAtPercentage | decimal hay null | ❌ | Ngưỡng phần trăm cảnh báo mới |

## Response

```json
{
  "id": "guid",
  "limitAmount": "decimal",
  "alertAtPercentage": "decimal"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh của hạn mức chi tiêu |
| limitAmount | decimal | Hạn mức chi tiêu sau khi cập nhật |
| alertAtPercentage | decimal | Ngưỡng cảnh báo sau khi cập nhật |

## Luồng xử lý

1. `LimitController` nhận yêu cầu cập nhật và gọi đến `UpdateLimit(id, request)`.
2. Service tải thông tin hạn mức và kiểm tra quyền sở hữu đối với người dùng hiện tại.
3. Validate thông tin số tiền mới và ngưỡng phần trăm.
4. Thực hiện lưu thông tin thay đổi vào cơ sở dữ liệu.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ cho phép chỉnh sửa hạn mức thuộc sở hữu của người dùng hiện tại.
- **Validation**: Số tiền giới hạn phải > 0, phần trăm cảnh báo phải nằm trong giới hạn hợp lệ.
- **Side effects**: Cập nhật dữ liệu tương ứng trong bảng `spending_limits`.
- **Security**: Không cho phép thay đổi mục tiêu (target) hoặc liên kết người dùng (user) thông qua endpoint chỉnh sửa này.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | Hạn mức không tồn tại hoặc không thuộc sở hữu của người dùng hiện tại |
| 422 | VALIDATION_FAILED | Số tiền hoặc phần trăm cảnh báo truyền lên không hợp lệ |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/LimitController.cs` |
| Service | `Personal_Finance_Management.Service/Limit/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Limit/Request.cs`, `Response.cs` |
| Entity | `SpendingLimit` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-LIMIT-002 | Chưa ghi nhận sai lệch nào về nghiệp vụ so với cấu trúc hiện tại | Hoạt động bình thường |

## Checklist

- [ ] Quyền sở hữu được kiểm tra chặt chẽ trước khi thay đổi.
- [ ] Không cho phép thay đổi đối tượng mục tiêu (`target`) thông qua endpoint này.
- [ ] Trạng thái cảnh báo sẽ tự động được đánh giá lại sau khi có giao dịch chi tiêu mới xuất hiện.
