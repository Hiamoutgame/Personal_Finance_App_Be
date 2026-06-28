# Imports — Upload Image/OCR

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/imports/image` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Tải lên (upload) một ảnh hóa đơn (receipt) hoặc tệp tin sao kê, tự động thực thi quá trình nhận dạng ký tự quang học (OCR) nếu được yêu cầu, và trả về kết quả dự thảo (draft/preview) để người dùng rà soát trước khi thực hiện lưu thành giao dịch thật.

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
| file | file | ✅ | Tệp tin ảnh hóa đơn cần phân tích |
| financialAccountId | guid hay null | ❌ | Tài khoản nguồn tiền để liên kết dự thảo |
| bankCode | string hay null | ❌ | Mã ngân hàng nhận diện (nếu có) |
| layout | string hay null | ❌ | Loại biểu mẫu/bố cục của hình ảnh để nhận diện OCR |
| runOcr | boolean | ✅ | Bật cờ chạy nhận diện ký tự quang học |
| includeDebug | boolean | ❌ | Bật cờ lấy thêm thông tin gỡ lỗi từ OCR |

## Response

```json
{
  "id": "guid",
  "financialAccountId": "guid",
  "status": "Pending | AwaitingReview | Processing | Completed | Failed",
  "message": "string",
  "fileName": "string",
  "originalFileName": "string",
  "contentType": "string | null",
  "sizeInBytes": "long",
  "receipt": {
    "isSuccess": "boolean",
    "totalAmount": "decimal | null",
    "transactionDate": "datetimeOffset | null",
    "merchantName": "string | null",
    "suggestedCategoryId": "guid | null",
    "suggestedCategoryName": "string | null",
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
      "note": "string | null"
    },
    "warnings": ["string"]
  },
  "rawOcrJson": "string | null"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | ID của tiến trình nhập dữ liệu (Import Job) |
| status | string | Trạng thái xử lý tiến trình |
| receipt | object | Kết quả phân tích biên lai thu được từ tiến trình OCR |
| preview | object | Dữ liệu xem trước của giao dịch để FE hiển thị |
| rawOcrJson | string hay null | Toàn bộ dữ liệu phản hồi thô từ hệ thống OCR (dùng để gỡ lỗi) |

## Luồng xử lý

1. `ImportController` tiếp nhận tệp tin và các tham số gửi qua định dạng multipart/form-data.
2. Dịch vụ xác nhận tính hợp lệ của tệp tin tải lên (kích thước, định dạng) và thực hiện lưu trữ an toàn.
3. Nếu cờ `runOcr=true` được bật, dịch vụ gửi gọi lên OCR service.
4. Trình phân tách (Parser) của hệ thống tiếp nhận kết quả từ OCR, ánh xạ thành thông tin bản nháp giao dịch (draft data), đồng thời chưa tạo ngay giao dịch thực tế trên cơ sở dữ liệu.
5. Tổng hợp thông tin bản nháp để trả về kết quả xem trước cho phía client.

## Quy tắc nghiệp vụ

- **Ownership**: Tệp tin hình ảnh tải lên và tiến trình nhập dữ liệu phải trực thuộc quyền sở hữu của người dùng hiện tại đang đăng nhập.
- **Validation**: Đảm bảo tệp tin hợp lệ về định dạng tệp ảnh, độ lớn và kiểm tra các tính hợp lệ tham số kèm theo.
- **Side effects**: Thêm bản ghi dữ liệu mới cho quá trình OCR vào bảng `import_jobs`, đồng thời lưu trữ tệp tin trên máy chủ phân vùng cho người dùng.
- **Security**: Đảm bảo sử dụng các biện pháp làm sạch dữ liệu đầu vào nhằm ngăn chặn tệp độc hại và không hiển thị đường dẫn gốc của tệp tin.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 415 | UNSUPPORTED_MEDIA_TYPE | Định dạng tệp tin tải lên không được hệ thống hỗ trợ |
| 422 | VALIDATION_FAILED | Kích thước tệp vượt hạn mức |
| 500 | OCR_SERVICE_ERROR | Quá trình nhận diện OCR bị lỗi kết nối |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/ImportController.cs` |
| Service | `Personal_Finance_Management.Service/import/Service.cs` |
| DTO | `Personal_Finance_Management.Service/import/Request.cs`, `Response.cs` |
| Storage | Thư mục lưu trữ hình ảnh trên máy chủ |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-009 | Route tải ảnh (`/api/v1/imports/image`) có chức năng bị trùng lặp nhiều phần đối với route tạo sao kê (`/api/v1/imports`) | Nên xác định rõ mục tiêu phân cấp API để FE sử dụng tách biệt nếu cần thiết |

## Checklist

- [ ] Thông tin nhận diện từ hóa đơn không tự động tạo ra giao dịch chính thức cho đến khi được duyệt.
- [ ] Giao diện (FE) có thể dùng `imageUrl` để xem trước kết quả tải lên thành công.
- [ ] Tính năng OCR xử lý ổn định, có cảnh báo nếu thông tin phân tích bị mờ hoặc thiếu chi tiết.
