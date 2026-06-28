# Admin - Broadcasts List

## Endpoint

- Method + route: `GET /api/v1/admin/broadcasts`
- Auth: Bearer Admin
- Status thành công: `200 OK`

## Mục đích

Cho admin xem danh sách broadcasts và trạng thái fanout.

## Request/Response

- Request DTO: direct params `pageIndex`, `pageSize`, `status`
- Response DTO: `Page<broadcast.Response.BroadcastsResponse>`
- Query quan trọng: `pageIndex`, `pageSize`, `status`

## FE Contract

### Request FE sends

```json
{
  "auth": "Bearer Admin",
  "query": {
    "pageIndex": "int",
    "pageSize": "int",
    "status": "string"
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
        "title": "string",
        "body": "string",
        "targetAudience": "string",
        "status": "string",
        "scheduledAt": "datetimeOffset | null",
        "sentAt": "datetimeOffset | null",
        "targetCount": "int",
        "deliveredCount": "int"
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

1. Controller gọi `GetBroadcasts`.
2. Service validate paging/status.
3. Query broadcasts.
4. Trả page response.

## File liên quan

- Controller: `AdminBroadcastController.cs`
- Service: `Service/broadcast/*`
- DTO: `Service/broadcast/Response.cs`
- Entity/schema: `Broadcast`
- Docs gốc: `docs/API V2.md`

## Ownership, Validation, Side Effects

- Ownership: Admin policy.
- Validation: broadcast status enum, pagination.
- DB side effects: none.
- Security: admin-only.

## Drift hiện tại

- Contract mong muốn: paginated admin list.
- Implementation hiện tại: uses legacy `Page`.
- Drift/backlog: DRIFT-005 pagination.

## Acceptance Checklist

- [ ] Non-admin bị 403.
- [ ] Status filter đúng.
- [ ] Counts/status fanout đúng.