# Transactions — Create

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/transactions` |
| Auth | Bearer User |
| Status thành công | `201 Created` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Tạo mới một giao dịch thu/chi, đồng thời tự động cập nhật số dư liên quan trong cùng một chu trình giao dịch (transaction boundary).

## Request

```json
{
  "financialAccountId": "guid | null",
  "type": "string",
  "transactionsAmount": "decimal",
  "categoryId": "guid | null",
  "fromJarId": "guid | null",
  "toJarId": "guid | null",
  "note": "string | null",
  "date": "datetimeOffset"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| financialAccountId | guid hay null | ❌ | Nguồn tiền liên kết |
| type | string | ✅ | Phân loại (`Income` hoặc `Expense`) |
| transactionsAmount | decimal | ✅ | Số tiền giao dịch (luôn dương) |
| categoryId | guid hay null | ❌ | ID danh mục |
| fromJarId | guid hay null | ❌ | Hũ nguồn bị trừ tiền (nếu có) |
| toJarId | guid hay null | ❌ | Hũ đích được cộng tiền (nếu có) |
| note | string hay null | ❌ | Ghi chú giao dịch |
| date | datetimeOffset | ✅ | Thời gian giao dịch |

## Response

```json
{
  "id": "guid",
  "financialAccountId": "guid | null",
  "type": "string",
  "transactionsAmount": "decimal",
  "date": "datetimeOffset"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | ID giao dịch |
| financialAccountId | guid hay null | Nguồn tiền liên kết |
| type | string | Loại giao dịch |
| transactionsAmount | decimal | Số tiền giao dịch |
| date | datetimeOffset | Thời gian thực hiện |

## Luồng xử lý

1. `TransactionsController` gọi phương thức `CreateTransaction(request)`.
2. Dịch vụ lấy ID của người dùng từ token.
3. Validate số tiền (>0), phân loại (`type`) và quyền sở hữu (ownership) đối với nguồn tiền, danh mục, hũ.
4. Tạo dữ liệu `Transaction` và đồng thời cập nhật `FinancialAccount.CurrentBalance`, `Jar.Balance`.
5. Kiểm tra và kích hoạt các mức giới hạn chi tiêu (spending limits) hoặc cảnh báo nếu đây là một khoản chi phí (`Expense`).
6. Lưu trữ tất cả thay đổi đồng bộ (atomic) bằng EF transaction.

## Quy tắc nghiệp vụ

- **Ownership**: Nguồn tiền, hũ, danh mục tự định nghĩa đều phải thuộc về người dùng hiện tại; các danh mục mặc định được dùng chung.
- **Validation**: Số tiền truyền qua API phải là số dương. Type theo hợp đồng chỉ có `Income` hoặc `Expense`.
- **Side effects**: Thêm mới giao dịch, cập nhật số dư, có thể phát sinh thông báo (notification).
- **Security**: Không lấy ID người dùng từ dữ liệu payload.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 400 | INSUFFICIENT_BALANCE | Số dư của tài khoản hoặc hũ không đủ |
| 401 | UNAUTHORIZED | Token không hợp lệ |
| 404 | NOT_FOUND | Thực thể (hũ, danh mục, nguồn tiền) không hợp lệ hoặc không có quyền sở hữu |
| 422 | VALIDATION_FAILED | Số tiền <= 0 hoặc loại giao dịch sai |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/TransactionsController.cs` |
| Service | `Personal_Finance_Management.Service/Transaction/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Transaction/Request.cs`, `Response.cs` |
| Entity | `Transaction`, `FinancialAccount`, `Jar`, `Category`, `Notification`, `SpendingLimit` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-003 | Loại `Transfer` vẫn còn hiện diện trong DB nhưng hợp đồng yêu cầu chỉ `Income/Expense` | Cần chốt loại bỏ Transfer khỏi public route |
| DRIFT-004 | Expense ở API gửi lên số dương, nhưng DB có thể đang lưu Expense là dương trái ngược với schema gốc là lưu âm | Cần chốt quy ước |
| DRIFT-011 | API hiện tại trả về mã trạng thái 200 thay vì 201 cho lệnh khởi tạo | FE phải xử lý dữ liệu trả về linh hoạt |

## Checklist

- [ ] FE luôn gửi lượng tiền (Amount) là số dương.
- [ ] Luồng dịch chuyển tiền (Money movement) đảm bảo tính nguyên tử (atomic) và không làm sai lệch số dư.
