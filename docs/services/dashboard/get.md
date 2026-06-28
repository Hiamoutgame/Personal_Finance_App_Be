# Dashboard — Get Personal Dashboard

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /api/v1/dashboard` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Trả về thông tin tổng quan về tài chính cá nhân của người dùng hiện tại, bao gồm: tổng số dư, báo cáo thu/chi, số dư trong hũ, chi tiêu theo danh mục, các giao dịch gần đây và tiến độ đạt mục tiêu.

## Request

*Không yêu cầu Request Body*

## Response

```json
{
  "balanceSummary": {
    "totalBalance": "decimal",
    "allocatedBalance": "decimal",
    "unallocatedBalance": "decimal",
    "totalIncome": "decimal",
    "totalExpense": "decimal",
    "netChange": "decimal"
  },
  "financialAccounts": [
    {
      "id": "guid",
      "name": "string",
      "currentBalance": "decimal",
      "isDefault": "boolean"
    }
  ],
  "jarSummary": [
    {
      "jarId": "guid",
      "jarName": "string",
      "balance": "decimal",
      "spent": "decimal",
      "spentPercentage": "decimal"
    }
  ],
  "categoryBreakdown": [
    {
      "categoryId": "guid",
      "categoryName": "string",
      "totalAmount": "decimal",
      "percentage": "decimal"
    }
  ],
  "recentTransactions": [
    {
      "id": "guid",
      "type": "string",
      "transactionsAmount": "decimal",
      "note": "string | null",
      "date": "datetimeOffset"
    }
  ],
  "goalProgress": [
    {
      "goalId": "guid",
      "title": "string",
      "progressPercentage": "decimal",
      "daysRemaining": "decimal"
    }
  ]
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| balanceSummary | object | Thống kê số dư tổng hợp, thu nhập, chi phí |
| financialAccounts | mảng object | Danh sách nguồn tiền kèm số dư hiện tại |
| jarSummary | mảng object | Danh sách tóm tắt chi tiêu và số dư theo từng hũ |
| categoryBreakdown | mảng object | Tỉ lệ chi tiêu phân bổ theo các danh mục |
| recentTransactions | mảng object | Các giao dịch được thực hiện gần đây nhất |
| goalProgress | mảng object | Tiến độ của các mục tiêu tài chính đang hoạt động |

## Luồng xử lý

1. `DashboardController.GetDashboard` tiếp nhận yêu cầu và gọi tới dịch vụ.
2. Service lấy ID người dùng từ thông tin xác thực JWT.
3. Thực hiện truy vấn và tính toán thống kê (aggregate) dữ liệu từ nhiều bảng (nguồn tiền, hũ, giao dịch, danh mục, mục tiêu) thuộc về đúng người dùng đó.
4. Tổng hợp các thông tin thành đối tượng Dashboard DTO và trả về.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ truy xuất và tổng hợp dữ liệu thuộc quyền sở hữu của người dùng hiện tại (filter bằng `OwnerUserId`).
- **Validation**: Kiểm tra tính hợp lệ của access token.
- **Side effects**: Không tác động làm thay đổi dữ liệu trong cơ sở dữ liệu.
- **Security**: Đảm bảo không để rò rỉ bất kỳ thông tin số dư hay giao dịch nào của người dùng khác.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/DashboardController.cs` |
| Service | `Personal_Finance_Management.Service/Dashboard/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Dashboard/Response.cs` |
| Entity | Các entity: `FinancialAccount`, `Jar`, `Transaction`, `Category`, `Goal` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-DASHBOARD-001 | Route controller hiện tại có thể đang trỏ `/user/dashboard` thay vì chuẩn `/api/v1/dashboard` | Cần cập nhật controller về đúng tiền tố `/api/v1` |

## Checklist

- [ ] Các thông tin thống kê phải khớp hoàn toàn với dữ liệu thực tế lưu trong cơ sở dữ liệu.
- [ ] Tuân thủ nghiêm ngặt việc chia sẻ (rò rỉ) dữ liệu với người dùng khác.
- [ ] Logic tính toán phải bao gồm việc xử lý dấu (sign) chuẩn xác cho thu nhập (Income) và chi phí (Expense).
