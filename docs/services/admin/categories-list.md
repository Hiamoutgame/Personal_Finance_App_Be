# Admin - Categories List

## Endpoint

- Method + route: `GET /api/v1/admin/categories`
- Auth: Bearer Admin
- Status thành công: `200 OK`

## Mục đích

Cho admin xem default categories hệ thống.

## Request/Response

- Request DTO: query `isActive`
- Response DTO: `category.Response.AdminCategoriesResponse`

## FE Contract

### Request FE sends

```json
{
  "auth": "Bearer Admin",
  "query": {
    "isActive": "boolean | null"
  },
  "body": null
}
```

### Response FE receives

```json
{
  "status": 200,
  "body": {
    "data": [
      {
        "id": "guid",
        "name": "string",
        "icon": "string | null",
        "color": "string | null",
        "order": "int",
        "isActive": "boolean"
      }
    ]
  }
}
```

### FE usage notes

- FE gửi `Authorization: Bearer <accessToken>` nếu Endpoint section ghi Bearer.
- Field JSON dùng camelCase theo `docs/conventions.md`.
- Khi response lỗi, FE đọc error envelope chuẩn trong `docs/conventions.md` và drift register nếu endpoint này đang lệch contract.

## Luồng xử lý service

1. `AdminCategoryController.GetCategories` gọi category service.
2. Service query default/admin-managed categories.
3. Filter active nếu có.
4. Trả response.

## File liên quan

- Controller: `Api/Controllers/AdminCategoryController.cs`
- Service: `Service/category/*`
- DTO: `Service/category/Response.cs`
- Entity/schema: `Category`
- Docs gốc: `docs/API V2.md`

## Ownership, Validation, Side Effects

- Ownership: Admin policy.
- Validation: `isActive` optional.
- DB side effects: none.
- Security: chỉ admin quản lý default category.

## Drift hiện tại

- Contract mong muốn: admin default categories.
- Implementation hiện tại: endpoint exists.
- Drift/backlog: none known.

## Acceptance Checklist

- [ ] Non-admin bị 403.
- [ ] Default/admin categories đúng.
- [ ] Filter active đúng.