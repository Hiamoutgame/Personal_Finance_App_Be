# Transactions — Detail

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /api/v1/transactions/{id}` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Trả về thông tin chi tiết của một giao dịch cụ thể thuộc về người dùng hiện tại.

## Request

*Không yêu cầu Request Body (ID truyền qua Route)*

## Response

```json
{
  "id": "guid",
  "type": "Income | Expense",
  "transactionsAmount": "decimal",
  "note": "string | null",
  "date": "datetimeOffset",
  "financialAccount": {
    "id": "guid | null",
    "name": "string | null"
  },
  "jar": {
    "id": "guid | null",
    "name": "string | null"
  },
  "category": {
    "id": "guid | null",
    "name": "string | null"
  }
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh duy nhất của giao dịch |
| type | string | Loại giao dịch (`Income` hoặc `Expense`) |
| transactionsAmount | decimal | Số tiền giao dịch |
| note | string hay null | Ghi chú thêm |
| date | datetimeOffset | Ngày giờ thực hiện |
| financialAccount | object | Thông tin nguồn tiền liên kết |
| jar | object | Thông tin hũ ngân sách liên kết |
| category | object | Thông tin danh mục chi tiêu liên kết |

## Luồng xử lý

1. `TransactionsController` tiếp nhận yêu cầu và gọi `GetTransactionById(id)`.
2. Service lấy ID người dùng hiện tại từ token.
3. Tải thông tin giao dịch theo ID kết hợp điều kiện chưa xóa và quyền sở hữu.
4. Ánh xạ dữ liệu sang DTO phản hồi.

## Quy tắc nghiệp vụ

- **Ownership**: Giao dịch phải thuộc quyền sở hữu của người dùng hiện tại.
- **Validation**: Đảm bảo định dạng GUID truyền lên là hợp lệ.
- **Side effects**: Không tác động làm thay đổi dữ liệu trong cơ sở dữ liệu.
- **Security**: Không trả về các thông tin metadata nhạy cảm từ các nhà cung cấp đồng bộ bên ngoài.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | Giao dịch không tồn tại hoặc không thuộc sở hữu của người dùng hiện tại |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/TransactionsController.cs` |
| Service | `Personal_Finance_Management.Service/Transaction/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Transaction/Response.cs` |
| Entity | `Transaction` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-010 | Tài liệu API V2 hiện tại đang thiếu phần tiêu đề mô tả riêng cho endpoint này | FE dựa trên DTO code để thiết kế giao diện |

## Checklist

- [ ] Người dùng A không thể đọc thông tin giao dịch của người dùng B.
- [ ] Trả về mã lỗi 404 cho trường hợp không tìm thấy hoặc sai quyền sở hữu.
- [ ] Tài liệu API V2 được cập nhật đồng bộ thêm endpoint này.
