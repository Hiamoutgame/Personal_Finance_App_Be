# Admin - Categories Delete

## Endpoint

- Method + route: `DELETE /api/v1/admin/categories/{id}`
- Auth: Bearer Admin
- Status thành công theo contract: convention ưu tiên `204`; API V2 hiện `200 + message`

## Mục đích

Deactivate/delete default category hệ thống.

## Request/Response

- Request DTO: none
- Response DTO: `category.Response.MessageResponse`
- Route: `id`

## FE Contract

### Request FE sends

```json
{
  "auth": "Bearer Admin",
  "route": {
    "id": "guid"
  },
  "body": null
}
```

### Response FE receives

```json
{
  "status": 200,
  "body": {
    "message": "string"
  }
}
```

### FE usage notes

- FE gửi `Authorization: Bearer <accessToken>` nếu Endpoint section ghi Bearer.
- Field JSON dùng camelCase theo `docs/conventions.md`.
- Khi response lỗi, FE đọc error envelope chuẩn trong `docs/conventions.md` và drift register nếu endpoint này đang lệch contract.

## Luồng xử lý service

1. Controller gọi `DeleteCategory(id)`.
2. Service load default category.
3. Soft deactivate nếu có transaction/reference.
4. Trả message.

## File liên quan

- Controller: `AdminCategoryController.cs`
- Service: `Service/category/*`
- Entity/schema: `Category`, `Transaction`
- Docs gốc: `docs/API V2.md`

## Ownership, Validation, Side Effects

- Ownership: Admin policy.
- Validation: cannot delete protected/default category nếu rule có.
- DB side effects: deactivate/delete category.
- Security: admin action audit-sensitive.

## Drift hiện tại

- Contract mong muốn: delete/deactivate semantics rõ.
- Implementation hiện tại: status code cần chốt.
- Drift/backlog: DRIFT-011.

## Acceptance Checklist

- [ ] Non-admin bị 403.
- [ ] Referenced category không phá vỡ history.
- [ ] Active filter cập nhật đúng.