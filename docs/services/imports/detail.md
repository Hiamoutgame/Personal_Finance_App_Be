# Imports — Detail

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /api/v1/imports/{id}` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Cung cấp thông tin chi tiết về một tiến trình nhập dữ liệu (import job) cùng toàn bộ các bản nháp giao dịch (draft lines) trực thuộc, hỗ trợ người dùng kiểm duyệt trước khi chính thức xác nhận.

## Request

*Không yêu cầu Request Body (ID truyền qua Route)*

## Response

```json
{
  "id": "guid",
  "financialAccountId": "guid",
  "fileName": "string",
  "originalContentType": "string | null",
  "status": "string",
  "progress": "int",
  "parsedCount": "int",
  "failedCount": "int",
  "errorMessage": "string | null",
  "imageUrl": "string",
  "uploadedAt": "datetimeOffset",
  "updatedAt": "datetimeOffset",
  "drafts": [
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
      "createdAt": "datetimeOffset",
      "updatedAt": "datetimeOffset"
    }
  ]
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh tiến trình nhập |
| financialAccountId | guid | ID tài khoản nguồn tiền liên kết |
| status | string | Trạng thái hiện tại |
| progress | int | Mức độ tiến độ (%) xử lý file |
| imageUrl | string | Đường dẫn xem trước hình ảnh/tệp tin đã upload |
| drafts | mảng object | Danh sách chi tiết các bản nháp giao dịch (drafts) |

## Luồng xử lý

1. `ImportController` tiếp nhận yêu cầu và gọi đến `GetImport(id)`.
2. Service thực hiện truy xuất thông tin tiến trình nhập theo ID và thuộc về người dùng hiện tại.
3. Kéo (Include) toàn bộ các bản nháp giao dịch tương ứng cùng các thông tin liên kết như danh mục, hũ.
4. Ánh xạ thành DTO chi tiết và trả về cho Front-End.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ truy xuất và trả về dữ liệu của tiến trình thuộc sở hữu của người dùng hiện tại.
- **Validation**: Định dạng tham số ID phải hợp lệ (GUID).
- **Side effects**: Không tác động làm thay đổi dữ liệu trong cơ sở dữ liệu.
- **Security**: Không để lộ đường dẫn tệp tin vật lý nội bộ; chỉ sử dụng `imageUrl` hoặc các endpoint lấy tệp an toàn.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 404 | NOT_FOUND | Tiến trình nhập không tồn tại hoặc không thuộc quyền sở hữu của người dùng |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/ImportController.cs` |
| Service | `Personal_Finance_Management.Service/import/Service.cs` |
| DTO | `Personal_Finance_Management.Service/import/Response.cs` |
| Entity | `ImportJob`, `ImportTransactionDraft` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-IMPORT-001 | Chưa có điểm lệch nào đáng kể về dữ liệu trả về so với cấu trúc hiện tại | FE hiển thị bảng nháp và cho phép chỉnh sửa dựa trên dữ liệu này |

## Checklist

- [ ] Thông tin của các bản nháp được trả về đầy đủ để tiện kiểm duyệt.
- [ ] Bảo mật đường dẫn hệ thống nội bộ, thay vào đó cung cấp đường dẫn hiển thị hợp lệ (URL).
- [ ] Xác minh quyền sở hữu người dùng chính xác.
