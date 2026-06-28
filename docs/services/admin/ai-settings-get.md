# Admin - AI Settings Get

## Endpoint

- Method + route: `GET /api/v1/admin/ai-settings`
- Auth: Bearer Admin
- Status thành công: `200 OK`

## Mục đích

Cho admin xem cấu hình AI đang áp dụng mà không expose raw API key.

## Request/Response

- Request DTO: none
- Response DTO: `AI.Response.AdminAiSettingsResponse`
- Response quan trọng: `modelName`, `systemPrompt`, `temperature`, `maxTokens`, `isEnabled`, `apiKeyMasked`

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
    "modelName": "string",
    "systemPrompt": "string",
    "temperature": "decimal",
    "maxTokens": "int",
    "isEnabled": "boolean",
    "apiKeyMasked": "string | null"
  }
}
```

### FE usage notes

- FE gửi `Authorization: Bearer <accessToken>` nếu Endpoint section ghi Bearer.
- Field JSON dùng camelCase theo `docs/conventions.md`.
- Khi response lỗi, FE đọc error envelope chuẩn trong `docs/conventions.md` và drift register nếu endpoint này đang lệch contract.

## Luồng xử lý service

1. `AdminAISettingController.GetSettings` gọi `AI.IService.GetAdminAiSettings`.
2. Service load `AiSetting`.
3. Mask API key status nếu có.
4. Trả response.

## File liên quan

- Controller: `Api/Controllers/AdminAISettingController.cs`
- Service: `Service/ai/IService.cs`, `Service.cs`
- DTO: `Service/ai/Response.cs`
- Entity/schema: `AiSetting`
- Docs gốc: `docs/API V2.md`, `docs/flow.md`

## Ownership, Validation, Side Effects

- Ownership: Admin policy.
- Validation: none.
- DB side effects: none.
- Security: không trả `ApiKeyEncrypted` hoặc raw key.

## Drift hiện tại

- Contract mong muốn: `apiKeyMasked` only.
- Implementation hiện tại: endpoint exists.
- Drift/backlog: system prompt exposure is admin-only.

## Acceptance Checklist

- [ ] Non-admin bị 403.
- [ ] Raw key không xuất hiện.
- [ ] Disabled provider status rõ.