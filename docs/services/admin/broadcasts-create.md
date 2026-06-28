# Admin - Broadcasts Create

## Endpoint

- Method + route: `POST /api/v1/admin/broadcasts`
- Auth: Bearer Admin
- Status thành công theo contract: create nên cần chốt `201`; API V2 hiện `200`

## Mục đích

Admin tạo broadcast notification, gửi ngay hoặc queue theo `scheduledAt`.

## Request/Response

- Request DTO: `broadcast.Request.BroadcastsRequest`
- Response DTO: `broadcast.Response.BroadcastsResponse`
- Body quan trọng: `title`, `body`, `targetAudience`, `scheduledAt`

## FE Contract

### Request FE sends

```json
{
  "auth": "Bearer Admin",
  "body": {
    "title": "string",
    "body": "string",
    "targetAudience": "string",
    "scheduledAt": "datetimeOffset | null"
  }
}
```

### Response FE receives

```json
{
  "status": 200,
  "body": {
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
}
```

### FE usage notes

- FE gửi `Authorization: Bearer <accessToken>` nếu Endpoint section ghi Bearer.
- Field JSON dùng camelCase theo `docs/conventions.md`.
- Khi response lỗi, FE đọc error envelope chuẩn trong `docs/conventions.md` và drift register nếu endpoint này đang lệch contract.

## Luồng xử lý service

1. Controller gọi `broadcast.IService.CreateBroadcast`.
2. Service validate title/body/audience/schedule.
3. Insert `Broadcast` với status queued/sent.
4. Fanout notification ngay hoặc để background dispatcher xử lý.

## File liên quan

- Controller: `Api/Controllers/AdminBroadcastController.cs`
- Service: `Service/broadcast/IService.cs`, `Service.cs`
- DTO: `Service/broadcast/Request.cs`, `Response.cs`
- Background: `Api/Jobs/BroadcastDispatchBackgroundService.cs`
- Entity/schema: `Broadcast`, `Notification`
- Docs gốc: `docs/API V2.md`, `docs/flow.md`

## Ownership, Validation, Side Effects

- Ownership: Admin policy.
- Validation: content required, schedule valid.
- DB side effects: insert broadcast, maybe insert notifications.
- Security: no raw secret in body.

## Drift hiện tại

- Contract mong muốn: broadcast fanout boundary rõ.
- Implementation hiện tại: background dispatcher exists.
- Drift/backlog: status code DRIFT-011; audit log policy.

## Acceptance Checklist

- [ ] Non-admin bị 403.
- [ ] Scheduled broadcast không gửi sớm.
- [ ] Delivered count/status consistent.