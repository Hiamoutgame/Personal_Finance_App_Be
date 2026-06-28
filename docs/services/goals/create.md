# Goals — Create

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/goals` |
| Auth | Bearer User |
| Status thành công | `201 Created` |
| Status hiện tại (code) | `201 Created` |

## Mục đích

Tạo mới một mục tiêu tiết kiệm tài chính (saving goal) cho người dùng hiện tại.

## Request

```json
{
  "title": "string",
  "targetAmount": "decimal",
  "dueDate": "datetime",
  "linkedJarId": "guid | null",
  "note": "string | null"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| title | string | ✅ | Tiêu đề của mục tiêu tiết kiệm |
| targetAmount | decimal | ✅ | Số tiền mục tiêu cần tiết kiệm (số dương) |
| dueDate | datetime | ✅ | Ngày hạn định để hoàn thành mục tiêu |
| linkedJarId | guid hay null | ❌ | ID hũ liên kết (nếu muốn liên kết tự động) |
| note | string hay null | ❌ | Ghi chú thêm về mục tiêu |

## Response

```json
{
  "id": "guid",
  "title": "string",
  "targetAmount": "decimal",
  "savedAmount": "decimal",
  "progressPercentage": "double",
  "status": "string",
  "dueDate": "datetime"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh duy nhất của mục tiêu mới tạo |
| title | string | Tiêu đề mục tiêu |
| targetAmount | decimal | Số tiền mục tiêu |
| savedAmount | decimal | Số tiền đã tiết kiệm được hiện tại (mặc định 0) |
| progressPercentage | double | Tiến độ hoàn thành mục tiêu (%) (mặc định 0.0) |
| status | string | Trạng thái mục tiêu (mặc định Active) |
| dueDate | datetime | Ngày tới hạn hoàn thành |

## Luồng xử lý

1. `GoalController` nhận yêu cầu và gọi đến phương thức `CreateGoal`.
2. Service thực hiện lấy ID của người dùng hiện tại từ JWT.
3. Validate số tiền cần tiết kiệm, ngày tới hạn và quyền sở hữu hũ ngân sách liên kết.
4. Chèn thêm một đối tượng `Goal` mới vào cơ sở dữ liệu với số tiền tiết kiệm ban đầu là 0.

## Quy tắc nghiệp vụ

- **Ownership**: Mục tiêu mới tạo phải thuộc sở hữu của người dùng hiện tại, hũ ngân sách liên kết cũng phải thuộc quyền sở hữu của người dùng này.
- **Validation**: Số tiền mục tiêu phải > 0, ngày tới hạn phải ở tương lai, ID hũ liên kết phải hợp lệ.
- **Side effects**: Thêm mới một bản ghi vào bảng `goals`.
- **Security**: Không cho phép tạo mục tiêu tiết kiệm hộ hoặc thay thế cho người dùng khác.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | Hũ ngân sách liên kết không tồn tại hoặc không thuộc sở hữu của người dùng |
| 422 | VALIDATION_FAILED | Số tiền mục tiêu <= 0 hoặc ngày tới hạn không hợp lệ |

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
| DRIFT-002 | Mô hình đóng góp mục tiêu (goal contribution) hiện chưa hoạt động chính thức | FE cần tự quản lý giao dịch đóng góp hoặc chờ thiết lập chuẩn |

## Checklist

- [ ] Xác minh quyền sở hữu chính xác đối với hũ ngân sách liên kết.
- [ ] Tiến độ và số tiền tiết kiệm ban đầu được đặt mặc định là 0.
- [ ] Trạng thái mặc định là hoạt động (`Active`).
