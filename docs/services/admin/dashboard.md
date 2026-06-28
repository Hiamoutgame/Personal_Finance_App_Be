# Admin - Dashboard

## Endpoint

- Method + route: `GET /api/v1/admin/dashboard`
- Auth: Bearer Admin
- Status thành công: `200 OK`

## Mục đích

Trả dashboard vận hành cho admin: users, transactions, jars, goals, imports.

## Request/Response

- Request DTO: none
- Response DTO: `admin.Response.AdminDashboardResponse`
- Response quan trọng: summary, recent users, recent transactions

## FE Contract

### Request FE sends

```json
{
  "auth": "Bearer Admin",
  "body": null
}
```

### Response FE receives

```json
{
  "status": 200,
  "body": {
    "summary": {
      "totalUsers": "int",
      "newUsersThisMonth": "int",
      "activeUsersLast30Days": "int",
      "bannedUsers": "int",
      "totalTransactions": "int",
      "transactionsThisMonth": "int",
      "totalJars": "int",
      "activeGoals": "int",
      "pendingImportJobs": "int"
    },
    "recentUsers": [
      {
        "id": "guid",
        "username": "string",
        "firstName": "string",
        "lastName": "string",
        "email": "string",
        "status": "string",
        "isOnboardingCompleted": "boolean",
        "lastLoginAt": "datetimeOffset | null"
      }
    ],
    "recentTransactions": [
      {
        "id": "guid",
        "type": "string",
        "transactionsAmount": "decimal",
        "note": "string | null",
        "transactionDate": "datetimeOffset",
        "user": {
          "id": "guid",
          "username": "string",
          "firstName": "string",
          "lastName": "string"
        },
        "financialAccount": {
          "id": "guid",
          "name": "string",
          "accountType": "string"
        },
        "category": {
          "id": "guid",
          "name": "string"
        }
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

1. `AdminDashboardController.GetDashboard` gọi `admin.IService.GetDashboard`.
2. Service query aggregate toàn hệ thống.
3. Project recent users/transactions.
4. Trả DTO.

## File liên quan

- Controller: `Api/Controllers/AdminDashboardController.cs`
- Service: `Service/admin/IService.cs`, `Service.cs`
- DTO: `Service/admin/Response.cs`
- Entity/schema: `Account`, `Transaction`, `Jar`, `Goal`, `ImportJob`
- Docs gốc: `docs/API V2.md`

## Ownership, Validation, Side Effects

- Ownership: Admin policy.
- Validation: none/optional date range if later added.
- DB side effects: none.
- Security: hide sensitive user/provider data.

## Drift hiện tại

- Contract mong muốn: admin-only operations dashboard.
- Implementation hiện tại: endpoint exists.
- Drift/backlog: performance/N+1 audit when scale.

## Acceptance Checklist

- [ ] Non-admin bị 403.
- [ ] No secrets/passwords.
- [ ] Aggregates consistent.