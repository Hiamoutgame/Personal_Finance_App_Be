# Admin - Users List

## Endpoint

- Method + route: `GET /api/v1/admin/users`
- Auth: Bearer Admin
- Status thanh cong: `200 OK`

## Muc dich

Cho admin xem danh sach user co paging/filter de van hanh tai khoan.

## Request/Response

- Request DTO: `User.Request.GetAdminUsersRequest`
- Response DTO: `Base.Response.PagedResponse<User.Response.AdminUserResponse>`
- Query quan trong: `pageIndex`, `pageSize`, `status`, `keyword`

## FE Contract

### Request FE sends

```json
{
  "auth": "Bearer Admin",
  "query": {
    "pageIndex": "int",
    "pageSize": "int",
    "status": "Active | Banned | null",
    "keyword": "string | null"
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
    ],
    "pagination": {
      "page": "int",
      "pageSize": "int",
      "totalCount": "int",
      "totalPages": "int"
    }
  }
}
```

### FE usage notes

- FE gui `Authorization: Bearer <accessToken>` neu Endpoint section ghi Bearer.
- Field JSON dung camelCase theo `docs/conventions.md`.
- Khi response loi, FE doc error envelope chuan trong `docs/conventions.md` va drift register neu endpoint nay dang lech contract.

## Luong xu ly service

1. `AdminUserController.GetUsers` goi `User.IService.GetAdminUsers`.
2. Service validate admin request.
3. Query accounts role User, filter status/keyword.
4. Tra paged response.

## File lien quan

- Controller: `Api/Controllers/AdminUserController.cs`
- Service: `Service/User/IService.cs`, `Service.cs`
- DTO: `Service/User/Request.cs`, `Response.cs`
- Entity/schema: `Account`, `Role`
- Docs goc: `docs/API V2.md`, `docs/flow.md`

## Ownership, Validation, Side Effects

- Ownership: admin policy, khong phai user ownership.
- Validation: pagination/status/keyword.
- DB side effects: none.
- Security: chi Admin policy duoc goi.

## Drift hien tai

- Contract mong muon: admin route duoi `/api/v1/admin`.
- Implementation hien tai: route aligned.
- Drift/backlog: pagination envelope DRIFT-005.

## Acceptance Checklist

- [ ] Non-admin bi 403.
- [ ] Khong tra password hash/secrets.
- [ ] Pagination/filter dung.
