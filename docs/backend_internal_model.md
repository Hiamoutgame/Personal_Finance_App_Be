# FinJar - Mô hình nội bộ Backend

Tài liệu này mô tả cách backend nên tổ chức model nội bộ, ranh giới transaction, entity EF Core và lớp Repository cho schema hiện tại trong finjar_schema.sql.

Phạm vi hiện tại là MVP một tháng, nên ưu tiên dùng được, dễ code và ít abstraction thừa. Schema cơ sở dữ liệu là nguồn thông tin chính cho persistence.

## 0. Cách đọc cùng `docs/apis.md`

`docs/apis.md` là API contract dành cho FE và BE cùng thống nhất. File đó mô tả request/response public mà FE sẽ gọi trong MVP, theo hướng client-first: request gọn, response chỉ trả dữ liệu cần render hoặc cập nhật UI.

`docs/backend_internal_model.md` là tài liệu nội bộ backend. File này mô tả entity, service command, transaction boundary, repository và các thao tác CRUD đầy đủ hơn để backend có thể vận hành, chỉnh sửa, seed data, xử lý job hoặc mở rộng admin/backoffice.

Quy ước phân tầng:

| Lớp | Người dùng chính | Mục đích | Có bắt buộc FE gọi không? |
| --- | --- | --- | --- |
| Public API contract | FE + BE | Endpoint trong `docs/apis.md`, phục vụ màn hình MVP | Có, nếu màn hình cần |
| Admin/Backoffice API | Admin FE + BE | Quản trị user, default category, broadcast, AI setting | Chỉ admin FE gọi |
| Backend internal service | BE | Command/query nội bộ, job, seed, data repair, nghiệp vụ atomic | Không |
| Repository/EF model | BE | Persistence theo `finjar_schema.sql` | Không |

Vì vậy backend có thể có đầy đủ method/service CRUD hơn API public. FE chỉ call các endpoint đã expose trong `docs/apis.md` hoặc các endpoint admin được cấp quyền. Những thao tác nội bộ không nên tự động expose ra public API nếu chưa có use case UI rõ ràng.

## 1. Mục tiêu

- Giữ API DTO gọn cho frontend.
- Không expose trực tiếp entity EF ra response API.
- Đảm bảo các nghiệp vụ tài chính chạy atomic trong database transaction.
- Đồng bộ code Repository với các thay đổi schema: `transactions_amount`, `from_jar_id`, `to_jar_id`, `edited_*` — loại bỏ `jar_allocations`, `jar_transfers`.
- Chuẩn bị nền tảng để sau này mở rộng import, notification, AI và bank sync.
- Cho phép backend có service CRUD nội bộ đầy đủ hơn public API, nhưng vẫn giữ ranh giới rõ: public API chỉ expose phần FE thật sự cần.

## 2. Nguyên tắc thiết kế backend

### API contract ≠ database schema

Frontend gửi request theo use case. Backend sẽ map request thành command/query rồi thao tác entity.

Ví dụ:

```text
CreateTransactionRequest
-> CreateTransactionCommand
-> Validate ownership/balance
-> Transaction entity
-> Update FinancialAccount/Jar
-> Return TransactionResponse
```

### Write model gọn, read model linh hoạt

- API ghi (`POST`, `PATCH`, `DELETE`) nên nhận request tối thiểu cần thiết.
- API đọc (`GET /dashboard`, `GET /transactions`, `GET /goals`) có thể dùng projection/read model riêng, join nhiều bảng để FE render nhanh.
- Internal command có thể nhận đầy đủ field hơn public request nếu phục vụ admin, migration, job hoặc data repair.

### Public API input ≠ internal persisted value

Một số field được FE gửi theo dạng dễ nhập, sau đó backend chuẩn hóa trước khi lưu.

Ví dụ với transaction:

- FE gửi `transactionsAmount` là số dương cho cả Income và Expense.
- Backend dựa vào `type` để lưu DB: Income dương, Expense âm.
- Khi update transaction public, FE chỉ sửa `transactionsAmount`, `categoryId`, `note`; các field như account, jar, type, date không đổi qua endpoint update MVP.

### Tiền tệ dùng `decimal`

Mọi giá trị tiền trong C# dùng `decimal`:

- balance
- amount
- targetAmount
- limitAmount
- savedAmount
- transactionsAmount

### Ranh giới transaction nằm ở application/service layer

Các use case sau phải chạy trong cùng database transaction:

- hoàn thành onboarding và setup jar mặc định
- tạo/cập nhật/xóa transaction
- thao tác nội bộ liên quan hũ nếu backend/job/admin cần phân bổ hoặc chuyển số dư
- đóng góp vào goal
- confirm import transaction drafts
- ban/unban user kèm audit log
- tạo broadcast kèm notification nếu dispatch đồng bộ

## 3. Kiến trúc lớp xuất ra

```text
Api
-> Service/Application
-> Repository / AppDbContext
-> PostgreSQL
```

Trong project hiện tại:

- `Personal_Finance_Management.Api`: controllers, middleware, swagger, auth wiring.
- `Personal_Finance_Management.Service`: request/response DTO, business orchestration, validation.
- `Personal_Finance_Management.Repository`: EF Core entities, `AppDbContext`, migrations.

Repository layer không nên chứa logic nghiệp vụ phức tạp như chia hũ, trừ balance, evaluate limit. Nó chỉ nên cung cấp persistence primitives và query/update.

## 3.1. Năng lực CRUD nội bộ

Bảng dưới đây mô tả năng lực backend nên có ở service layer. Không phải dòng nào cũng là public API cho FE.

| Module | Public API trong `apis.md` | Internal service nên có | Ghi chú expose |
| --- | --- | --- | --- |
| Account/Profile | register, login, logout, get/update me | CRUD account, đổi role/status, reset trạng thái onboarding, cập nhật last login | Chỉ expose phần profile cho user; quản trị account đi qua admin API |
| Onboarding | submit/get setup | tạo/cập nhật onboarding profile, tạo default financial account/jars/categories theo rule | FE không cần CRUD raw onboarding profile |
| FinancialAccount | CRUD nguồn tiền, update balance qua `PATCH /financial-accounts/{id}` | CRUD đầy đủ, set default, deactivate/reactivate, cập nhật sync metadata | Không expose endpoint balance riêng |
| Category | CRUD custom category, admin CRUD default category | CRUD default/custom, soft delete, restore nếu cần vận hành | User chỉ sửa category của mình; admin sửa default |
| Jar/JarSetup | CRUD jar | tạo default jars sau onboarding, CRUD jar setup, pause/archive/reactivate jar | Không còn public `jars/setup`, `jars/allocate`, `jars/transfer`, `jars/transfers` trong MVP |
| Transaction | CRUD transaction MVP | tạo manual/import/system transaction, reverse impact, soft delete, rebuild balance khi cần | Public update chỉ sửa amount/category/note |
| Import | upload/status/preview/confirm | CRUD import job/draft, parse, normalize, validate, confirm idempotent | Draft edit có thể expose qua import preview/confirm, không cần CRUD raw nếu FE chưa cần |
| Dashboard/Report | dashboard user/admin | read model/projection, aggregate query, cache snapshot nếu cần | FE gọi read API, không thao tác entity dashboard |
| Limit/Goal/Reminder | CRUD theo `apis.md` | CRUD đầy đủ, tính trạng thái, scheduler, contribution command | Job nội bộ tạo notification khi tới hạn |
| Notification/Broadcast | inbox/status, admin broadcast | CRUD notification, mark read, dispatch broadcast, retry delivery | User không tạo notification trực tiếp |
| AI Setting/AI Chat | admin AI setting, user chat | CRUD AI setting, build context, fallback rule-based, log lỗi provider | Không expose provider key ra FE |

Nguyên tắc expose:

- Nếu FE cần màn hình hoặc workflow cụ thể thì thêm vào `docs/apis.md`.
- Nếu chỉ phục vụ vận hành, seed, job, migration hoặc repair dữ liệu thì giữ ở service/internal/admin.
- Nếu expose endpoint mới, phải có auth/ownership/audit rõ ràng trước khi đưa vào FE.

## 4. Quy ước model chia sẻ

### Base entity

Không nên ép mọi entity có `IsDeleted`. Schema hiện tại chỉ có soft delete ở:

- `transactions`
- `categories` thông qua `deleted_at` và `is_active`

Kế hoạch nên tách base:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
}

public interface ICreatedAtEntity
{
    DateTimeOffset CreatedAt { get; set; }
}

public interface IUpdatedAtEntity
{
    DateTimeOffset UpdatedAt { get; set; }
}
```

Lý do: mọi entity chính dùng `Guid Id`, nhưng `goal_contributions` không có `updated_at`, `notifications` không có `updated_at`, nên một base class quá nặng sẽ làm EF map sai schema.

### Quy ước đặt tên

- C# dùng PascalCase: `PasswordHash`, `TransactionsAmount`, `FromJarId`.
- Database dùng snake_case: `password_hash`, `transactions_amount`, `from_jar_id`.
- Cấu hình tên cột trong `AppDbContext` hoặc dùng naming convention plugin nếu team thêm.

### Các cột JSON

Schema dùng `json` (không phải `jsonb`):

- `audit_logs.metadata_json`
- `transactions.raw_payload_json`
- `notifications.metadata_json`
- `import_transaction_drafts.normalized_payload_json`

Trong C#, MVP có thể dùng `string?` cho đơn giản. Sau này nếu cần query JSON thì đổi sang `JsonDocument`/owned type.

## 5. Enum nội bộ

Nên tạo enum hoặc hằng số cho các varchar enum sau:

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

MVP có thể lưu property là `string`, nhưng service phải validate bằng constants. Nếu dùng enum + EF conversion, cần đảm bảo giá trị string khớp schema.

## 6. Các aggregate nội bộ

### Identity

#### `Role`

- Bảng: `roles`
- PK: `Guid Id`
- Seed: `00000000-0000-0000-0000-000000000001 User`, `00000000-0000-0000-0000-000000000002 Admin`
- Có thể kế thừa `BaseEntity` vì `BaseEntity` chỉ chứa `Guid Id`.

#### `Account`

Vai trò:

- tài khoản đăng nhập của user/admin;
- root ownership của dữ liệu user;
- chứa status và cờ onboarding.

Trường cần có:

- `Guid Id`
- `Guid RoleId`
- `string Username`
- `string Email`
- `string PasswordHash`
- `string FirstName`
- `string LastName`
- `string? Phone`
- `string? AvatarUrl`
- `string Status`
- `string? StatusReason`
- `string PreferredCurrency`
- `bool IsOnboardingCompleted`
- `DateTimeOffset? LastLoginAt`
- `DateTimeOffset CreatedAt`
- `DateTimeOffset UpdatedAt`

Lưu ý: schema mới là `password_hash`; code hiện tại có `HashPassword`, cần đổi sang `PasswordHash`.

### Onboarding

#### `OnboardingProfile`

Vai trò:

- lưu kết quả onboarding;
- làm input gợi ý method budgeting và jars.

Trường có thể có:

- `Guid Id`
- `Guid UserId`
- `decimal? MonthlyIncome`
- `string? OccupationType`
- `string? FinancialGoalTypes`
- `string BudgetMethodPreference`
- `string? AgeRange`
- `string? SpendingChallenges`
- `string? RecommendedMethod`
- `DateTimeOffset? CompletedAt`
- `DateTimeOffset CreatedAt`
- `DateTimeOffset UpdatedAt`

Lưu ý: schema hiện tại dùng `text` cho `financial_goal_types` và `spending_challenges`, không phải JSON/list.

### Nguồn tiền

#### `FinancialAccount`

Vai trò:

- nguồn tiền user theo dõi;
- bắt buộc cho transaction và import job;
- non-custodial: FinJar không giữ tiền thật.

Trường có thể có:

- `Guid Id`
- `Guid UserId`
- `string Name`
- `string AccountType`
- `string ConnectionMode`
- `string? ProviderCode`
- `string? ProviderName`
- `string? ExternalAccountId`
- `string? ExternalAccountRef`
- `string? MaskedAccountNumber`
- `string? AccountHolderName`
- `string Currency`
- `decimal CurrentBalance`
- `string SyncStatus`
- `DateTimeOffset? LastSyncedAt`
- `string? LastSyncError`
- `string? AccessTokenRef`
- `DateTimeOffset? TokenExpiresAt`
- `DateTimeOffset? ConsentExpiresAt`
- `string? LastSyncCursor`
- `string? WebhookSubscriptionId`
- `bool IsDefault`
- `bool IsActive`
- `DateTimeOffset CreatedAt`
- `DateTimeOffset UpdatedAt`

Không còn:

- `AvailableBalance`
- `BalanceAsOf`

### Categories và jars

#### `Category`

Vai trò:

- phân loại transaction;
- có default category và custom category.

Cần `IsActive`, `DeletedAt`, `OwnerUserId`.

#### `JarSetup`

Vai trò:

- lưu method hiện tại của user: `SixJars`, `Rule503020`, `Custom`, `Undecided`.

Mỗi user có tối đa một `JarSetup`.

#### `Jar`

Vai trò:

- hũ ngân sách nội bộ;
- giữ `Balance`;
- được tham chiếu bởi transaction jar operation, goal contribution, spending limit.

Trường có thể có:

- `Guid Id`
- `Guid UserId`
- `Guid? JarSetupId`
- `string Name`
- `decimal Balance`
- `string Currency`
- `string? Color`
- `string? Icon`
- `bool IsDefault`
- `string Status`
- `DateTimeOffset CreatedAt`
- `DateTimeOffset UpdatedAt`

Không có:

- `Percentage`

Tỷ lệ chia hũ là rule trong service, không persist trong table `jars`.

## 7. Mô hình Transaction mới

### `Transaction`

Vai trò:

- lưu thu/chi chính thức;
- lưu operation liên quan hũ bằng `source_type = 'Jar'`;
- làm nguồn dữ liệu cho dashboard, limit, report.

Trường có thể có:

- `Guid Id`
- `Guid UserId`
- `Guid FinancialAccountId`
- `Guid? CategoryId`
- `Guid? ImportJobId`
- `string? ExternalTransactionId`
- `string Type`
- `decimal TransactionsAmount`
- `string? Note`
- `string? RawDescription`
- `DateTimeOffset TransactionDate`
- `string SourceType`
- `DateTimeOffset? PostedAt`
- `decimal? JarBalanceAfterAllocation`
- `Guid? FromJarId`
- `Guid? ToJarId`
- `string? RawPayloadJson`
- `bool IsDeleted`
- `DateTimeOffset? DeletedAt`
- `DateTimeOffset CreatedAt`
- `DateTimeOffset UpdatedAt`

Không còn:

- `Amount`
- `JarId`

Quy tắc:

- `Type = Income` thì `TransactionsAmount > 0`.
- `Type = Expense` thì `TransactionsAmount < 0`.
- Public API nhận amount dương từ FE; service chịu trách nhiệm chuyển thành giá trị có dấu trước khi lưu DB.
- `FromJarId` và `ToJarId` không được trùng nhau nếu đều có giá trị.
- Expense thông thường có thể dùng `FromJarId` để trừ hũ.
- Operation nội bộ ghi nhận tiền vào hũ có thể dùng `ToJarId` và `JarBalanceAfterAllocation`.
- Operation nội bộ chuyển giữa hũ có thể dùng cả `FromJarId` và `ToJarId`.
- Các operation hũ này không đồng nghĩa với việc có public API `jars/allocate` hoặc `jars/transfer` trong MVP.

## 8. Mô hình Import

### `ImportJob`

Vai trò:

- đại diện một lần upload/import sao kê;
- gắn với `FinancialAccountId` bắt buộc.

### `ImportTransactionDraft`

Trường có thể có:

- `Guid Id`
- `Guid ImportJobId`
- `int RowIndex`
- `DateTimeOffset? TransactionDate`
- `decimal? Amount`
- `string? Type`
- `string? RawDescription`
- `string? EditedNote`
- `Guid? EditedCategoryId`
- `Guid? EditedJarId`
- `bool IsValid`
- `string? ValidationError`
- `string? NormalizedPayloadJson`
- `DateTimeOffset CreatedAt`
- `DateTimeOffset UpdatedAt`

Không còn:

- `SuggestedNote`
- `SuggestedCategoryId`
- `SuggestedJarId`

## 9. Mô hình Planning

### `SpendingLimit`

Schema cho phép target theo jar hoặc category:

- `Guid? JarId`
- `Guid? CategoryId`

DB check hiện tại yêu cầu ít nhất một trong hai field có giá trị. Nếu sản phẩm muốn "chỉ một trong hai", cần sửa check constraint thành XOR.

### `Goal`

Goal có `LinkedJarId` nullable. `SavedAmount` là snapshot được update khi tạo contribution.

### `GoalContribution`

Trường có thể có:

- `Guid Id`
- `Guid GoalId`
- `Guid UserId`
- `Guid? SourceJarId`
- `decimal Amount`
- `string? Note`
- `DateTimeOffset CreatedAt`

Không có:

- `SourceFinancialAccountId`

Nếu đóng góp từ nguồn tiền thật, service nên tạo transaction tương ứng thay vì lưu source account trong contribution.

### `Reminder`

Trường có thể có:

- `Guid Id`
- `Guid UserId`
- `string Title`
- `decimal? Amount`
- `string? Frequency`
- `short? DayOfMonth`
- `DateTime StartDate`
- `Guid? CategoryId`
- `string? Note`
- `string Status`
- `short? NotifyDaysBefore`
- `DateTimeOffset CreatedAt`
- `DateTimeOffset UpdatedAt`

Không có:

- `NextDueDate`

Ngày nhắc tiếp theo được tính trong service/job từ `StartDate`, `Frequency`, `DayOfMonth`, `NotifyDaysBefore`.

## 10. Notification, admin, AI

### `Broadcast`

Dùng cho admin gửi thông báo hàng loạt.

### `Notification`

Notification inbox của user, có optional `BroadcastId`.

### `AuditLog`

Append-only log cho các thao tác quan trọng.

### `AiSetting`

Cấu hình AI toàn hệ thống. `ApiKeyEncrypted` không bao giờ trả ra API.

## 11. Bảng/entity bỏ khỏi MVP

Không setup entity, DbSet, migration cho các model sau:

- `JarAllocation`
- `JarAllocationItem`
- `JarTransfer`

Không setup field sau:

- `Account.HashPassword`
- `FinancialAccount.AvailableBalance`
- `FinancialAccount.BalanceAsOf`
- `Jar.Percentage`
- `Transaction.Amount`
- `Transaction.JarId`
- `ImportTransactionDraft.SuggestedNote`
- `ImportTransactionDraft.SuggestedCategoryId`
- `ImportTransactionDraft.SuggestedJarId`
- `GoalContribution.SourceFinancialAccountId`
- `Reminder.NextDueDate`

## 12. Ranh giới transaction theo use case

### Tạo transaction

Trong một DB transaction:

1. Load financial account, check ownership.
2. Validate category/jar nếu có.
3. Tạo `Transaction`.
4. Cập nhật `FinancialAccount.CurrentBalance`.
5. Nếu có `FromJarId` thì trừ `Jar.Balance`.
6. Nếu có `ToJarId` thì cộng `Jar.Balance`.
7. Evaluate spending limits nếu là expense.
8. Tạo notification nếu chạm ngưỡng.

### Operation nội bộ: phân bổ tiền vào jars

Trong một DB transaction:

1. Load financial account và jars.
2. Tính số tiền cho từng hũ theo phương pháp.
3. Cộng `Jar.Balance`.
4. Tạo một `Transaction` source `Jar` cho từng hũ, set `ToJarId`, `TransactionsAmount`, `JarBalanceAfterAllocation`.

Ghi chú:

- Đây là capability nội bộ cho service/job/admin khi cần tự động phân bổ hoặc repair dữ liệu.
- MVP hiện tại không expose public API `POST /api/v1/jars/allocate`.

### Operation nội bộ: chuyển giữa jars

Trong một DB transaction:

1. Load `fromJar`, `toJar`.
2. Check ownership và balance.
3. Trừ/cộng balance.
4. Tạo `Transaction` source `Jar`, set `FromJarId`, `ToJarId`.

Ghi chú:

- Đây là capability nội bộ nếu sau này sản phẩm cần hỗ trợ điều chỉnh số dư hũ.
- MVP hiện tại không expose public API `POST /api/v1/jars/transfer` hoặc `GET /api/v1/jars/transfers`.

### Xác nhận import (Confirm import)

Trong một DB transaction:

1. Load import job.
2. Load các drafts được chọn.
3. Validate lần cuối.
4. Tạo batch `transactions`.
5. Cập nhật financial account/jar balance nếu service rule yêu cầu.
6. Cập nhật import job status/count.

## 13. Kế hoạch setup entities cho `Personal_Finance_Management.Repository`

Đây là kế hoạch để đồng bộ folder `Personal_Finance_Management/Personal_Finance_Management.Repository` với schema mới.

### Bước 1 - Chốt danh sách entity

Giữ 18 entity:

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

Loại bỏ khỏi EF:

- `JarAllocation`
- `JarAllocationItem`
- `JarTransfer`

### Bước 2 - Sửa base abstractions

Hiện tại `BaseEntity` đang có `Guid Id` và `IsDeleted`, sẽ gây sai schema cho nhiều bảng.

Kế hoạch:

- Đổi `BaseEntity` chỉ còn `Guid Id`.
- Tạo interface `IHasCreatedAt`, `IHasUpdatedAt` nếu cần auto timestamp.
- Chỉ thêm `IsDeleted` vào `Transaction`.
- Category dùng `DeletedAt` và `IsActive`, không bắt buộc `IsDeleted`.
- `Role` có thể kế thừa `BaseEntity` vì PK là `Guid`.

### Bước 3 - Sửa scalar properties của entity

Sửa các mismatch lớn (tóm tắt):

- `Role`: `Id` là `Guid`.
- `Account`: `RoleId` là `Guid`; `HashPassword` -> `PasswordHash`.
- `FinancialAccount`: xóa `AvailableBalance`, `BalanceAsOf`.
- `OnboardingProfile`: `FinancialGoalTypes`, `SpendingChallenges` là `string?`.
- `Jar`: xóa `Percentage`.
- `Transaction`: `Amount` -> `TransactionsAmount`; xóa `JarId`; thêm `FromJarId`, `ToJarId`, `JarBalanceAfterAllocation`, `DeletedAt`.
- `ImportTransactionDraft`: `Suggested*` -> `Edited*`.
- `GoalContribution`: xóa `SourceFinancialAccountId`.
- `Reminder`: xóa `NextDueDate`; `Frequency` nullable.
- `AuditLog`, `Notification`, `Transaction`, `ImportTransactionDraft`: JSON column dùng `json`.

### Bước 4 - Sửa navigation properties

Cần tối thiểu navigation:

- `Account.Role`
- `Account.FinancialAccounts`
- `Account.OnboardingProfile`
- `Account.JarSetup`
- `Jar.JarSetup`
- `Transaction.User`
- `Transaction.FinancialAccount`
- `Transaction.Category`
- `Transaction.ImportJob`
- `Transaction.FromJar`
- `Transaction.ToJar`
- `ImportTransactionDraft.ImportJob`
- `ImportTransactionDraft.EditedCategory`
- `ImportTransactionDraft.EditedJar`
- `GoalContribution.Goal`
- `GoalContribution.User`
- `GoalContribution.SourceJar`

Không cần navigation ngược cho mọi quan hệ trong MVP. Thêm quá nhiều collection có thể làm entity phức tạp và khó query.

### Bước 5 - Sửa `AppDbContext`

Cần đồng bộ DbSet:

- Xóa `DbSet<JarAllocation>`
- Xóa `DbSet<JarAllocationItem>`
- Xóa `DbSet<JarTransfer>`

Sửa Fluent API:

- Table/column name theo snake_case.
- `roles.id` Guid.
- Check constraints giống `finjar_schema.sql`.
- Index giống schema:
  - accounts: role/status, last_login
  - financial_accounts: user/default/sync
  - jars: user/status
  - transactions: user/date, account/date, category/date, import job, from/to jar
  - notifications: unread
- JSON columns dùng `json`.
- Xóa mapping cho các table/field đã bỏ.

### Bước 6 - Tạo migration mới

Vì schema thay đổi nhiều, nếu DB local/test có thể reset:

1. Xóa migration cũ nếu đội chưa deploy.
2. Tạo migration mới từ entity đã sửa.
3. So sánh migration generated với `docs/finjar_schema.sql`.
4. Chạy update database trên DB dev.

Nếu DB đã có data cần giữ, cần migration chuyển đổi:

- rename `hash_password` -> `password_hash`
- migrate `transactions.amount` -> `transactions.transactions_amount`
- map `transactions.jar_id` sang `from_jar_id` hoặc `to_jar_id` theo rule sản phẩm
- drop old jar allocation/transfer tables sau khi migrate xong

### Bước 7 - Seed data

Seed tối thiểu:

- roles: `User`, `Admin`
- default categories nếu sản phẩm cần
- optional default AI setting nếu muốn AI có config ban đầu

Role seed phải dùng id Guid ổn định:

- `00000000-0000-0000-0000-000000000001 = User`
- `00000000-0000-0000-0000-000000000002 = Admin`

### Bước 8 - Verification

Sau khi setup entity:

1. `dotnet build`
2. Tạo migration
3. Review migration SQL
4. Chạy update database trên DB dev/test
5. Test nhanh:
   - register/login
   - create financial account
   - complete onboarding để tạo default jars
   - create income/expense transaction
   - create import job/draft
   - create goal contribution

## 14. Thứ tự implement để ít lỗi nhất

1. Sửa base abstractions và `Role`.
2. Sửa các entity scalar fields cho khớp schema.
3. Xóa entity/DbSet jar allocation/transfer.
4. Sửa `Transaction` và các navigation liên quan jar.
5. Sửa `ImportTransactionDraft`.
6. Sửa `GoalContribution`, `Reminder`, `FinancialAccount`.
7. Viết lại `AppDbContext.OnModelCreating`.
8. Build.
9. Tạo migration.
10. So sánh migration với `finjar_schema.sql`.

## 15. Ghi chú cho team

- Trong MVP, dùng string enum + validation ở service là đủ.
- Dùng `DateTimeOffset` cho `timestamptz`.
- Không viết business rule trong entity EF quá sớm; ưu tiên service layer rõ ràng.
- Mọi API thao tác dữ liệu user phải check `UserId`.
- FE gửi transaction amount dương; service chuẩn hóa trước khi lưu.
- Transaction amount trong DB đã có dấu: Income dương, Expense âm. Dùng quy ước này nhất quán từ service đến query/report.
