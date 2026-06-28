# [Module] — [Hành động]

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/...` |
| Auth | Bearer User / Admin / Public |
| Status thành công | `200 OK` / `201 Created` / `204 No Content` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Mô tả ngắn 1-2 câu về nghiệp vụ của endpoint này.

## Request

```json
{
  "field": "type"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| field | string | ✅ | Mô tả ngắn |

## Response

```json
{
  "field": "type"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| field | string | Mô tả |

## Luồng xử lý

1. Bước 1.
2. Bước 2.

## Quy tắc nghiệp vụ

- **Ownership**: ...
- **Validation**: ...
- **Side effects**: ...
- **Security**: ...

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 404 | NOT_FOUND | Tài nguyên không tồn tại hoặc không thuộc quyền sở hữu |
| 422 | VALIDATION_FAILED | Dữ liệu đầu vào không hợp lệ |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Api/Controllers/XController.cs` |
| Service | `Service/X/Service.cs` |
| DTO | `Service/X/Request.cs`, `Response.cs` |
| Entity | `X`, `Y` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-XXX | ... | ... |

## Checklist

- [ ] Nghiệp vụ hoạt động đúng mong muốn.
- [ ] Định dạng phản hồi chính xác.
