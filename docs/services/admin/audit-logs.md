# Admin - Audit Logs

## Endpoint

- Method + route: `GET /api/v1/admin/audit-logs`
- Auth: Bearer Admin
- Status thành công: `200 OK`

## Mục đích

Cho admin xem audit log thao tác nhạy cảm.

## Request/Response

- Request DTO: `admin.Request.AdminAuditLogsRequest`
- Response DTO: `Page<admin.Response.AdminAuditLogItem>`
- Query quan trọng: `adminId`, `actionType`, `entityType`, `fromDate`, `toDate`, `page`, `pageSize`

## FE Contract

### Request FE sends

```json
{
  "auth": "Bearer Admin",
  "query": {
    "adminId": "guid | null",
    "actionType": "string | null",
    "entityType": "string | null",
    "fromDate": "datetimeOffset | null",
    "toDate": "datetimeOffset | null",
    "page": "int",
    "pageSize": "int"
  },
  "body": null
}
```

### Response FE receives

```json
{
  "status": 200,
  "body": {
    "items": [
      {
        "id": "guid",
        "adminUsername": "string",
        "actionType": "string",
        "entityType": "string",
        "description": "string",
        "createdAt": "datetimeOffset"
      }
    ],
    "pagination": {
      "pageIndex": "int",
      "pageSize": "int",
      "totalCount": "int",
      "totalPages": "int"
    }
  }
}
```

### FE usage notes

- FE gửi `Authorization: Bearer <accessToken>` nếu Endpoint section ghi Bearer.
- Field JSON dùng camelCase theo `docs/conventions.md`.
- Khi response lỗi, FE đọc error envelope chuẩn trong `docs/conventions.md` và drift register nếu endpoint này đang lệch contract.

## Luồng xử lý service

1. Controller gọi `admin.IService.GetAuditLogs(request)`.
2. Service validate query.
3. Query `AuditLog` và join admin username nếu cần.
4. Trả paginated response.

## File liên quan

- Controller: `Api/Controllers/AdminAuditLogController.cs`
- Service: `Service/admin/IService.cs`, `Service.cs`
- DTO: `Service/admin/Request.cs`, `Response.cs`
- Entity/schema: `AuditLog`, `Account`
- Docs gốc: `docs/API V2.md`

## Ownership, Validation, Side Effects

- Ownership: Admin policy.
- Validation: date range, page/pageSize.
- DB side effects: none.
- Security: audit details không nên chứa secret raw.

## Drift hiện tại

- Contract mong muốn: append-only audit log.
- Implementation hiện tại: endpoint exists.
- Drift/backlog: ensure sensitive actions actually write audit logs.

## Acceptance Checklist

- [ ] Non-admin bị 403.
- [ ] Pagination consistent.
- [ ] Audit details sanitized.