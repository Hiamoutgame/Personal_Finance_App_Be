# Health — DB Render

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /health/db/render` |
| Auth | Public |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` (Lỗi trả về `500 Internal Server Error`) |

## Mục đích

Kiểm tra trạng thái kết nối tới cơ sở dữ liệu trên máy chủ Render (Postgres target) nhằm phục vụ cho quá trình gỡ lỗi (debug) khi triển khai (deploy).

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
| status | string | Trạng thái kết nối |
| target | string | Tên máy chủ đích kết nối |
| database | string | Tên cơ sở dữ liệu Render |
| environment | string | Môi trường hệ thống |

*(Lưu ý: Mẫu JSON Response cũ đã bị sao chép nhầm dữ liệu về hũ/jar, bản này đã được chuẩn hóa lại)*

## Luồng xử lý

1. `HealthController` cấu hình để ưu tiên chọn kết nối tới máy chủ Render.
2. Thử nghiệm mở kết nối và thực thi lệnh truy vấn chẩn đoán.
3. Phản hồi thông tin trạng thái thành công hoặc thông báo lỗi đã được khử nhạy cảm (sanitized).

## Quy tắc nghiệp vụ

- **Ownership**: Không áp dụng.
- **Validation**: Không áp dụng.
- **Side effects**: Chỉ thực hiện đọc và kết nối kiểm tra.
- **Security**: Tuyệt đối không trả về chuỗi kết nối gốc (raw Render connection string).

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 500 | DATABASE_CONNECTION_FAILED | Không thể kết nối tới cơ sở dữ liệu trên Render |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/HealthController.cs` |
| Config | Khóa cấu hình `ConnectionStrings:RenderConnection` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-HEALTH-002 | Việc hiển thị công khai endpoint chẩn đoán này trên Production có thể gây rủi ro | Cần rà soát lại chính sách bảo mật trước khi vận hành chính thức (production exposure) |

## Checklist

- [ ] Không xuất bất kỳ chuỗi cấu hình bí mật nào.
- [ ] Phân biệt rõ ràng giữa môi trường Local và Render.
- [ ] Hỗ trợ chẩn đoán chính xác quá trình deploy.
