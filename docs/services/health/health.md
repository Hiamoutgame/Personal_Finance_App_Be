# Health — Basic

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /health` |
| Auth | Public |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Đóng vai trò là công cụ kiểm tra sức khỏe cơ bản (health probe) cho các công cụ giám sát, ops và dev. Đây không phải là một API nghiệp vụ (business API) và là một ngoại lệ nằm ngoài phân cấp `/api/v1`.

## Request

*Không yêu cầu Request Body*

## Response

```json
{
  "status": "string"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| status | string | Trạng thái ứng dụng hiện tại (ví dụ: "Healthy") |

## Luồng xử lý

1. Bộ điều khiển `HealthController.Get` trực tiếp trả về trạng thái.
2. Không gọi hoặc phụ thuộc vào bất kỳ tầng dịch vụ (service layer) hay cơ sở dữ liệu nào riêng biệt, đảm bảo phản hồi tức thì.

## Quy tắc nghiệp vụ

- **Ownership**: Không áp dụng.
- **Validation**: Không áp dụng.
- **Side effects**: Không có bất kỳ thay đổi nào.
- **Security**: Không để lộ bất kỳ thông tin cấu hình nội bộ (config/secret).

## Lỗi

*Endpoint này thường luôn phản hồi thành công hoặc bị ngắt kết nối trực tiếp (timeout) nếu server chết.*

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/HealthController.cs` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-HEALTH-003 | Không có sai lệch hiện tại | Endpoint hoạt động bình thường như một tiện ích hệ thống |

## Checklist

- [ ] Hoàn toàn không trả về các cấu hình bảo mật hoặc thông tin hệ thống nội bộ.
- [ ] Tốc độ phản hồi nhanh (Fast response).
- [ ] Hữu ích để các dịch vụ cloud thực hiện lệnh `uptime probe` duy trì trạng thái hoạt động.
