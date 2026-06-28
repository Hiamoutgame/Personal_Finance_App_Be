# Imports — List

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /api/v1/imports` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Trả về danh sách các tiến trình nhập dữ liệu (import jobs) của người dùng hiện tại, hỗ trợ theo dõi lịch sử và tiến độ xử lý file.

## Request

*Truyền qua tham số query (Query Params)*

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| page | int | ❌ | Số trang dữ liệu muốn lấy (mặc định 1) |
| pageSize | int | ❌ | Số lượng tiến trình trên một trang (mặc định 20, tối đa 100) |
| status | string hay null | ❌ | Lọc theo trạng thái của tiến trình nhập |
| financialAccountId | guid hay null | ❌ | Lọc theo nguồn tiền liên quan |

## Response

```json
{
  "data": [
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
      "updatedAt": "datetimeOffset"
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
| data | mảng object | Danh sách các tiến trình nhập của người dùng |
| pagination | object | Thông tin cấu trúc phân trang |

## Luồng xử lý

1. `ImportController` gọi đến dịch vụ `GetImports(request)`.
2. Service lấy ID người dùng hiện tại từ thông tin JWT.
3. Truy vấn các bản ghi trong bảng `ImportJob` được lọc theo ID người dùng và các tham số trạng thái/tài khoản tương ứng.
4. Trả về cấu trúc phản hồi kèm thông tin phân trang cho Front-End.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ trả về danh sách các tiến trình thuộc sở hữu của người dùng hiện tại.
- **Validation**: Kiểm tra tính hợp lệ của các tham số lọc trạng thái và phân trang.
- **Side effects**: Không tác động làm thay đổi dữ liệu trong cơ sở dữ liệu.
- **Security**: Không để lộ đường dẫn tệp tin thực tế trên máy chủ nội bộ.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực hết hạn hoặc không hợp lệ |
| 422 | VALIDATION_FAILED | Các tham số phân trang truyền lên bị sai định dạng |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/ImportController.cs` |
| Service | `Personal_Finance_Management.Service/import/Service.cs` |
| DTO | `Personal_Finance_Management.Service/import/Request.cs`, `Response.cs` |
| Entity | `ImportJob`, `FinancialAccount` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-005 | Cấu trúc phân trang hiện tại đang dùng thuộc tính `page` thay vì `pageIndex` đồng bộ chung | FE xử lý theo cấu trúc thực tế của response |

## Checklist

- [ ] Chỉ hiển thị các tiến trình nhập dữ liệu thuộc sở hữu của người dùng đang đăng nhập.
- [ ] Lọc trạng thái và phân trang hoạt động chính xác.
- [ ] Không để lộ cấu trúc tệp tin hệ thống.
