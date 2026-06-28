# Imports — Update Draft

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `PATCH /api/v1/imports/{id}/drafts/{draftId}` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Cho phép người dùng chỉnh sửa thông tin chi tiết của một bản nháp giao dịch (draft transaction) trước khi thực hiện xác nhận nhập dữ liệu chính thức.

## Request

```json
{
  "transactionDate": "datetimeOffset | null",
  "amount": "decimal | null",
  "type": "Income | Expense | null",
  "editedNote": "string | null",
  "editedCategoryId": "guid | null",
  "editedJarId": "guid | null",
  "isValid": "boolean | null",
  "validationError": "string | null"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| transactionDate | datetimeOffset hay null | ❌ | Ngày giao dịch điều chỉnh |
| amount | decimal hay null | ❌ | Số tiền giao dịch điều chỉnh |
| type | string hay null | ❌ | Loại giao dịch điều chỉnh (`Income` hoặc `Expense`) |
| editedNote | string hay null | ❌ | Ghi chú điều chỉnh |
| editedCategoryId | guid hay null | ❌ | Danh mục chi tiêu được chọn |
| editedJarId | guid hay null | ❌ | Hũ ngân sách được chọn |
| isValid | boolean hay null | ❌ | Trạng thái hợp lệ của bản nháp |
| validationError | string hay null | ❌ | Lỗi dữ liệu của bản nháp |

## Response

```json
{
  "id": "guid",
  "rowIndex": "int",
  "transactionDate": "datetimeOffset | null",
  "amount": "decimal | null",
  "type": "Income | Expense | null",
  "rawDescription": "string | null",
  "editedNote": "string | null",
  "isValid": "boolean",
  "validationError": "string | null",
  "editedCategoryId": "guid | null",
  "editedCategoryName": "string | null",
  "editedJarId": "guid | null",
  "editedJarName": "string | null",
  "updatedAt": "datetimeOffset"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh bản nháp giao dịch |
| rowIndex | int | Chỉ số dòng trong tệp dữ liệu nguồn |
| isValid | boolean | Bản nháp có hợp lệ để xác nhận hay không |
| editedCategoryName | string hay null | Tên danh mục chi tiêu sau điều chỉnh |
| editedJarName | string hay null | Tên hũ ngân sách sau điều chỉnh |

## Luồng xử lý

1. `ImportController` tiếp nhận yêu cầu cập nhật bản nháp và gọi `UpdateImportDraft(id, draftId, request)`.
2. Service tải thông tin tiến trình nhập và bản nháp giao dịch tương ứng theo quyền hạn của người dùng.
3. Xác nhận bản nháp thuộc về đúng tiến trình nhập tương ứng và tiến trình đó đang ở trạng thái cho phép chỉnh sửa.
4. Cập nhật các trường thông tin thay đổi vào DB.

## Quy tắc nghiệp vụ

- **Ownership**: Tiến trình nhập và bản nháp giao dịch phải thuộc sở hữu của người dùng hiện tại.
- **Validation**: Các trường danh mục, hũ liên kết mới phải được xác thực đúng quyền sở hữu.
- **Side effects**: Cập nhật thông tin trong bảng `import_transaction_drafts`.
- **Security**: Endpoint này chỉ chỉnh sửa dữ liệu nháp, tuyệt đối không tạo giao dịch thực tế hoặc thay đổi số dư tài khoản vào lúc này.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực không hợp lệ |
| 404 | NOT_FOUND | Tiến trình nhập hoặc bản nháp không tồn tại hoặc sai quyền sở hữu |
| 409 | INVALID_JOB_STATUS | Tiến trình nhập dữ liệu đã hoàn thành hoặc thất bại và không cho phép chỉnh sửa |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/ImportController.cs` |
| Service | `Personal_Finance_Management.Service/import/Service.cs` |
| DTO | `Personal_Finance_Management.Service/import/Request.cs`, `Response.cs` |
| Entity | `ImportTransactionDraft`, `ImportJob`, `Category`, `Jar` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-IMPORT-002 | Chưa ghi nhận sai lệch nào so với luồng nghiệp vụ thiết kế | Hoạt động bình thường |

## Checklist

- [ ] Xác minh quyền sở hữu tiến trình và bản nháp chính xác.
- [ ] Không tạo giao dịch thực tế hoặc thay đổi số dư tài khoản.
- [ ] Trả về thông tin bản nháp sau khi đã cập nhật thành công.
