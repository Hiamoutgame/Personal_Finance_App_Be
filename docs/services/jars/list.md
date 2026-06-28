# Jars — List

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /api/v1/jars` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Trả về thông tin tóm tắt về thiết lập hũ (jar setup summary) và danh sách tất cả các hũ ngân sách thuộc về người dùng hiện tại.

## Request

*Không yêu cầu Request Body*

## Response

```json
{
  "methodType": "string",
  "totalJarBalance": "decimal",
  "unallocatedBalance": "decimal",
  "data": [
    {
      "id": "guid",
      "name": "string",
      "balance": "decimal",
      "color": "string",
      "icon": "string",
      "status": "string"
    }
  ]
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| methodType | string | Phương pháp quản lý (ví dụ: SixJars, Rule503020) |
| totalJarBalance | decimal | Tổng số dư hiện có trong toàn bộ các hũ |
| unallocatedBalance | decimal | Số dư chưa được phân bổ vào các hũ |
| data | mảng object | Danh sách chi tiết thông tin các hũ |

## Luồng xử lý

1. `JarController.GetJar` tiếp nhận yêu cầu và gọi đến `Jar.IService.GetJar`.
2. Dịch vụ phân tách ID của người dùng từ token.
3. Tiến hành truy vấn đối tượng `JarSetup`, danh sách các hũ của user và tính toán lại các số dư tổng hợp.
4. Trả đối tượng DTO kết quả về cho FE.

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ lọc và trả về danh sách các hũ của người dùng hiện tại.
- **Validation**: Đảm bảo token truy cập hợp lệ.
- **Side effects**: Không tác động làm thay đổi dữ liệu trong cơ sở dữ liệu.
- **Security**: Tuyệt đối không trả về thông tin hũ của các người dùng khác.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực thiếu, hết hạn hoặc không hợp lệ |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/JarController.cs` |
| Service | `Personal_Finance_Management.Service/Jar/Service.cs` |
| DTO | `Personal_Finance_Management.Service/Jar/Response.cs` |
| Entity | `Jar`, `JarSetup`, `FinancialAccount` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-JAR-001 | Thông tin trả về chưa hoàn toàn đồng nhất về `status` của mỗi hũ so với hợp đồng mong muốn | FE có thể cần xử lý fallback nếu không có thuộc tính trạng thái |

## Checklist

- [ ] Tổng số dư phải được tính toán chính xác đối chiếu theo tài khoản của người dùng.
- [ ] Không làm lộ các hũ đã bị lưu trữ (archived) nếu hợp đồng yêu cầu chỉ lấy hũ đang hoạt động (active-only).
- [ ] Trạng thái (status) được cung cấp đúng như mong đợi.
