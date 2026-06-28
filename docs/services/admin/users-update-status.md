# Admin - User Update Status

## Endpoint

- Method + route: `PATCH /api/v1/admin/users/{id}/status`
- Auth: Bearer Admin
- Status thành công: `200 OK`

## Mục đích

Ban/unban user bằng status explicit `Active` hoặc `Banned`.

## Request/Response

- Request DTO: `User.Request.UserStatusRequest`
- Response DTO: `User.Response.AdminUserResponse`
- Route: `id`
- Body quan trọng: `status`, `statusReason`

## FE Contract

### Request FE sends

```json
{
  "auth": "Bearer Admin",
  "route": {
    "id": "guid"
  },
  "body": {
    "status": "Active | Banned",
    "statusReason": "string | null"
  }
}
```

### Response FE receives

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "userName": "string",
    "firstName": "string",
    "lastName": "string",
    "email": "string",
    "phone": "string | null",
    "avatarUrl": "string | null",
    "preferredCurrency": "string",
    "isOnboardingCompleted": "boolean",
    "status": "string",
    "statusReason": "string | null",
    "createdAt": "datetimeOffset",
    "lastLoginAt": "datetimeOffset | null"
  }
}
```

### FE usage notes

- FE gửi `Authorization: Bearer <accessToken>` nếu Endpoint section ghi Bearer.
- Field JSON dùng camelCase theo `docs/conventions.md`.
- Khi response lỗi, FE đọc error envelope chuẩn trong `docs/conventions.md` và drift register nếu endpoint này đang lệch contract.

## Luồng xử lý service

1. Controller gọi `UpdateUserStatus(id, request)`.
2. Service validate admin request/status.
3. Load user account, update status/reason.
4. Save và trả updated user DTO.

## File liên quan

- Controller: `AdminUserController.cs`
- Service: `Service/User/*`
- DTO: `Service/User/Request.cs`, `Response.cs`
- Entity/schema: `Account`
- Docs gốc: `docs/API V2.md`, `docs/flow.md`

## Ownership, Validation, Side Effects

- Ownership: Admin policy.
- Validation: status enum, cannot self-ban/special admin rule nếu có.
- DB side effects: update account status/reason.
- Security: banned user phải bị chặn login/protected APIs theo contract.

## Drift hiện tại

- Contract mong muốn: status explicit, không toggle ngầm.
- Implementation hiện tại: endpoint exists.
- Drift/backlog: login banned enforcement can audit Auth service.

## Acceptance Checklist

- [ ] Non-admin bị 403.
- [ ] Banned user bị chặn login.
- [ ] Status reason lưu đúng.