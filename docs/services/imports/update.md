# Imports — Update Import

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `PATCH /api/v1/imports/{id}` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Cập nhật thông tin cấp độ nháp (draft-level data) hoặc cấp độ tiến trình nhập dữ liệu. Việc triển khai hiện tại (implementation) của endpoint này đang tiếp nhận cấu trúc `UpdateImportDraftRequest` để chỉnh sửa bản nháp đầu tiên; tuy nhiên cần phải xem xét lại về mặt ngữ nghĩa sử dụng lâu dài.

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
| editedCategoryId | guid hay null | ❌ | Danh mục chi tiêu điều chỉnh |
| editedJarId | guid hay null | ❌ | Hũ ngân sách điều chỉnh |
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
| isValid | boolean | Trạng thái hợp lệ |
| updatedAt | datetimeOffset | Thời điểm cập nhật cuối cùng |

## Luồng xử lý

1. `ImportController` tiếp nhận yêu cầu và gọi `UpdateImport(id, request)`.
2. Service thực hiện tải thông tin tiến trình nhập và bản nháp liên quan dựa trên quyền sở hữu của người dùng hiện tại.
3. Kiểm tra xem tiến trình nhập có đang ở trạng thái cho phép chỉnh sửa hay không.
4. Cập nhật các trường thông tin thay đổi vào cơ sở dữ liệu.

## Quy tắc nghiệp vụ

- **Ownership**: Tiến trình nhập dữ liệu bắt buộc phải thuộc sở hữu của người dùng hiện tại.
- **Validation**: Đảm bảo tiến trình nhập dữ liệu vẫn đang trong trạng thái cho phép chỉnh sửa.
- **Side effects**: Cập nhật thông tin bản ghi bản nháp giao dịch trực thuộc.
- **Security**: Tuyệt đối không cho phép chỉnh sửa thông qua endpoint này nếu như giao dịch thực tế tương ứng đã được xác nhận tạo thành công.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | Tiến trình nhập dữ liệu không tồn tại hoặc sai quyền sở hữu |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/ImportController.cs` |
| Service | `Personal_Finance_Management.Service/import/Service.cs` |
| DTO | `Personal_Finance_Management.Service/import/Request.cs`, `Response.cs` |
| Entity | `ImportJob`, `ImportTransactionDraft` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-IMPORT-003 | Endpoint này hiện đang xử lý cho hành động chỉnh sửa bản nháp thay vì chỉnh sửa thông tin tiến trình nhập | Nếu FE đã có cụ thể ID bản nháp, nên ưu tiên gọi endpoint `/drafts/{draftId}` thay cho endpoint này. Cần chốt lại ngữ nghĩa thiết kế API |

## Checklist

- [ ] Người dùng không được phép chỉnh sửa các thông tin sau khi giao dịch thực tế đã được xác nhận.
- [ ] Tuân thủ việc kiểm tra nghiêm ngặt quyền sở hữu.
- [ ] Giao diện FE sử dụng dữ liệu phản hồi để hiển thị làm mới thông tin.
