# Admin - Change Role

## Endpoint

- Method + route: `PATCH /api/v1/change-role/{accountId}`
- Auth: Bearer Admin
- Status thành công: `200 OK`

## Mục đích

Cho admin đổi role account. Endpoint này là exception hiện tại vì không nằm dưới `/api/v1/admin/...`.

## Request/Response

- Request DTO: route `accountId`, query/body `role`
- Response DTO: string message
- Role: `AccountRole`

## FE Contract

### Request FE sends

```json
{
  "auth": "Bearer Admin",
  "route": {
    "accountId": "guid"
  },
  "queryOrBody": {
    "role": "AccountRole"
  }
}
```

### Response FE receives

```json
{
  "status": 200,
  "body": "string"
}
```

### FE usage notes

- FE gửi `Authorization: Bearer <accessToken>` nếu Endpoint section ghi Bearer.
- Field JSON dùng camelCase theo `docs/conventions.md`.
- Khi response lỗi, FE đọc error envelope chuẩn trong `docs/conventions.md` và drift register nếu endpoint này đang lệch contract.

## Luồng xử lý service

1. `AdminChangeRoleController` gọi `admin.IService.UpdateRole`.
2. Service load account và role target.
3. Update account role.
4. Trả message.

## File liên quan

- Controller: `Api/Controllers/AdminChangeRoleController.cs`
- Service: `Service/admin/IService.cs`, `Service.cs`
- DTO/Enum: `Repository/Enum/AccountRole.cs`
- Entity/schema: `Account`, `Role`
- Docs gốc: `docs/API V2.md`, `docs/conventions.md`

## Ownership, Validation, Side Effects

- Ownership: Admin policy.
- Validation: role hợp lệ; cần protect last admin nếu business rule có.
- DB side effects: update account role.
- Security: action audit-sensitive, nên cần audit log nếu refactor.

## Drift hiện tại

- Contract mong muốn: admin endpoints dưới `/api/v1/admin/...`.
- Implementation hiện tại: route `/api/v1/change-role/{accountId}`.
- Drift/backlog: DRIFT-007.

## Acceptance Checklist

- [ ] Non-admin bị 403.
- [ ] Role hợp lệ.
- [ ] Route exception được document nếu giữ.