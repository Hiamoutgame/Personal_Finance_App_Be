# AGENTS.MD - FinJar / Personal Finance Management

Tài liệu này là context bắt buộc cho agent khi làm việc trong repo này. Đọc file này trước khi sửa code hoặc docs để nắm domain, API contract, schema, trạng thái code hiện tại và các điểm dễ sai.

Nguồn đã tổng hợp:

- `docs/apis.md`
- `docs/finjar_schema.sql`
- `docs/user story.md`
- `docs/backend_internal_model.md`
- code trong `Personal_Finance_Management`

## 1. Sản phẩm

Tên sản phẩm trong tài liệu là `FinJar`; solution backend tên `Personal_Finance_Management`.

FinJar là backend cho ứng dụng quản lý tài chính cá nhân. User có thể:

- đăng ký, đăng nhập, quản lý hồ sơ;
- hoàn thành onboarding để app gợi ý nguồn tiền, hũ và danh mục;
- tạo và quản lý nguồn tiền thủ công như tiền mặt, ngân hàng tự theo dõi, ví điện tử;
- tạo và quản lý hũ chi tiêu;
- ghi nhận thu nhập/chi phí;
- import sao kê và xem preview trước khi confirm;
- xem dashboard cá nhân;
- đặt hạn mức, nhận cảnh báo, quản lý mục tiêu, nhắc lịch và thông báo;
- chat với AI ở mức MVP.

Admin có thể:

- đăng nhập admin portal;
- xem và quản lý user;
- quản lý danh mục mặc định;
- gửi broadcast notification;
- xem dashboard vận hành;
- xem audit log;
- cấu hình AI settings.

Quan trọng: FinJar là hệ thống non-custodial. `financial_accounts` chỉ là nguồn tiền user theo dõi trong app, không phải ví thật do FinJar giữ tiền và không thực hiện chuyển tiền thật.

## 2. Scope

Scope MVP một tháng ưu tiên các flow core:

1. `P0 - Authentication & Admin Foundation`
2. `P1 - Onboarding, Profile, Financial Accounts, Category`
3. `P2 - Jars, Transactions, Import Statement`
4. `P3 - Personal Dashboard`
5. `P4 - Limits, Notifications, Goals, Reminders`
6. `P6 - Admin User Management & System Notifications`
7. `P7 - AI Chatbot MVP`

Các story `scale` và `optional` để sau khi core ổn định: bank sync, OCR receipt nâng cao, export, shared jar, analytics/retention nâng cao, AI chat/session hoàn chỉnh.

## 3. Tài liệu Nguồn

`docs/apis.md` là API contract public cho FE và BE. File đó mô tả request/response mà FE sẽ gọi trong MVP theo hướng client-first.

`docs/backend_internal_model.md` là tài liệu nội bộ backend. Backend có thể có service command, CRUD nội bộ, job, seed, data repair hoặc admin/backoffice capability đầy đủ hơn public API. Không tự expose capability nội bộ ra public API nếu chưa có use case UI rõ ràng.

`docs/finjar_schema.sql` là schema mục tiêu cho persistence.

`docs/user story.md` mô tả nghiệp vụ và ưu tiên `core`, `scale`, `optional`.

Khi code đang lệch docs, không đoán. Nêu rõ: docs là contract mục tiêu, code là trạng thái implement hiện tại.

## 4. Kiến trúc Repo

Root chính:

- `Personal_Finance_Management/`: solution .NET backend.
- `docs/`: API contract, schema, user story, internal backend model.
- `Ocr_Service/`: service OCR Python riêng nếu có task liên quan OCR.
- Docker/deploy files ở root.

Solution .NET:

- `Personal_Finance_Management.Api`: ASP.NET Core Web API, controllers, middleware, Swagger, JWT, migration/seed wiring.
- `Personal_Finance_Management.Service`: DTO request/response, business orchestration, validation, JWT, import/OCR client, admin/dashboard/AI services.
- `Personal_Finance_Management.Repository`: EF Core entities, enums/constants, `AppDbContext`, migrations.

Dependency direction mong muốn:

```text
Api
-> Service/Application
-> Repository / AppDbContext
-> PostgreSQL
```

Controller chỉ nhận request, gọi service, trả response. Business rules, ownership checks và transaction boundary đặt ở service layer. Repository/EF model không nên chứa nghiệp vụ tài chính phức tạp.

## 5. Stack

- .NET 8 / ASP.NET Core Web API.
- EF Core 8 + Npgsql PostgreSQL.
- `EFCore.NamingConventions` với `UseSnakeCaseNamingConvention()`.
- JWT Bearer authentication.
- BCrypt cho password hashing.
- FluentValidation có trong service layer.
- Swagger qua Swashbuckle.
- Google GenAI package có trong service project.
- OCR service Python có thể chạy riêng.

Lệnh thường dùng:

```powershell
dotnet build Personal_Finance_Management/Personal_Finance_Management.sln
dotnet run --project Personal_Finance_Management/Personal_Finance_Management.Api
dotnet ef migrations add <Name> --project Personal_Finance_Management/Personal_Finance_Management.Repository --startup-project Personal_Finance_Management/Personal_Finance_Management.Api
dotnet ef database update --project Personal_Finance_Management/Personal_Finance_Management.Repository --startup-project Personal_Finance_Management/Personal_Finance_Management.Api
```

## 6. API Contract Quan Trọng

Base URL mục tiêu: `/api/v1`.

Auth:

- Public: không cần token.
- Bearer: user access token.
- Admin: access token role `Admin`.
- Core contract là access token only. Không có refresh token trong MVP.

Error format mục tiêu:

```json
{
  "success": false,
  "error": "Mô tả lỗi ngắn gọn cho user",
  "details": {
    "field": "amount",
    "code": "INSUFFICIENT_BALANCE"
  },
  "traceId": "guid"
}
```

Pagination format:

```json
{
  "data": [],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 0,
    "totalPages": 0
  }
}
```

Endpoint groups trong `docs/apis.md`:

- Auth: register, login, logout, admin login.
- User/profile/setup.
- Onboarding.
- Financial accounts.
- Categories.
- Jars.
- Transactions.
- Imports.
- Dashboard.
- Limits.
- Notifications.
- Goals.
- Reminders.
- Admin users/categories/broadcasts/dashboard/audit logs/AI settings.
- AI chatbot.

## 7. Contract Đã Chốt Gần Đây

Các điểm này phải ưu tiên theo `docs/apis.md` hiện tại:

- Không còn `PATCH /api/v1/financial-accounts/{id}/balance`.
- Cập nhật balance đi qua `PATCH /api/v1/financial-accounts/{id}` bằng `currentBalance`; nếu không cập nhật balance thì FE gửi `currentBalance = null`.
- Không còn public API `POST /api/v1/jars/setup`.
- Không còn public API `POST /api/v1/jars/allocate`.
- Không còn public API `POST /api/v1/jars/transfer`.
- Không còn public API `GET /api/v1/jars/transfers`.
- Hũ mặc định được tạo sau onboarding.
- FE gửi `transactionsAmount` là số dương cho cả Income và Expense.
- Backend dựa vào transaction `type` để cộng Income và trừ Expense.
- Public `PATCH /api/v1/transactions/{id}` chỉ nhận `transactionsAmount`, `categoryId`, `note`.
- User dashboard `GET /api/v1/dashboard` không nhận query params hoặc request body.

Nếu code hoặc docs cũ nhắc các endpoint jar setup/allocate/transfer hoặc balance endpoint riêng, xem đó là thông tin lỗi thời trừ khi user yêu cầu khôi phục.

## 8. Schema Cốt Lõi

Schema mục tiêu gồm 18 entity chính:

- `Role`
- `Account`
- `AuditLog`
- `OnboardingProfile`
- `JarSetup`
- `FinancialAccount`
- `Category`
- `Jar`
- `ImportJob`
- `Transaction`
- `SpendingLimit`
- `Goal`
- `GoalContribution`
- `Reminder`
- `Broadcast`
- `Notification`
- `ImportTransactionDraft`
- `AiSetting`

Không dùng lại các model/table cũ:

- `JarAllocation`
- `JarAllocationItem`
- `JarTransfer`

Không đưa lại các field cũ:

- `Account.HashPassword`; dùng `PasswordHash`.
- `FinancialAccount.AvailableBalance`.
- `FinancialAccount.BalanceAsOf`.
- `Jar.Percentage`; tỷ lệ hũ là rule trong service.
- `Transaction.Amount`; dùng `TransactionsAmount`.
- `Transaction.JarId`; dùng `FromJarId` và `ToJarId`.
- `ImportTransactionDraft.SuggestedNote`, `SuggestedCategoryId`, `SuggestedJarId`; dùng `Edited*`.
- `GoalContribution.SourceFinancialAccountId`.
- `Reminder.NextDueDate`; tính trong service/job khi trả response.

## 9. Enum Chuẩn

Giá trị string phải khớp schema/docs:

- `AccountStatus`: `Active`, `Banned`
- `FinancialAccountType`: `Cash`, `Bank`, `EWallet`, `Other`
- `ConnectionMode`: `Manual`, `LinkedApi`
- `SyncStatus`: `NeverSynced`, `Synced`, `Syncing`, `Error`, `Disconnected`
- `BudgetMethodType`: `SixJars`, `Rule503020`, `Custom`, `Undecided`
- `JarStatus`: `Active`, `Paused`, `Archived`
- `TransactionType`: `Income`, `Expense`
- `TransactionSourceType`: `Manual`, `Imported`, `OCR`, `Jar`, `System`
- `LimitPeriod`: `Daily`, `Monthly`
- `GoalStatus`: `Active`, `Completed`, `Cancelled`
- `ReminderFrequency`: `Daily`, `Weekly`, `Monthly`, `Quarterly`, `Yearly`
- `ReminderStatus`: `Active`, `Paused`, `Completed`, `Cancelled`
- `BroadcastStatus`: `Queued`, `Sent`, `Failed`, `Cancelled`
- `NotificationType`: `SpendingAlert`, `GoalUpdate`, `Reminder`, `System`, `Broadcast`
- `ImportJobStatus`: `Pending`, `Processing`, `AwaitingReview`, `Completed`, `Failed`

Lưu ý: code hiện tại có chỗ cho phép `Transaction.Type = Transfer`, nhưng `docs/apis.md` và `docs/finjar_schema.sql` hiện chỉ chốt `Income | Expense` cho public contract. Nếu sửa transaction, phải quyết định rõ giữ transfer là internal capability hay loại khỏi public/schema.

## 10. Domain Rules

### Account

`Account` là root ownership của dữ liệu user/admin. Mọi dữ liệu user phải filter/check theo `UserId`.

User bị `Banned` phải bị chặn đăng nhập hoặc gọi protected user APIs theo contract.

Role seed ổn định:

- `00000000-0000-0000-0000-000000000001 = User`
- `00000000-0000-0000-0000-000000000002 = Admin`

### Onboarding

Onboarding lưu thu nhập, nghề nghiệp, mục tiêu, phương pháp ngân sách, độ tuổi và khó khăn chi tiêu.

Sau onboarding nên:

- tạo onboarding profile;
- tạo default financial account kiểu `Cash`;
- tạo `JarSetup`;
- tạo default jars;
- set `Account.IsOnboardingCompleted = true`.

### FinancialAccount

`FinancialAccount` là nguồn tiền user theo dõi, bắt buộc cho transaction và import job.

Rule:

- manual account dùng `ConnectionMode = Manual`;
- linked account là phase sau với `ConnectionMode = LinkedApi`;
- delete nên map thành `IsActive = false` nếu đã có dữ liệu liên quan;
- mỗi user nên có default account hợp lệ;
- cập nhật số dư thủ công dùng `PATCH /api/v1/financial-accounts/{id}` với `currentBalance`.

### Category

Default category:

- `IsDefault = true`
- `OwnerUserId = null`
- admin quản lý

Custom category:

- `OwnerUserId = userId`
- user quản lý

Delete nên soft delete bằng `IsActive = false` và/hoặc `DeletedAt`.

### Jar

`JarSetup` lưu method budgeting hiện tại: `SixJars`, `Rule503020`, `Custom`, `Undecided`.

`Jar` giữ balance nội bộ, màu, icon, trạng thái.

Không lưu percentage trong DB. Tỷ lệ chia hũ là rule trong service theo method type.

Public MVP chỉ có CRUD jar. Các operation phân bổ/chuyển hũ nếu còn trong service thì xem là internal capability, không phải public API contract.

### Transaction

Transaction là model trung tâm cho thu/chi và các operation tác động balance.

Contract public:

- FE gửi amount dương.
- `POST /api/v1/transactions` tạo giao dịch.
- `PATCH /api/v1/transactions/{id}` chỉ sửa amount/category/note.
- Response có thể trả amount dương cho FE theo contract.

Persistence/docs schema:

- Income lưu dương.
- Expense trong `docs/finjar_schema.sql` đang lưu âm.
- Service phải là nơi chuẩn hóa dấu nếu DB giữ dấu.

Khi tạo/cập nhật/xóa transaction phải xử lý atomic:

1. Load financial account và check ownership.
2. Validate category/jar nếu có.
3. Tạo hoặc chỉnh transaction.
4. Cập nhật `FinancialAccount.CurrentBalance`.
5. Nếu có `FromJarId`, trừ `Jar.Balance`.
6. Nếu có `ToJarId`, cộng `Jar.Balance`.
7. Evaluate spending limits nếu là expense.
8. Tạo notification nếu chạm ngưỡng.

Không hard delete transaction; dùng `IsDeleted` và `DeletedAt`.

### Import

`ImportJob` đại diện một lần upload/import sao kê và phải gắn với `FinancialAccountId`.

`ImportTransactionDraft` là dòng preview trước confirm. Confirm import phải tạo batch transactions, cập nhật balance và cập nhật job status/count trong cùng DB transaction.

### Limits, Goals, Reminders

`SpendingLimit` target theo `JarId` hoặc `CategoryId`. Schema hiện yêu cầu ít nhất một trong hai có giá trị.

`GoalContribution` chỉ có optional `SourceJarId`. Nếu đóng góp từ nguồn tiền thật, service nên tạo transaction tương ứng thay vì thêm `SourceFinancialAccountId`.

`Reminder` không lưu `NextDueDate`; tính từ `StartDate`, `Frequency`, `DayOfMonth`, `NotifyDaysBefore`.

### Notification, Broadcast, Audit, AI

- `Notification` là inbox của user.
- `Broadcast` do admin tạo, có thể dispatch thành notifications.
- `AuditLog` append-only cho thao tác nhạy cảm.
- `AiSetting` không bao giờ trả `ApiKeyEncrypted` hoặc secret thô ra API/log. `apiKeyMasked` chỉ để FE biết server đã có config provider key hay chưa.

## 11. Trạng Thái Code Hiện Tại

Code đã có nhiều module, nhưng chưa chắc khớp hoàn toàn `docs/apis.md`.

Đã có wiring:

- `Program.cs` đăng ký controllers, Swagger, JWT, authorization policies, DbContext Npgsql, snake_case, migration, seed accounts.
- `AppDbContext` có 18 DbSet chính.
- Global exception middleware.
- Authorization policies `User` và `Admin`.
- Services cho auth, user, onboarding, financial account, jar, transaction, dashboard, category, broadcast, admin, AI, import/OCR.

Controllers hiện có:

- `AuthController`: route `api/v1/auth`, có register/login.
- `CategoryController`: route `api/v1/categories`, user policy.
- `ImportController`: route `api/v1/imports`, có `POST image`.
- `AdminUserController`: route `api/v1/admin/users`.
- `AdminCategoryController`: route `api/v1/admin/categories`.
- `AdminDashboardController`: route `api/v1/admin/dashboard`.
- `AdminBroadcastController`: route `api/v1/admin/broadcasts`.
- `AdminAuditLogController`: route `api/v1/admin/audit-logs`.
- `AdminAISettingController`: route `api/v1/admin/ai-settings`.
- `HealthController`: route `health`.
- `UserController`: currently route `[controller]` => `/User/...`.
- `OnboardingController`: currently route `[controller]` => `/Onboarding`.
- `FinancialAccountController`: currently route `[controller]` => `/FinancialAccount`.
- `JarController`: currently route `[controller]` => `/Jar`.
- `TransactionsController`: currently route `[controller]` => `/Transactions`.
- `DashboardController`: controller route `[controller]`, action has absolute route `/user/dashboard`.

Known mismatches with `docs/apis.md`:

- Register returns `Ok`, contract says `201 Created`.
- Register/login response shape is flatter than contract; contract wants token metadata and nested user/admin object.
- Login request in code uses `Email + Password`; docs use `username + password`.
- Login currently does not clearly enforce `Banned` status.
- No `POST /api/v1/auth/logout`.
- No `POST /api/v1/admin/auth/login` as separate route.
- Several user routes are not under `/api/v1`.
- `FinancialAccountController`, `JarController`, `TransactionsController`, `DashboardController`, `UserController`, `OnboardingController` need route alignment if task is API contract compliance.
- Financial account update returns `updatedAt`, while docs prefer not returning internal fields in write responses.
- Transaction service currently has logic for `Transfer`, but public docs removed jar transfer APIs and schema/docs only use Income/Expense.
- `AppDbContext` transaction check constraint currently allows Expense positive and Transfer; `docs/finjar_schema.sql` says Income positive, Expense negative.
- Transaction create/update does not consistently use explicit EF database transaction.
- Some service queries use `FirstOrDefault` without ownership filter by user id after loading the user.
- Some projections assume nullable navigation is non-null, for example transaction jar/category, which can break if optional.
- Import implemented endpoint is `POST /api/v1/imports/image`, not the full statement import flow in docs (`POST /api/v1/imports`, status, preview, confirm).

Do not rewrite large areas just because mismatch exists. Fix only the requested surface unless user asks for contract-wide alignment.

## 12. Current Project Patterns

Common service folder pattern:

```text
Personal_Finance_Management.Service/<feature>/
  IService.cs
  Request.cs
  Response.cs
  Service.cs
```

Controller pattern:

- Use `[ApiController]`.
- Inject feature service interface.
- Return `Ok(result)` or `StatusCode(201, result)` where already used.
- User endpoints should use `AuthorizationExtension.Policies.User`.
- Admin endpoints should use `AuthorizationExtension.Policies.Admin`.

Current service code often reads user id from JWT claim `"id"`:

```csharp
_httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value
```

If changing JWT claims, update all service usages.

## 13. Config and Deploy

Important config:

- `ConnectionStrings:DefaultConnection`
- `ConnectionStrings:RenderConnection`
- `ApplyMigrations`
- `EnableSwagger`
- `JwtOptions:SecretKey`
- `JwtOptions:Issuer`
- `JwtOptions:Audience`
- `JwtOptions:ExpireMinutes`
- `SeedAccounts`
- `Ocr:BaseUrl`
- `Ocr:Layout`
- `Ocr:TimeoutSeconds`
- AI provider key via server config/env, not public API.

Security:

- Do not add real secrets to docs or committed config.
- Do not print connection strings or API keys in logs/responses.
- If a tracked file contains a real secret, do not copy it elsewhere; mention that it should be rotated.

## 14. How To Implement Safely

When implementing a feature:

1. Read the target endpoint in `docs/apis.md`.
2. Check the entity/schema in `docs/finjar_schema.sql` and `AppDbContext`.
3. Check current controller route and service pattern.
4. Add/update DTOs in `Request.cs` and `Response.cs`.
5. Put validation, ownership checks and financial logic in service.
6. Use EF database transaction for money movement, import confirm, goal contribution, broadcast dispatch and admin audit-sensitive operations.
7. Do not return EF entities directly.
8. Keep response fields aligned with docs; avoid `createdAt/updatedAt` in write responses unless docs require it.
9. Run `dotnet build Personal_Finance_Management/Personal_Finance_Management.sln` after C# edits when feasible.

Preferred order for broad contract alignment:

1. Normalize routes to `/api/v1`.
2. Align auth responses, logout, admin login and banned-user checks.
3. Align financial accounts CRUD and balance update behavior.
4. Align categories user/admin CRUD.
5. Align jars CRUD and onboarding default jar creation.
6. Align transactions CRUD, amount sign normalization and atomic balance updates.
7. Align personal dashboard endpoint.
8. Implement limits, notifications, goals, reminders.
9. Implement full import statement flow.
10. Finish admin APIs and AI chat.

## 15. Pitfalls

- Nhầm `docs/apis.md` contract với code đã implement.
- Dùng route `[controller]` trong khi contract yêu cầu `/api/v1/...`.
- Reintroduce refresh token.
- Reintroduce `/financial-accounts/{id}/balance`.
- Reintroduce public `jars/setup`, `jars/allocate`, `jars/transfer`, `jars/transfers`.
- Dùng `Amount` thay vì `TransactionsAmount`.
- Lưu `Jar.Percentage`.
- Dùng `Transaction.JarId` thay vì `FromJarId`/`ToJarId`.
- Đảo dấu amount ở nhiều layer không nhất quán.
- Quên update `FinancialAccount.CurrentBalance` hoặc `Jar.Balance` khi transaction thay đổi.
- Update transaction amount nhưng không hoàn tác tác động cũ trước khi áp dụng số mới.
- Hard delete dữ liệu cần soft delete/deactivate.
- Quên check ownership `UserId`.
- Confirm import không atomic.
- Trả `ApiKeyEncrypted`, API key, prompt nội bộ hoặc secret config ra FE.
- Thêm migration mà không so với `docs/finjar_schema.sql`.
- Sửa file generated/upload trong `Service/import/Upload` nếu task không liên quan.

## 16. Khi Nào Đọc Lại File Gốc

Đọc lại `docs/apis.md` khi cần JSON request/response đầy đủ.

Đọc lại `docs/finjar_schema.sql` khi sửa entity, migration, constraint, index hoặc enum.

Đọc lại `docs/user story.md` khi cần quyết định scope/priority.

Đọc lại `docs/backend_internal_model.md` khi cần phân biệt public API với internal backend capability.

Đọc code thật trong `Personal_Finance_Management` trước khi sửa vì code có thể đang lệch docs.

## 17. Quy Tắc Làm Việc Cho Agent

- Trả lời bằng tiếng Việt nếu user dùng tiếng Việt.
- Nói rõ phần nào đã sửa, phần nào chỉ là phát hiện.
- Không tự refactor lớn ngoài phạm vi user yêu cầu.
- Không revert thay đổi lạ trong worktree nếu không được yêu cầu.
- Khi sửa docs, giữ đồng bộ với `docs/apis.md`, `docs/finjar_schema.sql`, `docs/user story.md`.
- Khi sửa C#, ưu tiên build solution sau khi hoàn tất.
- Nếu thấy contract và code mâu thuẫn, hỏi hoặc nêu giả định trước khi đổi behavior lớn.
