# Transactions — List

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /api/v1/transactions` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Trả về danh sách các giao dịch của người dùng hiện tại, hỗ trợ tìm kiếm, phân trang và nhiều bộ lọc chi tiết (theo nguồn tiền, hũ, danh mục, khoảng thời gian...).

## Request

*Truyền qua tham số query (Query Params)*

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| pageIndex | int | ❌ | Trang hiện tại (bắt đầu từ 1) |
| pageSize | int | ❌ | Số lượng giao dịch trên mỗi trang |
| financialAccountId | guid hay null | ❌ | Lọc theo nguồn tiền |
| type | string hay null | ❌ | Lọc theo loại (`Income` hoặc `Expense`) |
| jarId | guid hay null | ❌ | Lọc theo hũ |
| categoryId | guid hay null | ❌ | Lọc theo danh mục |
| fromDate | date hay null | ❌ | Lọc từ ngày |
| toDate | date hay null | ❌ | Lọc đến ngày |
| keyword | string hay null | ❌ | Tìm kiếm theo từ khóa trong ghi chú |
| sortBy | string hay null | ❌ | Trường sắp xếp |
| sortDir | string hay null | ❌ | Hướng sắp xếp (`asc` hoặc `desc`) |

## Response

```json
{
  "data": [
    {
      "id": "guid",
      "type": "string",
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
  ],
  "pagination": {
    "page": "int",
    "pageSize": "int",
    "totalCount": "int",
    "totalPages": "int"
  }
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| data | mảng object | Danh sách các giao dịch phù hợp |
| pagination | object | Thông tin phân trang |

## Luồng xử lý

1. `TransactionsController` tiếp nhận các tham số lọc và gọi `GetTransactions(request)`.
2. Service lấy ID người dùng hiện tại từ token.
3. Tạo truy vấn tìm kiếm giao dịch lọc theo quyền sở hữu và trạng thái chưa xóa.
4. Liên kết thông tin (Include) với nguồn tiền, hũ, danh mục tương ứng.
5. Trả về kết quả phân trang.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ trả về các giao dịch thuộc quyền sở hữu của chính người dùng hiện tại.
- **Validation**: Đảm bảo token xác thực hợp lệ.
- **Side effects**: Không tác động làm thay đổi dữ liệu trong cơ sở dữ liệu.
- **Security**: Không để rò rỉ thông tin về danh mục hay nguồn tiền của người dùng khác.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 422 | VALIDATION_FAILED | Tham số phân trang truyền lên không hợp lệ |

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
| DRIFT-005 | Cấu trúc phân trang hiện tại đang dùng `page/pageSize` trong khi một số tài liệu khác dùng `pageIndex` | FE cần xử lý linh hoạt theo cấu trúc phản hồi thực tế |

## Checklist

- [ ] Không hiển thị giao dịch của người dùng khác.
- [ ] Tính toán số tiền và phân loại giao dịch chính xác.
- [ ] Lọc đúng trạng thái giao dịch chưa bị xóa.
