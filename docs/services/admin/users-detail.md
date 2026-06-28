# Admin - User Detail

## Endpoint

- Method + route: `GET /api/v1/admin/users/{id}`
- Auth: Bearer Admin
- Status thanh cong: `200 OK`

## Muc dich

Cho admin xem chi tiet mot user.

## Request/Response

- Request DTO: `User.Request.UserIdRequest` hoac route id
- Response DTO: `User.Response.AdminUserResponse`
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

- FE gui `Authorization: Bearer <accessToken>` neu Endpoint section ghi Bearer.
- Field JSON dung camelCase theo `docs/conventions.md`.
- Khi response loi, FE doc error envelope chuan trong `docs/conventions.md` va drift register neu endpoint nay dang lech contract.

## Luong xu ly service

1. Controller goi `GetUserById(id)`.
2. Service validate admin va load account.
3. Map public admin user fields.

## File lien quan

- Controller: `AdminUserController.cs`
- Service: `Service/User/*`
- DTO: `Service/User/Request.cs`, `Response.cs`
- Entity/schema: `Account`, `Role`
- Docs goc: `docs/API V2.md`

## Ownership, Validation, Side Effects

- Ownership: Admin policy.
- Validation: id GUID, user exists.
- DB side effects: none.
- Security: khong expose password hash/token/secret.

## Drift hien tai

- Contract mong muon: admin detail.
- Implementation hien tai: endpoint exists.
- Drift/backlog: none known.

## Acceptance Checklist

- [ ] Admin-only.
- [ ] User not found tra 404.
- [ ] Sensitive fields hidden.
