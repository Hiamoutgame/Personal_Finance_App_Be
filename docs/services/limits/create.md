# Limits — Create

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/limits` |
| Auth | Bearer User |
| Status thành công | `201 Created` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Tạo mới một mức giới hạn chi tiêu (spending limit) cho một danh mục hoặc một hũ ngân sách của người dùng hiện tại.

## Request

```json
{
  "targetType": "Jar | Category",
  "targetId": "guid",
  "limitAmount": "decimal",
  "period": "string",
  "alertAtPercentage": "decimal"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| targetType | string | ✅ | Đối tượng áp dụng giới hạn (`Jar` hoặc `Category`) |
| targetId | guid | ✅ | ID của hũ hoặc danh mục muốn giới hạn |
| limitAmount | decimal | ✅ | Hạn mức chi tiêu cụ thể (số tiền) |
| period | string | ✅ | Chu kỳ áp dụng (`Daily` hoặc `Monthly`) |
| alertAtPercentage | decimal | ✅ | Ngưỡng phần trăm (%) cảnh báo sắp đạt giới hạn |

## Response

```json
{
  "id": "guid",
  "targetType": "Jar | Category",
  "targetId": "guid",
  "limitAmount": "decimal",
  "period": "string",
  "alertAtPercentage": "decimal"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh của hạn mức chi tiêu mới tạo |
| targetType | string | Loại đối tượng đã áp dụng |
| targetId | guid | ID đối tượng đã áp dụng |
| limitAmount | decimal | Hạn mức chi tiêu đã tạo |
| period | string | Chu kỳ chi tiêu |
| alertAtPercentage | decimal | Ngưỡng cảnh báo (%) |

## Luồng xử lý

1. `LimitController` nhận yêu cầu và chuyển đến `CreateLimit` của dịch vụ.
2. Dịch vụ xác thực người dùng hiện tại và đối tượng mục tiêu.
3. Ánh xạ loại mục tiêu thành `JarId` hoặc `CategoryId` tương ứng.
4. Chèn thêm một bản ghi mới vào bảng `spending_limits`.

## Quy tắc nghiệp vụ

- **Ownership**: Đối tượng mục tiêu (Hũ hoặc Danh mục) phải thuộc quyền sở hữu của người dùng hiện tại, hoặc là một danh mục mặc định hợp lệ.
- **Validation**: Số tiền hạn mức phải > 0, chu kỳ thuộc các loại định sẵn, phần trăm cảnh báo (alertAtPercentage) nằm trong khoảng 1-100.
- **Side effects**: Thêm mới một dòng vào cơ sở dữ liệu cho phần thiết lập giới hạn.
- **Security**: Không cho phép tạo giới hạn cho bất kỳ người dùng nào khác.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | Đối tượng mục tiêu (hũ/danh mục) không tồn tại hoặc không thuộc sở hữu của người dùng |
| 422 | VALIDATION_FAILED | Số tiền giới hạn nhỏ hơn hoặc bằng 0, hoặc thông tin yêu cầu không hợp lệ |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/LimitController.cs` |
| Service | `Personal_Finance_Management.Service/Limit/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Limit/Request.cs`, `Response.cs` |
| Entity | `SpendingLimit`, `Jar`, `Category` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-011 | Trạng thái phản hồi cho hành động tạo mới đáng lẽ là 201 Created nhưng hiện tại có thể đang trả về 200 OK | Cần thống nhất lại mã phản hồi và FE cần tự thích ứng với mã hiện tại |

## Checklist

- [ ] Đối tượng thiết lập hạn mức là hợp lệ và được xác minh đúng quyền sở hữu.
- [ ] Phần trăm cảnh báo nằm trong mức giới hạn cho phép.
- [ ] Hạn mức có hiệu lực ngay sau khi khởi tạo thành công.
