# Refactor progress note

Tham chiếu plan đầy đủ: `C:\Users\anhvi\.claude\plans\iridescent-petting-chipmunk.md`

---

## ✅ Đã hoàn thành

### Phase 1.5 — Thay Casso bằng SePay (2026-05-26)
- **Docs**: cập nhật `conventions.md` (§11 endpoint sepay, §15 dedup rule), `API V2.md` (§3 title, §11 webhook spec mới `{"success": true}`, route `/sepay/*`), `flow.md` (§7.3, §11 SePay flow), `overview.md` (line 164), `finjar_schema.sql` (default `'sepay'`).
- **Constants mới**: `ProviderCodes` (Sepay/SepayDisplay), `ConfigKeys.Sepay` (15 keys: ClientId/Secret, RedirectUri, BaseUrl, AuthorizationUrl, TokenUrl, Scope, ApiKey, WebhookApiKey, TokenEncryptionKey, TimeoutSeconds, ConnectionMode, DefaultReturnUrl, AllowedReturnUrlPrefix). Xóa `ConfigKeys.Casso`, `ConfigKeys.CasooOptions`. Đổi `IntegrationDefaults.CassoTimeoutSeconds` → `SepayTimeoutSeconds`. Rename 20 `Casso*` ErrorCodes/Messages → `Sepay*` + thêm `SepayTokenRefreshFailed`.
- **`Service/Sepay/` mới** (5 file): `SepayOptions`, `SepayModels` (TokenResponse/StoredToken/Account/TransactionRecord), `SepayTokenProtector` (AES-GCM `v1:` format), `ISepayClient` + `SepayClient` (OAuth: authorize URL, token exchange form body, **`RefreshTokenAsync` mới** — `grant_type=refresh_token`. API: `GET /bank-account`, `GET /transaction` Bank Hub).
- **Services**: `BankConnection/Service.cs` rewrite (`StartSepayConnection`, `HandleSepayCallback`, `UpsertSepayFinancialAccounts`, `provider_code = ProviderCodes.Sepay`); `BankSync/Service.cs` rewrite (`ProcessSepayWebhook` — header `Authorization: Apikey {key}`, `transferType "in"/"out"` mapping; `SyncLinkedAccountForUser` auto-refresh khi 401; bỏ `TriggerSyncAsync`). DTO: `BankConnection.Request.StartSepayConnectionRequest`, `Response.StartSepayConnectionResponse`/`SepayCallbackResponse`. `BankSync.Request.SepayWebhookRequest` (typed: id long, gateway, transactionDate, transferType, transferAmount, ...), `Response.SepayTransactionsResponse` (có `success: true`). `FinancialAccount/Service.cs:208-209` dùng `ProviderCodes.Sepay/SepayDisplay`.
- **Controllers**: `FinancialAccountController` route `/sepay/connect`, `/sepay/callback`, **mới `/sepay/webhook`**. `TransactionsController` xóa cả `GET` và `POST /Casso`. `Transaction/Service.cs` xóa `ProcessCassoWebhook` + `SyncCassoTransactions` (~534 dòng).
- **DI** (`Program.cs:109-118`): swap `CassoService` → `SepayService`, `ConfigKeys.Sepay.TimeoutSeconds` + `IntegrationDefaults.SepayTimeoutSeconds`.
- **Config**: `appsettings.json` + `appsettings.Development.json` block `Casso:*` → `Sepay:*` (URL `my.sepay.vn` + `bankhub-api.sepay.vn`, scope `bank-account:read transaction:read`, `WebhookApiKey` placeholder).
- **DB**: entity `BankConnectionSession.ProviderCode` default `"sepay"`; `AppDbContext` HasDefaultValue + snapshot updated; migration mới `20260526055205_AppendSepayProviderDefault` (`ALTER ... DEFAULT 'sepay'`, `UPDATE` rows từ `'casso'` → `'sepay'`).
- **Xóa**: folder `Service/Casso/` (5 file).

**Verification**: `dotnet build` → **0 errors**, 56 warnings (pre-existing).

Còn lại để chạy thực tế cần: đăng ký app SePay developer portal (client_id/secret), set `Sepay:WebhookApiKey`, chạy migration mới trên DB dev.

---

## ✅ Đã hoàn thành (trước đó)

### Phase 1 — Đồng bộ tài liệu
- Tạo `docs/conventions.md` (single source of truth: route prefix `api/v1`, JSON camelCase, sign convention transaction `amount > 0`, ISO-8601 UTC, pagination `{items,totalCount,page,pageSize}`, error envelope `{code,message,field?,details?,traceId}`).
- Sửa `docs/API V2.md`: thêm `Processing` vào ImportJob status, mark legacy `/Transactions/Casso` deprecated, append §11 spec cho 4 endpoint còn thiếu (jar allocate, goal contributions GET/POST, limit detail).
- Sửa `docs/flow.md`: bỏ Transfer ambiguity (v1 chỉ Income/Expense), mark Casso flow legacy.
- Sửa `docs/finjar_schema.sql`: header comment sign convention.
- Sửa `docs/overview.md`, `docs/user story.md`: link tới conventions + backlog background jobs.

### Phase 2.1.bis PR-A — Constants/Enums skeleton
- Tạo `Personal_Finance_Management.Service/Common/Constants/` (9 file):
  `ErrorCodes`, `ErrorMessages`, `Policies`, `AppClaimTypes`, `ConfigKeys`, `RoutePrefixes`+`Routes`, `SourceTypes`, `Defaults`, `UploadLimits`.
- Tạo `Personal_Finance_Management.Service/Common/Enums/` (12 file): `TransactionType`, `AccountStatus`, `JarStatus`, `ImportJobStatus`, `BudgetMethod`, `SpendingLimitPeriod`+`TargetType`, `ReminderFrequency`+`Status`, `NotificationType`, `BroadcastStatus`, `FinancialAccountType`+`ConnectionMode`+`SyncStatus`+`BankConnectionSessionStatus`, `GoalStatus`.
- Pattern: `static class` chứa `const string` + `IReadOnlySet<string> All` + `IsValid(string?)` (không dùng C# enum thật vì schema dùng VARCHAR+CHECK).

### Phase 2.1.bis PR-B — Enum literals
- Thay tất cả literal `"Income"`, `"Expense"`, `"Active"`, `"Pending"`, `"AwaitingReview"`, `"SixJars"`, `"Daily"`, `"Monthly"`... bằng constants trong: Dashboard, Limit, BankSync, ai, Transaction, import (+ Response), BankConnection, broadcast (+ Response, IService), Reminder, Jar.
- Dùng alias `using TxEnums = ...Common.Enums;` để né namespace clash với folder `Transaction`, `ServiceEnums` cho clash với `Repository.Enum.ReminderFrequency`.

### Phase 2.1.bis PR-C — Policy + Claim types
- `Api/Extensions/AuthorizationExtension.cs` delegate sang `AppPolicies` constants.
- `Auth/Service.cs`: tất cả `new Claim(...)` dùng `AppClaimTypes.*`.

### Phase 2.1.bis PR-E — Config keys
- `Program.cs`: dùng `ConfigKeys.Casso.TimeoutSeconds` cho HttpClient.
- `Jobs/BroadcastDispatchBackgroundService.cs`: dùng `ConfigKeys.BroadcastDispatch.IntervalSeconds` + `IntegrationDefaults.BroadcastDispatchIntervalSeconds`.

### Phase 2.2 — Error & Response chuẩn hoá
- Rewrite `AppValidationException`: expose `Code`, `Field` top-level; thêm factory `Unauthorized`, `Forbidden`, `ValidationFailed`.
- Rewrite `GlobalExceptionHandlerMiddleware`: envelope chuẩn `{code, message, field, details, traceId}` camelCase JSON.

### Phase 2.3 — Shared helpers
- `Service/Base/CurrentUserAccessor.cs`: `ICurrentUserAccessor.TryGetUserId/GetRequiredUserId` (đọc claim `AppClaimTypes.Id`).
- `Service/Base/PagedResult.cs`: `PagedResult<T>` + `QueryableExtensions.ToPagedAsync(page, pageSize)` dùng `PaginationDefaults`.
- `ServiceClaimHelper`: dùng `AppClaimTypes.Id`.

### Phase 2.6 — Structured logging + Correlation ID
- Add Serilog packages (`Serilog.AspNetCore` 8.0.3, sinks Console + File rolling, enrichers MachineName + ThreadId).
- `Program.cs`: cấu hình `UseSerilog` (console + `logs/app-.log` rolling 14 ngày), `UseSerilogRequestLogging`.
- New `Middlewares/CorrelationIdMiddleware.cs`: đọc/gen `X-Correlation-Id`, push `LogContext` `CorrelationId` + `UserId` cho mọi log trong scope request.
- Đăng ký middleware trước `GlobalExceptionHandlerMiddleware`.

### Phase 2.1.bis PR-D — Error codes/messages (round 1 + 2)
- Mở rộng `ErrorCodes` thêm ~50 code mới (Transaction, FinancialAccount, Goal, Limit, Reminder, Casso, Image, Import...).
- Mở rộng `ErrorMessages` thêm ~50 message (Vietnamese + English variants để tương thích code đang dùng English).
- Thay ~160 literal `throw AppValidationException.X("msg literal", field, "CODE_LITERAL")` → `ErrorMessages.X, field, ErrorCodes.X` trong 25+ service file (Auth, Reminder, Transaction, Jar, broadcast, BankConnection, BankSync, ai, import, Limit, Dashboard, Goal, Notification, ValidationServices, FinancialAccount, Casso/Client+TokenProtector, ocr, category, admin, User, Onboarding, Base/ServiceTextHelper, seeding/DatabaseSeedService, Validations/FluentValidationExtensions).

**Verification**: `dotnet build Personal_Finance_Management.sln` → 0 errors (chỉ còn pre-existing warnings về nullable + package vulnerability).

---

## 🚧 Còn lại trong plan

### Phase 2.1 — Rename folder service về PascalCase
Cơ học nhưng đụng nhiều `using` + `Program.cs` `AddScoped`:
- `admin → Admin`, `ai → Ai`, `broadcast → Broadcast`, `category → Category`, `import → Import`, `ocr → Ocr`, `goal → Goal` (đã ở `Goal/`?), `limit → Limit` (đã ở `Limit/`?), `notification → Notification`, `seeding → Seeding`, `baseServices → Base` (đã ở `Base/`).
- Đổi class trùng tên `Service.cs` sang `{Domain}Service.cs`.
- Cập nhật namespace alias trong `Program.cs`.
**Risk**: medium — conflict merge cao nếu có branch khác đang chạy.

### Phase 2.1.bis PR-F — Routes + magic numbers còn lại
- Thay `[Route("...")]` trong controllers bằng `Routes.*` constants.
- Thay magic numbers còn sót (page size, file size, OCR timeout) bằng `PaginationDefaults`, `UploadLimits`, `IntegrationDefaults`.

### Phase 2.4 — Tách fat services (risk cao)
- `Transaction/Service.cs` (1544 dòng) → `TransactionQueryService` + `TransactionCommandService` + `CassoWebhookService`.
- `import/Service.cs` (1011 dòng) → `ImportJobService` + `ImportDraftService` + `OcrParser` (gộp với folder `ocr/`).
- Move biz logic Casso webhook ra khỏi `TransactionsController.cs:58-73`; move bank redirect ra khỏi `FinancialAccountController`.

### Phase 2.5 — Repository abstraction nhẹ
- `IJarBalanceCalculator`
- `ITransactionRepository` (chỉ expose method thực sự dùng)
- `IFinancialAccountRepository`
- Giữ EF Core trực tiếp cho query đơn giản.

### Phase 2.7 — Validation đồng nhất
- Mở rộng FluentValidation cho DTO: Transaction, Jar, Goal, Limit, Reminder, Import.
- Folder `Service/Validators/` tập trung.

### Phase 2.8 — Concurrency & ownership guard
- Extension `IQueryable<T>.OwnedBy(userId)` cho entity có `user_id`.
- Optimistic concurrency token (`xmin`/`rowversion`) cho `jars`, `financial_accounts`.

### Phase 3 — Tests + CI
- `Personal_Finance_Management.Service.Tests` (xUnit + FluentAssertions + EFCore.InMemory hoặc Testcontainers Postgres).
- `Personal_Finance_Management.Api.Tests` (WebApplicationFactory integration).
- Cover: `TransactionCommandService` (sign convention, jar balance), `GoalContributionService` (progress %), `LimitAlertChecker`, `ImportJobService` (state machine).
- GitHub Actions workflow: `dotnet build` + `dotnet test` + `dotnet format --verify-no-changes`.

### Phase 4 — Background jobs + realtime
- Hangfire + Postgres storage. 4 job:
  - `ReminderDispatcherJob` (mỗi 5 phút).
  - `LimitAlertJob` (trigger post-transaction + recurring mỗi giờ).
  - `BroadcastFanoutJob` (fan-out broadcast → notifications).
  - `ImportJobProcessor` (background Pending → Processing).
- (Phase 5 sau): SignalR hub `/hubs/notifications` cho realtime push.

### Phase 1 dư việc (optional)
- Migration mới (append-only): composite index `notifications(user_id, is_read, created_at DESC)` + covering index `transactions(financial_account_id, category_id, transaction_date DESC)`.

---

## Thứ tự đề xuất tiếp theo

1. **PR-F** (routes + magic numbers) — mechanical, low risk, sạch hết hardcode còn lại.
2. **Phase 2.1** (rename folder PascalCase) — làm cuối nhóm mechanical để giảm conflict.
3. **Phase 2.4** (tách Transaction, Import) — sau khi naming sạch, log đã có để debug.
4. **Phase 2.5 / 2.7 / 2.8** — gộp 1 PR.
5. **Phase 3** (tests + CI) — có thể song song với 2.4.
6. **Phase 4** (Hangfire) — sau khi fat service đã tách.
