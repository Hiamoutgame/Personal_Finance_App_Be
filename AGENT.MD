# AGENT.MD - Context dự án Personal Finance Management / FinJar

Tài liệu này là bộ nhớ nhanh cho agent khi làm việc trong repo này. Mục tiêu là đọc file này trước để nắm domain, kiến trúc, schema, API contract, tình trạng code hiện tại và các điểm dễ sai mà không cần đọc lại toàn bộ tài liệu gốc mỗi lần.

Nguồn đã tổng hợp:

- `docs/apis.md`
- `docs/backend_internal_model.md`
- `docs/finjar_schema.sql`
- `docs/user story.md`
- `Ocr_Service/Note.md`
- code hiện tại trong `Personal_Finance_Management`

## 1. Tóm tắt sản phẩm

Tên dự án trong code là `Personal_Finance_Management`, tài liệu nghiệp vụ dùng tên `FinJar`. Đây là backend cho ứng dụng quản lý tài chính cá nhân.

Sản phẩm giúp user:

- đăng ký, đăng nhập và quản lý hồ sơ cá nhân;
- hoàn thành onboarding để hệ thống hiểu thu nhập, mục tiêu và thói quen chi tiêu;
- tạo nguồn tiền thủ công như tiền mặt, tài khoản ngân hàng tự theo dõi hoặc ví điện tử;
- chia ngân sách vào các hũ chi tiêu;
- ghi nhận thu nhập, chi phí, giao dịch import/OCR;
- xem dashboard cá nhân;
- đặt hạn mức chi tiêu, cảnh báo, mục tiêu tiết kiệm, nhắc lịch thanh toán;
- nhận thông báo cá nhân và broadcast từ admin.

Admin có thể:

- đăng nhập portal quản trị;
- xem, tìm kiếm, khóa/mở khóa tài khoản user;
- quản lý danh mục mặc định;
- gửi broadcast notification;
- xem dashboard vận hành;
- xem audit log;
- cấu hình AI settings.

Quan trọng: FinJar là hệ thống non-custodial. `financial_accounts` chỉ là nguồn tiền user theo dõi trong app, không phải ví thật do FinJar giữ tiền và không thực hiện chuyển tiền thật với ngân hàng.

## 2. Scope và thứ tự ưu tiên

Scope MVP một tháng ưu tiên các luồng `core`:

1. `P0 - Authentication & Admin Foundation`
2. `P1 - Onboarding + Financial Setup`
3. `P2 - Transaction Management Core`
4. `P3 - Personal Dashboard`
5. `P4 - Limits, Alerts, Goals`
6. `P5 - Import Statement`
7. `P6 - Admin User Management & System Notifications`
8. `P7 - Chatbot API MVP` nếu còn capacity

Các tính năng `scale` để sau khi core ổn định:

- bank account sync;
- upload receipt photo và OCR transaction draft;
- AI spending advice;
- suggested saving plan;
- export transaction history;
- admin analytics nâng cao.

Các tính năng `optional` để phase sau:

- shared jar;
- AI chat hoàn chỉnh;
- báo cáo/churn/retention nâng cao.

## 3. Kiến trúc repo

Root repo:

- `Personal_Finance_Management/`: solution .NET backend chính.
- `Ocr_Service/`: service OCR Python riêng, chạy HTTP API `/ocr`.
- `docs/`: tài liệu sản phẩm, API contract, schema, user stories.
- `Dockerfile`, `docker-compose.yml`, `render.yaml`: chạy/deploy API.
- `.env.example`: biến môi trường mẫu.

Solution .NET:

- `Personal_Finance_Management.Api`: ASP.NET Core Web API, controllers, middleware, Swagger, JWT wiring, database migration wiring.
- `Personal_Finance_Management.Service`: DTO request/response, orchestration nghiệp vụ, validation, JWT service, OCR client, import service.
- `Personal_Finance_Management.Repository`: EF Core entities, `AppDbContext`, migrations, enum/constants.

Dependency direction mong muốn:

```text
Api
-> Service/Application
-> Repository / AppDbContext
-> PostgreSQL
```

Repository không nên chứa logic nghiệp vụ phức tạp như chia hũ, trừ balance, evaluate limit. Các rule đó đặt ở service layer.

## 4. Stack kỹ thuật

- .NET 8 / ASP.NET Core Web API.
- EF Core 8 + Npgsql PostgreSQL.
- `EFCore.NamingConventions` với `UseSnakeCaseNamingConvention()`.
- JWT Bearer authentication.
- BCrypt cho password hashing.
- Swagger qua Swashbuckle.
- Python OCR service ở `Ocr_Service`, mặc định port `9380`.
- Docker multi-stage build cho API.
- Render deployment qua `render.yaml`.

Các command hay dùng:

```powershell
dotnet build Personal_Finance_Management/Personal_Finance_Management.sln
dotnet run --project Personal_Finance_Management/Personal_Finance_Management.Api
dotnet ef migrations add <Name> --project Personal_Finance_Management/Personal_Finance_Management.Repository --startup-project Personal_Finance_Management/Personal_Finance_Management.Api
dotnet ef database update --project Personal_Finance_Management/Personal_Finance_Management.Repository --startup-project Personal_Finance_Management/Personal_Finance_Management.Api
```

Chạy OCR service:

```powershell
cd Ocr_Service
pip install -r requirements.txt
python ocr_api.py --host 0.0.0.0 --port 9380
```

## 5. Quy ước chung khi code

- API contract không được xem là database schema. Request/response phục vụ use case FE, entity phục vụ persistence.
- Không expose trực tiếp EF entity ra API response.
- Write API nên trả response gọn: `id`, vài field FE cần cập nhật state, `message`.
- Read API có thể trả projection/read model giàu thông tin để FE render.
- Mọi giá trị tiền dùng `decimal` trong C# và `numeric(18,2)` trong DB.
- Thời gian DB dùng `timestamptz`; C# ưu tiên `DateTimeOffset`.
- DB dùng snake_case, C# dùng PascalCase.
- JSON column trong MVP có thể map bằng `string?`; nếu cần query JSON sau này mới đổi `JsonDocument`/owned type.
- Mọi API thao tác dữ liệu user phải check ownership bằng `UserId`.
- Các luồng tài chính phải atomic trong database transaction.
- Access token only: không có refresh token trong core contract.
- Logout chủ yếu là FE xóa token local; BE có thể ghi audit log nếu cần.

## 6. Auth và JWT

Contract mục tiêu:

- `POST /api/v1/auth/register`
- `POST /api/v1/auth/login`
- `POST /api/v1/auth/logout`
- `POST /api/v1/admin/auth/login`

Hệ thống dùng access token only:

- không có `POST /api/v1/auth/refresh`;
- login/register không trả `refreshToken`;
- token user thường hết hạn theo `JwtOptions.ExpireMinutes`;
- admin login theo docs dự kiến `expiresIn = 3600`, nhưng code hiện tại dùng cùng `JwtOptions`.

JWT claims hiện tại trong code:

- `sub`
- `ClaimTypes.NameIdentifier`
- custom `"id"`
- `ClaimTypes.Name`
- `"username"`
- `ClaimTypes.Email`
- `"firstName"`
- `"lastName"`
- `ClaimTypes.Role`

Service hiện tại lấy user id từ claim `"id"`. Nếu đổi JWT claim, phải sửa các service đang đọc claim này.

Role seed ổn định:

- `00000000-0000-0000-0000-000000000001 = User`
- `00000000-0000-0000-0000-000000000002 = Admin`

## 7. Database model cốt lõi

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

Không dùng các model cũ trong MVP:

- `JarAllocation`
- `JarAllocationItem`
- `JarTransfer`

Các field cũ không nên đưa lại:

- `Account.HashPassword`; dùng `PasswordHash`.
- `FinancialAccount.AvailableBalance`, `FinancialAccount.BalanceAsOf`.
- `Jar.Percentage`; tỷ lệ hũ là rule trong service.
- `Transaction.Amount`, `Transaction.JarId`; dùng `TransactionsAmount`, `FromJarId`, `ToJarId`.
- `ImportTransactionDraft.SuggestedNote`, `SuggestedCategoryId`, `SuggestedJarId`; dùng `Edited*`.
- `GoalContribution.SourceFinancialAccountId`.
- `Reminder.NextDueDate`; tính trong service/job khi response.

## 8. Enum/value chuẩn

Dùng enum hoặc constants string, giá trị phải khớp schema:

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

## 9. Domain rules quan trọng

### Account

`Account` là root ownership của dữ liệu user/admin. Nó chứa login identity, role, status, profile fields và cờ onboarding.

Các field chính:

- `Username`, `Email`, `PasswordHash`
- `FirstName`, `LastName`, `Phone`, `AvatarUrl`
- `Status`, `StatusReason`
- `PreferredCurrency`
- `IsOnboardingCompleted`
- `LastLoginAt`

User bị `Banned` phải bị chặn đăng nhập hoặc thao tác protected.

### Onboarding

Onboarding lưu câu trả lời ban đầu:

- `MonthlyIncome`
- `OccupationType`
- `FinancialGoalTypes`
- `BudgetMethodPreference`
- `AgeRange`
- `SpendingChallenges`
- `RecommendedMethod`
- `CompletedAt`

Trong MVP, `FinancialGoalTypes` và `SpendingChallenges` lưu text, không cần JSON/list trong DB. Service hiện tại join list bằng dấu phẩy.

Sau onboarding nên bootstrap:

- default financial account kiểu `Cash`, `ConnectionMode = Manual`;
- các category gợi ý;
- các jar gợi ý;
- set `Account.IsOnboardingCompleted = true`.

### FinancialAccount

`FinancialAccount` là nguồn tiền user theo dõi. Nó bắt buộc cho transaction và import job.

Các rule:

- manual account dùng `ConnectionMode = Manual`.
- bank sync phase sau dùng `ConnectionMode = LinkedApi`.
- nếu user xóa nguồn tiền đã có transaction/import, nên deactivate bằng `IsActive = false`, không hard delete.
- nên đảm bảo mỗi user có một default account hợp lệ.

### Category

Category phân loại transaction:

- default category do admin quản lý có `IsDefault = true`, `OwnerUserId = null`;
- custom category của user có `OwnerUserId = userId`;
- soft delete bằng `IsActive = false` và/hoặc `DeletedAt`.

### JarSetup và Jar

`JarSetup` lưu phương pháp ngân sách của user: `SixJars`, `Rule503020`, `Custom`, `Undecided`.

`Jar` giữ số dư hũ nội bộ:

- `Balance`
- `Currency`
- `Color`, `Icon`
- `IsDefault`
- `Status`

Không lưu percentage trong table `jars`. Tỷ lệ chia hũ là rule service theo `methodType`.

### Transaction

Transaction là model trung tâm cho thu/chi và các operation liên quan hũ.

Field chính:

- `FinancialAccountId`
- `CategoryId`
- `ImportJobId`
- `Type`
- `TransactionsAmount`
- `TransactionDate`
- `SourceType`
- `FromJarId`
- `ToJarId`
- `JarBalanceAfterAllocation`
- `RawPayloadJson`
- `IsDeleted`, `DeletedAt`

Quy tắc dấu tiền:

- `Type = Income` thì `TransactionsAmount > 0`.
- `Type = Expense` thì `TransactionsAmount < 0`.
- Không đảo dấu tùy tiện trong query/report.

Quy tắc hũ:

- expense thông thường có thể dùng `FromJarId` để trừ hũ;
- income/allocation vào hũ dùng `ToJarId`;
- nếu cần biểu diễn chuyển hũ trong lịch sử giao dịch thì dùng cả `FromJarId` và `ToJarId` trong `transactions`, không có API transfer hũ riêng trong contract hiện tại;
- `FromJarId` và `ToJarId` không được trùng nhau.

### Import

`ImportJob` đại diện một lần upload/import sao kê và phải gắn với `FinancialAccountId`.

`ImportTransactionDraft` là các dòng preview trước khi confirm:

- `RowIndex`
- `TransactionDate`
- `Amount`
- `Type`
- `RawDescription`
- `EditedNote`
- `EditedCategoryId`
- `EditedJarId`
- `IsValid`
- `ValidationError`
- `NormalizedPayloadJson`

Confirm import phải tạo batch `transactions`, cập nhật balance theo rule và cập nhật status/count của job trong cùng DB transaction.

### Limits, Goals, Reminders

`SpendingLimit` có thể target theo `JarId` hoặc `CategoryId`. Constraint hiện tại yêu cầu ít nhất một trong hai có giá trị. Nếu sản phẩm muốn chỉ một target duy nhất, cần đổi constraint thành XOR.

`Goal` có:

- `TargetAmount`
- `SavedAmount`
- `DueDate`
- `Status`
- optional `LinkedJarId`

`GoalContribution` chỉ lưu optional `SourceJarId`. Nếu đóng góp đến từ nguồn tiền thật, nên tạo `Transaction` tương ứng thay vì lưu `SourceFinancialAccountId`.

`Reminder` không lưu `NextDueDate` trong DB. Response tính `nextDueDate` từ `StartDate`, `Frequency`, `DayOfMonth`, `NotifyDaysBefore`.

### Notification, Broadcast, AuditLog, AiSetting

- `Notification`: inbox của user, có `Type`, `Title`, `Body`, `IsRead`, optional `BroadcastId`.
- `Broadcast`: admin gửi thông báo hàng loạt, có `Status`, `TargetCount`, `DeliveredCount`.
- `AuditLog`: append-only log cho thao tác nhạy cảm như admin login, ban/unban, category change, broadcast send, AI settings change.
- `AiSetting`: cấu hình AI toàn hệ thống. Không bao giờ trả `ApiKeyEncrypted` ra API; chỉ trả masked key nếu cần.

## 10. Ranh giới transaction bắt buộc

Các use case sau phải chạy trong cùng database transaction:

- hoàn thành onboarding và bootstrap financial account/category/jar;
- tạo/cập nhật/xóa transaction;
- đóng góp vào goal;
- confirm import transaction drafts;
- ban/unban user kèm audit log;
- tạo broadcast kèm notification nếu dispatch đồng bộ.

Tạo transaction nên làm theo thứ tự:

1. Load financial account và check ownership.
2. Validate category/jar nếu có.
3. Tạo `Transaction`.
4. Cập nhật `FinancialAccount.CurrentBalance`.
5. Nếu có `FromJarId`, trừ `Jar.Balance`.
6. Nếu có `ToJarId`, cộng `Jar.Balance`.
7. Evaluate spending limits nếu là expense.
8. Tạo notification nếu chạm ngưỡng.

Jar setup hiện tại chỉ khởi tạo `jar_setups` và các `jars` ban đầu, thường dùng chung với onboarding. Không còn contract riêng cho phân bổ hũ, chuyển hũ hoặc lịch sử chuyển hũ; các thay đổi tiền/hũ phát sinh sau setup đi qua transaction flow.

## 11. API contract mục tiêu

Base URL mục tiêu: `/api/v1`

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

Nhóm endpoint mục tiêu:

- Auth: register, login, logout, admin login.
- User/profile: `GET/PATCH /api/v1/user/me`, `GET /api/v1/user/me/setup`.
- Onboarding: `POST /api/v1/users/onboarding`.
- Financial accounts: list/create/update/update balance/delete.
- Categories: list/create/update/delete user categories; admin CRUD default categories.
- Jars: list/setup/create/update/delete.
- Transactions: list/create/update/delete.
- Imports: upload, status, preview, confirm.
- Dashboard: user dashboard.
- Limits: CRUD spending limits.
- Notifications: list, mark read/unread.
- Goals: CRUD, detail, contributions.
- Reminders: CRUD.
- Admin: users list/detail/status, broadcasts, dashboard, audit logs, AI settings.

Admin dashboard response hiện tại trong contract phải có:

- `statCards` đúng 4 phần tử theo thứ tự: `total_users`, `engagement`, `transactions`, `system_health`;
- `transactionVolumeTrend` theo `timeframe = day|month|year`;
- `topSpendingCategories` sorted desc, top N và phần `"Khác"`;
- `retentionTrend` theo `D0`, `D7`, `D14`, `D21`, `D30`;
- `recentUsers`, `recentTransactions`.

## 12. Tình trạng code hiện tại

Code hiện tại đã có foundation .NET, EF schema và một số API đầu tiên. Không được giả định toàn bộ contract trong `docs/apis.md` đã implement xong.

Đã có:

- ASP.NET Core API project.
- EF Core `AppDbContext` với 18 DbSet theo schema mục tiêu.
- Migrations hiện có: initial schema và thay đổi OCR config/global draft.
- JWT authentication wiring.
- Global exception middleware trả format gần giống contract cho `AppValidationException`.
- Auth service: register/login user, BCrypt, JWT.
- User service: get/update profile, view setup.
- Onboarding service: lưu profile, bootstrap cash account, category, jar.
- Import image + OCR client: upload file, gọi OCR service, lưu raw OCR JSON.
- Health endpoints.
- Docker/Render deployment config.

Endpoint thực tế hiện tại:

- `POST /api/v1/auth/register`
- `POST /api/v1/auth/login`
- `GET /health`
- `GET /health/db/local`
- `GET /health/db/render`
- `GET /User/me`
- `PATCH /User/me`
- `GET /User/me/setup`
- `POST /Onboarding`
- `POST /api/v1/imports/image`

Các điểm lệch giữa code hiện tại và contract docs:

- `AuthController.Register` hiện trả `Ok` thay vì `201 Created`.
- Register response hiện chỉ có `Id`, `Username`, `FirstName`, `LastName`, `Email`, `AccessToken`; chưa có wrapper `user`, `tokenType`, `expiresIn`, `role`, `isOnboardingCompleted`.
- Login request trong code dùng `Email + Password`, trong docs ghi `username + password`.
- Login chưa check `Account.Status = Banned` theo contract.
- Chưa có `logout`.
- Chưa có admin auth/login riêng.
- `UserController` đang route `[Route("[controller]")]`, nghĩa là `/User/...`, chưa phải `/api/v1/user/...`.
- `OnboardingController` đang route `/Onboarding` và thiếu `[Authorize]`, nhưng service lại đọc user id từ JWT claim. Nên align route/auth khi làm tiếp.
- Import hiện tại là `POST /api/v1/imports/image`, không phải full statement import flow `/api/v1/imports`, status, preview, confirm.
- Full CRUD financial accounts, categories, jars, transactions, dashboard, limits, notifications, goals, reminders, admin APIs chưa thấy implement trong service/controller hiện tại.

Khi agent làm việc, phải phân biệt rõ:

- `docs/apis.md` là contract mục tiêu.
- `docs/backend_internal_model.md` và `docs/finjar_schema.sql` là định hướng persistence/model.
- code hiện tại là trạng thái implement thực tế, có thể chưa khớp contract.

## 13. OCR service

OCR nằm ngoài .NET API ở `Ocr_Service`.

Chạy:

```powershell
cd Ocr_Service
pip install -r requirements.txt
python ocr_api.py --host 0.0.0.0 --port 9380
```

Swagger OCR:

```text
http://127.0.0.1:9380/docs
```

Endpoint OCR:

```http
POST /ocr?layout=none|invoice|document
Content-Type: multipart/form-data
file=<image-or-pdf>
```

Response cơ bản:

```json
{
  "text": "...",
  "layout": "none",
  "pages": [
    {
      "page": 1,
      "text": "..."
    }
  ]
}
```

`layout=invoice` trả thêm `invoice` với `sections`, `items`, `fields`, `boxes`.

`layout=document` trả thêm `layout_regions`.

Backend C# hiện tại chỉ cần:

- upload file vào `Personal_Finance_Management.Service/import/Upload`;
- gọi OCR service qua `Ocr:BaseUrl`;
- lưu raw OCR JSON vào `Upload/OcrResponses`;
- trả raw JSON và text cho FE;
- để FE xử lý layout JSON chi tiết.

## 14. Config và deploy

Config chính:

- `ConnectionStrings:DefaultConnection` dùng local/development.
- `ConnectionStrings:RenderConnection` dùng production/Render.
- `ApplyMigrations` bật thì API tự chạy EF migration lúc start.
- `EnableSwagger` bật Swagger ngoài Development.
- `JwtOptions:SecretKey`, `Issuer`, `Audience`, `ExpireMinutes`.
- `Ocr:BaseUrl`, `Ocr:Layout`, `Ocr:TimeoutSeconds`.

`Program.cs` chọn connection string:

- Development: `DefaultConnection`
- môi trường khác: `RenderConnection`

Lưu ý bảo mật:

- Không thêm secret thật vào tài liệu/context mới.
- Nếu chỉnh config, ưu tiên `.env.example`, user secrets hoặc environment variables.
- Nếu phát hiện secret đang nằm trong tracked file, không copy lại secret sang tài liệu mới; nên đề xuất xoay key nếu đây là repo dùng thật.

## 15. Cách implement tiếp cho ít lỗi

Khi thêm feature mới:

1. Đọc endpoint trong `docs/apis.md`.
2. Đối chiếu schema/entity trong `AppDbContext` và `docs/finjar_schema.sql`.
3. Tạo/update DTO trong `Personal_Finance_Management.Service/<feature>/Request.cs` và `Response.cs`.
4. Viết service orchestration trong `Service.cs`; validate ownership và business rules ở đây.
5. Nếu thao tác tài chính, dùng EF database transaction.
6. Controller chỉ nhận request, gọi service, trả response.
7. Không trả EF entity trực tiếp.
8. Build solution.
9. Nếu đổi entity/schema, tạo migration và review migration.

Thứ tự implement backend được khuyến nghị:

1. Chuẩn hóa route hiện có về `/api/v1`.
2. Hoàn thiện auth response, login bằng username/email theo quyết định sản phẩm, check banned, logout, admin login.
3. Hoàn thiện financial accounts CRUD.
4. Hoàn thiện categories CRUD user/admin.
5. Hoàn thiện jars setup/list/CRUD.
6. Hoàn thiện transactions CRUD và transaction-safe balance updates.
7. Hoàn thiện dashboard user.
8. Hoàn thiện limits/notifications/goals/reminders.
9. Hoàn thiện import statement flow dựa trên `ImportJob` và `ImportTransactionDraft`.
10. Hoàn thiện admin APIs.

## 16. Những lỗi dễ gặp

- Quên `UseSnakeCaseNamingConvention()` sẽ làm EF query sai tên bảng/cột.
- Dùng `Amount` thay vì `TransactionsAmount`.
- Lưu `Jar.Percentage` vào DB dù schema đã bỏ field này.
- Lấy user id từ claim khác `"id"` trong khi service hiện tại đang đọc `"id"`.
- Không check ownership `UserId` khi load financial account/category/jar/goal.
- Hard delete financial account/category/transaction khi schema mong muốn deactivate/soft delete.
- Đảo dấu amount trong nhiều layer. Quy ước duy nhất: Income dương, Expense âm.
- Tạo transaction nhưng quên cập nhật `FinancialAccount.CurrentBalance` hoặc `Jar.Balance`.
- Update transaction nhưng không đảo tác động balance cũ trước khi áp dụng giá trị mới.
- Confirm import nhưng không chạy atomic transaction.
- Trả `ApiKeyEncrypted` hoặc secret config ra API/log.
- Nhầm endpoint contract với endpoint đã implement.
- Thêm navigation collection quá nhiều làm entity phức tạp và query khó kiểm soát.

## 17. Khi nào phải đọc lại docs gốc

Đọc `AGENT.MD` trước. Chỉ đọc lại docs gốc khi:

- cần chi tiết JSON request/response đầy đủ của một endpoint;
- cần đối chiếu check constraint/index trong schema;
- cần cập nhật AGENT.MD vì docs/code đã thay đổi;
- cần hiểu user story/priority ở mức chi tiết hơn;
- cần xử lý OCR layout chi tiết.

File ưu tiên theo mục đích:

- API contract: `docs/apis.md`
- Internal model/schema rules: `docs/backend_internal_model.md`
- SQL schema: `docs/finjar_schema.sql`
- Product scope/user stories: `docs/user story.md`
- OCR integration: `Ocr_Service/Note.md`
- Code thật: `Personal_Finance_Management`

## 18. Nguyên tắc trả lời/ghi chú cho agent sau này

Khi làm task trong repo này:

- Trả lời bằng tiếng Việt nếu user dùng tiếng Việt.
- Nói rõ phần nào đã implement, phần nào mới là contract/kế hoạch.
- Nếu route/response hiện tại lệch docs, nêu lệch đó trước khi sửa.
- Ưu tiên sửa nhỏ, khớp pattern hiện tại.
- Không tự refactor lớn nếu task chỉ yêu cầu một endpoint nhỏ.
- Sau khi sửa code, chạy `dotnet build` nếu thay đổi C#.
- Không revert thay đổi lạ trong worktree nếu không được yêu cầu.
