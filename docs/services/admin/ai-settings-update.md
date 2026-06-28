# Admin - AI Settings Update

## Endpoint

- Method + route: `PATCH /api/v1/admin/ai-settings`
- Auth: Bearer Admin
- Status thành công: `200 OK`

## Mục đích

Cập nhật cấu hình AI an toàn cho toàn hệ thống.

## Request/Response

- Request DTO: `AI.Request.UpdateAiSettingsRequest`
- Response DTO: `AI.Response.UpdateAiSettingsResponse`
- Body quan trọng: `modelName`, `systemPrompt`, `temperature`, `maxTokens`, `isEnabled`

## FE Contract

### Request FE sends

```json
{
  "auth": "Bearer Admin",
  "body": {
    "modelName": "string | null",
    "systemPrompt": "string | null",
    "temperature": "decimal | null",
    "maxTokens": "int | null",
    "isEnabled": "boolean | null"
  }
}
```

### Response FE receives

```json
{
  "status": 200,
  "body": {
    "modelName": "string",
    "isEnabled": "boolean"
  }
}
```

### FE usage notes

- FE gửi `Authorization: Bearer <accessToken>` nếu Endpoint section ghi Bearer.
- Field JSON dùng camelCase theo `docs/conventions.md`.
- Khi response lỗi, FE đọc error envelope chuẩn trong `docs/conventions.md` và drift register nếu endpoint này đang lệch contract.

## Luồng xử lý service

1. Controller gọi `AI.IService.UpdateAdminAiSettings`.
2. Service validate ranges/model/prompt.
3. Update `AiSetting`.
4. Trả response gọn.

## File liên quan

- Controller: `AdminAISettingController.cs`
- Service: `Service/ai/*`
- DTO: `Service/ai/Request.cs`, `Response.cs`
- Entity/schema: `AiSetting`
- Docs gốc: `docs/API V2.md`

## Ownership, Validation, Side Effects

- Ownership: Admin policy.
- Validation: temperature/maxTokens/modelName.
- DB side effects: update AI settings.
- Security: DTO hiện tại không nhận raw API key; nếu sau này thêm phải encrypt/mask.

## Drift hiện tại

- Contract mong muốn: no secret exposure.
- Implementation hiện tại: endpoint exists.
- Drift/backlog: audit log policy cho config change.

## Acceptance Checklist

- [ ] Non-admin bị 403.
- [ ] No raw secret in response/log.
- [ ] AI chat dùng setting mới.