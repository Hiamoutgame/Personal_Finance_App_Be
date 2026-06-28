# Imports — Confirm

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/imports/{id}/confirm` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Xác nhận (confirm) một yêu cầu nhập dữ liệu (import job) để tạo hàng loạt giao dịch thực tế (batch transactions) từ các dữ liệu bản nháp (drafts) đã được kiểm duyệt, đồng thời cập nhật số dư nguồn tiền và trạng thái tiến trình một cách đồng bộ.

## Request

```json
{
  "financialAccountId": "guid | null",
  "fromJarId": "guid | null",
  "draftIds": ["guid"]
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| financialAccountId | guid hay null | ❌ | Nguồn tiền mặc định để gán cho các giao dịch nháp (nếu chưa chọn) |
| fromJarId | guid hay null | ❌ | Hũ mặc định để trừ tiền (nếu chưa chọn) |
| draftIds | mảng guid | ✅ | Danh sách ID các giao dịch nháp muốn xác nhận |

## Response

```json
{
  "importJobId": "guid",
  "status": "Completed",
  "createdCount": 5,
  "transactions": [
    {
      "id": "guid",
      "draftId": "guid",
      "financialAccountId": "guid | null",
      "fromJarId": "guid | null",
      "categoryId": "guid | null",
      "type": "Income | Expense",
      "transactionsAmount": "decimal",
      "transactionDate": "datetimeOffset"
    }
  ],
  "message": "Import confirmed"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| importJobId | guid | ID của tiến trình nhập dữ liệu |
| status | string | Trạng thái mới của tiến trình (`Completed`) |
| createdCount | int | Số lượng giao dịch thực tế đã tạo thành công |
| transactions | mảng object | Danh sách chi tiết các giao dịch được tạo |
| message | string | Lời nhắn thông báo thành công |

## Luồng xử lý

1. `ImportController` tiếp nhận yêu cầu và gọi đến `ConfirmImport(id, request)`.
2. Service tải thông tin tiến trình nhập (`ImportJob`) và các bản nháp giao dịch tương ứng theo quyền hạn của người dùng.
3. Validate trạng thái tiến trình phải ở mức chờ duyệt (`AwaitingReview`) và kiểm tra quyền sở hữu đối với nguồn tiền.
4. Lần lượt tạo giao dịch thực tế từ các bản nháp giao dịch đã sửa đổi.
5. Cập nhật lại số dư tài khoản/hũ ngân sách, cập nhật số lượng và trạng thái tiến trình nhập, thực hiện lưu trữ đồng bộ (atomic) trong một DB transaction.

## Quy tắc nghiệp vụ

- **Ownership**: Tiến trình nhập, nguồn tiền và các bản nháp được chọn đều phải thuộc quyền sở hữu của người dùng hiện tại.
- **Validation**: Đảm bảo các thông tin ngày tháng, số tiền, tài khoản của các bản nháp là hợp lệ; chặn việc xác nhận trùng lặp.
- **Side effects**: Thêm mới hàng loạt giao dịch (`transactions`), cập nhật số dư các bảng tài khoản/hũ, và thay đổi trạng thái của tiến trình nhập.
- **Security**: Đảm bảo tính nhất quán dữ liệu, tránh tình trạng xác nhận trùng lặp (double confirm).

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | Tiến trình nhập hoặc tài khoản không tồn tại hoặc sai quyền sở hữu |
| 409 | INVALID_JOB_STATUS | Tiến trình nhập dữ liệu đã được xác nhận hoặc thất bại trước đó |
| 422 | VALIDATION_FAILED | Các bản nháp được chọn thiếu thông tin quan trọng hoặc không hợp lệ |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/ImportController.cs` |
| Service | `Personal_Finance_Management.Service/import/Service.cs` |
| DTO | `Personal_Finance_Management.Service/import/Request.cs`, `Response.cs` |
| Entity | `ImportJob`, `ImportTransactionDraft`, `Transaction`, `FinancialAccount` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-009 | Sự sai khác giữa việc import sao kê (statement import) và tải hóa đơn để OCR | FE cần sử dụng đúng route xác nhận này cho tiến trình tương ứng |

## Checklist

- [ ] Việc xác nhận phải diễn ra đồng bộ (atomic), không tạo giao dịch nửa chừng nếu gặp lỗi.
- [ ] Trạng thái tiến trình chuyển sang `Completed`.
- [ ] Số dư tài khoản và hũ được cập nhật đúng tương ứng.
