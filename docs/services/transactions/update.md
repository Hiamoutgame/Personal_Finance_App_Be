# Transactions — Update

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `PATCH /api/v1/transactions/{id}` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Chỉnh sửa thông tin số tiền, danh mục, hoặc ghi chú của một giao dịch, và tự động điều chỉnh lại số dư tài khoản/hũ trước đó.

## Request

```json
{
  "transactionsAmount": "decimal | null",
  "categoryId": "guid | null",
  "note": "string | null"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| transactionsAmount | decimal hay null | ❌ | Số tiền mới |
| categoryId | guid hay null | ❌ | Danh mục chi tiêu mới |
| note | string hay null | ❌ | Ghi chú mới |

## Response

```json
{
  "id": "guid",
  "type": "string",
  "transactionsAmount": "decimal",
  "date": "datetimeOffset"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh duy nhất của giao dịch |
| type | string | Loại giao dịch |
| transactionsAmount | decimal | Số tiền giao dịch sau khi cập nhật |
| date | datetimeOffset | Thời gian giao dịch |

## Luồng xử lý

1. `TransactionsController` tiếp nhận yêu cầu và gọi `UpdateTransaction(id, request)`.
2. Service thực hiện tải thông tin giao dịch theo quyền sở hữu của người dùng hiện tại.
3. Hoàn tác số tiền tác động từ trước đối với số dư tài khoản và hũ.
4. Kiểm tra tính hợp lệ của số tiền và danh mục mới.
5. Áp dụng các thay đổi số dư theo thông tin giao dịch mới và tiến hành lưu lại.

## Quy tắc nghiệp vụ

- **Ownership**: Giao dịch, danh mục, hũ và nguồn tiền đều phải được xác minh thuộc quyền sở hữu của người dùng hiện tại.
- **Validation**: Người dùng chỉ được sửa trường `transactionsAmount`, `categoryId`, `note`. Việc thay đổi loại giao dịch (`type`), hũ (`jar`), nguồn tiền (`account`) hoặc ngày tháng là không được phép.
- **Side effects**: Cập nhật thông tin bản ghi giao dịch và điều chỉnh số dư tương ứng trên bảng tài khoản/hũ.
- **Security**: Những giao dịch tự động đồng bộ (imported/linked) sẽ có quy định chặn cập nhật thủ công nếu hệ thống triển khai rule này.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 400 | INSUFFICIENT_BALANCE | Số dư tài khoản hoặc hũ không đủ sau khi điều chỉnh cập nhật |
| 401 | UNAUTHORIZED | Token xác thực thiếu, hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | Giao dịch hoặc danh mục không tồn tại hoặc sai quyền sở hữu |
| 422 | VALIDATION_FAILED | Số tiền hoặc thông tin mới cung cấp không hợp lệ |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/TransactionsController.cs` |
| Service | `Personal_Finance_Management.Service/Transaction/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Transaction/Request.cs`, `Response.cs` |
| Entity | `Transaction`, `FinancialAccount`, `Jar`, `Category` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-003 | Loại `Transfer` vẫn còn hiện diện trong hệ thống dù theo mong muốn chỉ hỗ trợ `Income/Expense` | Có thể gây nhiễu luồng hoàn tác số dư nếu gặp dữ liệu Transfer |
| DRIFT-004 | Quy ước số dư Expense đang lệch giữa DB và API | Việc cập nhật cần chú ý quy ước dấu hiện tại trong code |

## Checklist

- [ ] Phải luôn hoàn tác số dư cũ trước khi áp dụng số dư mới.
- [ ] Không cho phép thay đổi `type`, `account`, `jar`, `date` theo yêu cầu hợp đồng.
- [ ] Đảm bảo tính nhất quán của số dư sau khi cập nhật.
