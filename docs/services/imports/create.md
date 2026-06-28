# Imports — Create Import

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/imports` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Khởi tạo một tiến trình nhập dữ liệu (import job) thông qua việc tải lên tệp tin (sao kê hoặc hóa đơn).

## Request

*Yêu cầu định dạng Content-Type: `multipart/form-data`*

```json
{
  "file": "file",
  "financialAccountId": "guid | null",
  "bankCode": "string | null",
  "layout": "string | null",
  "runOcr": "boolean",
  "includeDebug": "boolean"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| file | file | ✅ | Tệp tin sao kê (CSV/Excel) hoặc ảnh hóa đơn cần tải lên |
| financialAccountId | guid hay null | ❌ | Nguồn tiền liên kết để nạp giao dịch |
| bankCode | string hay null | ❌ | Mã ngân hàng để nhận diện cấu trúc sao kê |
| layout | string hay null | ❌ | Bố cục hóa đơn để nhận diện OCR |
| runOcr | boolean | ✅ | Bật cờ chạy nhận diện ký tự quang học (OCR) |
| includeDebug | boolean | ❌ | Bật cờ lấy thêm thông tin gỡ lỗi |

## Response

```json
{
  "id": "guid",
  "financialAccountId": "guid",
  "status": "Pending | AwaitingReview | Processing | Completed | Failed",
  "message": "string",
  "fileName": "string",
  "originalFileName": "string",
  "storedFilePath": "string",
  "contentType": "string | null",
  "sizeInBytes": "long",
  "ocrJsonFileName": "string | null",
  "storedOcrJsonPath": "string | null",
  "receipt": {
    "isSuccess": "boolean",
    "totalAmount": "decimal | null",
    "totalRawText": "string | null",
    "transactionDate": "datetimeOffset | null",
    "transactionDateRawText": "string | null",
    "merchantName": "string | null",
    "suggestedCategoryId": "guid | null",
    "suggestedCategoryName": "string | null",
    "categoryMatchedBy": "string | null",
    "warnings": ["string"]
  },
  "preview": {
    "id": "string",
    "status": "success | uploaded",
    "imageUrl": "string",
    "transaction": {
      "merchantName": "string | null",
      "amount": "decimal | null",
      "date": "datetimeOffset | null",
      "type": "Expense",
      "suggestedCategoryId": "guid | null",
      "suggestedCategoryName": "string | null",
      "matchedBy": "string | null",
      "note": "string | null"
    },
    "items": [
      {
        "name": "string",
        "amount": "decimal | null"
      }
    ],
    "summary": {
      "subtotal": "decimal | null",
      "discount": "decimal | null",
      "total": "decimal | null"
    },
    "warnings": ["string"]
  },
  "rawOcrJson": "string | null",
  "ocrResult": {
    "isSuccess": "boolean",
    "text": "string | null",
    "layout": "string | null",
    "engine": "string",
    "executionTimeMs": "long",
    "rawResponse": "string | null"
  }
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | ID của tiến trình nhập dữ liệu vừa khởi tạo |
| status | string | Trạng thái ban đầu của tiến trình |
| receipt | object | Kết quả phân tích hóa đơn (nếu chạy OCR) |
| preview | object | Dữ liệu xem trước của giao dịch được trích xuất |

## Luồng xử lý

1. `ImportController` tiếp nhận tệp tin và các tham số thông qua định dạng form-data.
2. Service thực hiện xác thực và lưu trữ tệp tin tạm thời vào thư mục upload.
3. Nếu cờ `runOcr = true`, hệ thống gửi yêu cầu trích xuất thông tin sang dịch vụ OCR.
4. Chuyển đổi kết quả trích xuất thành các bản ghi giao dịch nháp (drafts) mà không tạo giao dịch chính thức.
5. Trả về thông tin xem trước và kết quả phân tích để người dùng kiểm duyệt.

## Quy tắc nghiệp vụ

- **Ownership**: Tiến trình nhập và tài khoản liên kết phải được xác minh thuộc quyền sở hữu của người dùng hiện tại.
- **Validation**: Định dạng tệp tin tải lên phải hợp lệ, kích thước nằm trong giới hạn cho phép.
- **Side effects**: Thêm mới bản ghi vào bảng `import_jobs`, lưu trữ tệp tin vật lý trên máy chủ.
- **Security**: Thực hiện các biện pháp chống tấn công tải tệp nguy hiểm (file upload vulnerability).

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 415 | UNSUPPORTED_MEDIA_TYPE | Định dạng tệp tải lên không được hệ thống hỗ trợ |
| 422 | VALIDATION_FAILED | Kích thước tệp vượt hạn mức hoặc dữ liệu tài khoản liên kết sai |

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
| DRIFT-009 | Route tạo tiến trình nhập `/api/v1/imports` và route tải hóa đơn `/api/v1/imports/image` đang bị chồng chéo chức năng | Cần phân tách rõ ràng mục đích sử dụng ở Front-End |

## Checklist

- [ ] Lưu trữ tệp tin an toàn tại thư mục upload đã cấu hình.
- [ ] Trả về thông tin xem trước (preview) chính xác để người dùng kiểm duyệt.
- [ ] Chặn các mối nguy hại về bảo mật tệp tin tải lên.
