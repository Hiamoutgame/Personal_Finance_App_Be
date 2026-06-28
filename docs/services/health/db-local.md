# Health — DB Local

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /health/db/local` |
| Auth | Public |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` (Lỗi trả về `500 Internal Server Error`) |

## Mục đích

Kiểm tra trạng thái kết nối tới cơ sở dữ liệu cục bộ hoặc mặc định (local/default DB) phục vụ cho việc chẩn đoán trong quá trình phát triển (dev diagnostics).

## Request

*Không yêu cầu Request Body*

## Response

```json
{
  "status": "string",
  "target": "string",
  "database": "string",
  "environment": "string"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| status | string | Trạng thái kết nối (ví dụ: Healthy) |
| target | string | Máy chủ đích kết nối |
| database | string | Tên cơ sở dữ liệu |
| environment | string | Môi trường hoạt động |

## Luồng xử lý

1. `HealthController` đọc chuỗi kết nối cục bộ/mặc định từ cấu hình.
2. Thực hiện thử nghiệm kết nối và chạy truy vấn kiểm tra tới DB.
3. Phản hồi trạng thái thành công hoặc thông tin lỗi đã được loại bỏ các thông tin nhạy cảm.

## Quy tắc nghiệp vụ

- **Ownership**: Không áp dụng.
- **Validation**: Không áp dụng.
- **Side effects**: Chỉ thực hiện đọc/kết nối thử nghiệm, không ghi dữ liệu.
- **Security**: Tuyệt đối không trả về chuỗi kết nối đầy đủ (`connection string`) hoặc mật khẩu DB.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 500 | DATABASE_CONNECTION_FAILED | Kết nối tới cơ sở dữ liệu cục bộ thất bại |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/HealthController.cs` |
| Config | `Personal_Finance_Management.Api/appsettings.json`, `appsettings.Development.json` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-HEALTH-001 | Nên xem xét bảo vệ hoặc tắt endpoint chẩn đoán này trên môi trường Production | Tránh lộ lọt thông tin môi trường phát triển |

## Checklist

- [ ] Không rò rỉ chuỗi kết nối hoặc thông tin nhạy cảm.
- [ ] Hành vi quá thời gian kết nối (timeout) hoạt động đúng.
- [ ] Cho phép truy cập công khai trên môi trường phát triển hiện tại.
