# Admin - Categories Update

## Endpoint

- Method + route: `PATCH /api/v1/admin/categories/{id}`
- Auth: Bearer Admin
- Status thành công: `200 OK`

## Mục đích

Cập nhật default category hệ thống.

## Request/Response

- Request DTO: `category.Request.UpdateAdminCategoryRequest`
- Response DTO: `category.Response.AdminCategoryResponse`
- Route: `id`
- Body quan trọng: `name`, `icon`, `color`, `order`, `isActive`

## FE Contract

### Request FE sends

```json
{
  "auth": "Bearer Admin",
  "route": {
    "id": "guid"
  },
  "body": {
    "name": "string | null",
    "icon": "string | null",
    "color": "string | null",
    "order": "int | null",
    "isActive": "boolean | null"
  }
}
```

### Response FE receives

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "name": "string",
    "icon": "string | null",
    "color": "string | null",
    "order": "int",
    "isActive": "boolean"
  }
}
```

### FE usage notes

- FE gửi `Authorization: Bearer <accessToken>` nếu Endpoint section ghi Bearer.
- Field JSON dùng camelCase theo `docs/conventions.md`.
- Khi response lỗi, FE đọc error envelope chuẩn trong `docs/conventions.md` và drift register nếu endpoint này đang lệch contract.

## Luồng xử lý service

1. Controller gọi `UpdateCategory(id, request)`.
2. Service load default category.
3. Validate fields/order.
4. Update category.

## File liên quan

- Controller: `AdminCategoryController.cs`
- Service: `Service/category/*`
- DTO: `Service/category/Request.cs`, `Response.cs`
- Entity/schema: `Category`
- Docs gốc: `docs/API V2.md`

## Ownership, Validation, Side Effects

- Ownership: Admin policy.
- Validation: cannot update user custom category via admin default endpoint nếu contract cấm.
- DB side effects: update category.
- Security: admin action audit-sensitive.

## Drift hiện tại

- Contract mong muốn: admin manages default categories.
- Implementation hiện tại: endpoint exists.
- Drift/backlog: audit log policy cần chốt.

## Acceptance Checklist

- [ ] Non-admin bị 403.
- [ ] Default category only.
- [ ] Transaction/category references remain valid.