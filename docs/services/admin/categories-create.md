# Admin - Categories Create

## Endpoint

- Method + route: `POST /api/v1/admin/categories`
- Auth: Bearer Admin
- Status thành công theo contract: `201 Created`

## Mục đích

Tạo default category hệ thống.

## Request/Response

- Request DTO: `category.Request.CreateAdminCategoryRequest`
- Response DTO: `category.Response.AdminCategoryResponse`
- Body quan trọng: `name`, `icon`, `color`, `order`

## FE Contract

### Request FE sends

```json
{
  "auth": "Bearer Admin",
  "body": {
    "name": "string",
    "icon": "string | null",
    "color": "string | null",
    "order": "int"
  }
}
```

### Response FE receives

```json
{
  "status": 201,
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

1. Controller gọi `CreateCategory`.
2. Service validate admin category fields.
3. Insert category `IsDefault=true`, owner null.
4. Trả DTO.

## File liên quan

- Controller: `AdminCategoryController.cs`
- Service: `Service/category/*`
- DTO: `Service/category/Request.cs`, `Response.cs`
- Entity/schema: `Category`
- Docs gốc: `docs/API V2.md`

## Ownership, Validation, Side Effects

- Ownership: Admin policy.
- Validation: name/order unique/range nếu service enforce.
- DB side effects: insert default category.
- Security: no user owner id.

## Drift hiện tại

- Contract mong muốn: create 201.
- Implementation hiện tại: can verify controller status.
- Drift/backlog: DRIFT-011 if not 201.

## Acceptance Checklist

- [ ] Category default/owner null.
- [ ] Non-admin bị 403.
- [ ] Response public DTO.