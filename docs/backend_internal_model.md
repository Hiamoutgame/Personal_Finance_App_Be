# Personal Finance Manager — Backend Internal Model

Tài liệu này mô tả **mô hình nội bộ của backend** cho hệ thống **Personal Finance Manager**. Mục tiêu của tài liệu là giúp team backend thống nhất cách tổ chức domain, application flow, transaction boundary, và quy tắc map giữa API contract với model nội bộ.

Tài liệu này đi cùng với:

- [overview.md](/d:/Coding/Project/prj_pied/Personal_Finance_App_Be/docs/overview.md)
- [user story.md](/d:/Coding/Project/prj_pied/Personal_Finance_App_Be/docs/user%20story.md)
- [apis.md](/d:/Coding/Project/prj_pied/Personal_Finance_App_Be/docs/apis.md)

## 1. Mục tiêu của internal model

`apis.md` là phần contract public giữa frontend và backend.  
`backend_internal_model.md` là phần mô tả cách backend **tự xử lý bên trong** để:

- giữ request/response cho frontend gọn và rõ;
- không lộ trực tiếp database schema ra ngoài API;
- gom business logic vào đúng layer;
- đảm bảo tính nhất quán dữ liệu cho các nghiệp vụ tiền tệ;
- dễ mở rộng cho AI, import, notification, audit log về sau.

Nói ngắn gọn:

- **Frontend thấy DTO đơn giản**
- **Backend xử lý bằng internal command, aggregate, domain event, transaction**

## 2. Nguyên tắc thiết kế

### 2.1. API contract không phải database schema

Backend không trả thẳng entity DB ra ngoài và cũng không bắt frontend gửi đúng shape của bảng dữ liệu.

Ví dụ:

- FE gửi `CreateTransactionRequest`
- Backend tự map sang `Transaction`, `JarBalanceAdjustment`, `LimitEvaluation`, `Notification`

### 2.2. Thin write, rich read

- Các API ghi dữ liệu như `POST`, `PUT`, `PATCH`, `DELETE` nên nhận request gọn, đúng ý định nghiệp vụ.
- Các API đọc dữ liệu như `GET /dashboard`, `GET /transactions`, `GET /goals` có thể trả response phong phú hơn để FE render màn hình.

### 2.3. Domain-driven theo use case

Không thiết kế hệ thống xoay quanh table CRUD đơn thuần.  
Thiết kế nội bộ nên xoay quanh các hành động nghiệp vụ:

- đăng ký, đăng nhập, refresh token;
- onboarding;
- setup jars;
- allocate income;
- transfer giữa jars;
- tạo/sửa/xóa transaction;
- evaluate limits;
- contribute vào goal;
- import statement;
- broadcast notification;
- AI settings.

### 2.4. Tiền tệ luôn dùng decimal

Mọi giá trị liên quan tới tiền:

- balance
- amount
- targetAmount
- limitAmount
- transactionVolume

đều dùng `decimal`.

### 2.5. Atomic ở application layer

Các nghiệp vụ có side effect nhiều bảng phải nằm trong cùng transaction:

- `AllocateIncomeToJars`
- `TransferBetweenJars`
- `CreateExpenseTransaction`
- `UpdateTransactionAmount`
- `DeleteTransaction`
- `ContributeToGoal`
- `ConfirmImportTransactions`
- `BanUser`

## 3. Kiến trúc layer đề xuất

Backend có thể tổ chức theo hướng clean architecture hoặc modular monolith với 4 lớp chính:

### 3.1. API Layer

Chứa:

- Controllers / Minimal APIs
- Request DTO
- Response DTO
- Auth decorators / policies
- Model binding

Không chứa:

- logic cập nhật balance
- logic evaluate limit
- logic sinh notification
- logic audit

### 3.2. Application Layer

Chứa:

- Commands / Queries
- Command handlers / Query handlers
- Business orchestration
- Transaction boundary
- Mapping DTO -> domain input -> result DTO

Ví dụ:

- `CreateTransactionCommand`
- `TransferBetweenJarsCommand`
- `GetDashboardQuery`
- `ConfirmImportJobCommand`

### 3.3. Domain Layer

Chứa:

- Aggregate roots
- Entities
- Value objects
- Enums
- Domain rules
- Domain events

Ví dụ:

- `Jar`
- `FinancialGoal`
- `SpendingLimit`
- `Money`
- `TransactionType`
- `JarTransferredEvent`

### 3.4. Infrastructure Layer

Chứa:

- EF Core DbContext
- Repositories
- Token service
- BCrypt password hashing
- Hangfire jobs
- AES/DataProtection cho AI API key
- OCR/import parsers
- AI provider adapter
- email/push/in-app notification delivery

## 4. Flow tổng quát giữa FE và BE

```text
Frontend DTO
-> API Controller
-> Application Command / Query
-> Domain Aggregate + Services
-> Repository / DbContext / External Services
-> Domain Events
-> Response Mapper
-> Frontend DTO
```

Điểm quan trọng là:

- frontend không biết backend dùng bao nhiêu entity nội bộ;
- backend không bắt frontend gửi các field tính toán;
- response trả về shape phù hợp màn hình thay vì shape của DB.

## 5. Shared building blocks

## 5.1. Base entity

Phần lớn entity nghiệp vụ dùng UUID trong `finjar_schema.sql` có thể dùng base structure thống nhất:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
```

Không phải mọi entity đều cần soft delete. Với phase 1 theo `finjar_schema.sql`, chỉ thêm trạng thái soft delete/active đúng các bảng có cột tương ứng, ví dụ:

- transactions
- categories

Các bảng còn lại dùng status hoặc `is_active` nếu schema đã có, không ép mọi entity phải có `IsDeleted`.

Ngoại lệ như `roles.id` dùng `smallint` seed cố định thì không cần kế thừa base UUID này.

## 5.2. Auditable entity

Với các entity nhạy cảm có thể thêm:

```csharp
public abstract class AuditableEntity : BaseEntity
{
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }
}
```

## 5.3. Value objects gợi ý

Các value object nên được dùng để giảm lỗi logic:

- `Money`
- `DateRange`
- `Percentage`
- `FullName`
- `EmailAddress`
- `PhoneNumber`
- `ColorHex`

Ví dụ:

```csharp
public sealed class Money
{
    public decimal Value { get; }
    public string Currency { get; }
}
```

Nếu team muốn đơn giản hơn ở phase đầu, có thể chưa implement value object đầy đủ ở EF layer, nhưng vẫn nên dùng tư duy value object ở application/domain validation.

## 5.4. Enum nội bộ chính

- `UserRole`: `User`, `Admin`, `SuperAdmin`
- `AccountStatus`: `Active`, `Banned`
- `FinancialAccountType`: `Cash`, `Bank`, `EWallet`, `Other`
- `ConnectionMode`: `Manual`, `LinkedApi`
- `SyncStatus`: `NeverSynced`, `Synced`, `Syncing`, `Error`, `Disconnected`
- `BudgetMethodType`: `SixJars`, `Rule503020`, `Custom`, `Undecided`
- `JarStatus`: `Active`, `Paused`, `Archived`
- `TransactionType`: `Income`, `Expense`
- `TransactionSourceType`: `Manual`, `Imported`, `OCR`, `Synced`, `System`
- `LimitTargetType`: `Jar`, `Category`
- `LimitPeriodType`: `Daily`, `Monthly`
- `NotificationType`: `SpendingAlert`, `GoalUpdate`, `Reminder`, `System`, `Broadcast`
- `GoalStatus`: `Active`, `Completed`, `Cancelled`
- `ReminderFrequency`: `Daily`, `Weekly`, `Monthly`, `Quarterly`, `Yearly`
- `ReminderStatus`: `Active`, `Paused`, `Completed`, `Cancelled`
- `ImportJobStatus`: `Pending`, `Processing`, `AwaitingReview`, `Completed`, `Failed`
- `BroadcastStatus`: `Queued`, `Sent`, `Failed`, `Cancelled`

## 6. Aggregate và entity nội bộ

Phần này là lõi của internal model. Không nhất thiết aggregate nào cũng map 1-1 với table, nhưng đây là cách chia domain hợp lý để backend tổ chức code.

## 6.1. Identity & Access aggregate

### A. `UserAccount`

**Vai trò**

- đại diện cho tài khoản người dùng cuối;
- phục vụ auth, profile, status và ownership check.

**Field chính**

- `Id`
- `Username`
- `Email`
- `PasswordHash`
- `FullName`
- `Phone`
- `AvatarUrl`
- `Role`
- `Status`
- `IsOnboardingCompleted`
- `PreferredCurrency`
- `LastLoginAtUtc`

**Quan hệ**

- 1 user có nhiều categories custom
- 1 user có nhiều financial accounts
- 1 user có nhiều jars
- 1 user có nhiều transactions
- 1 user có nhiều import jobs
- 1 user có nhiều goals
- 1 user có nhiều notifications
- 1 user có nhiều reminders

### B. `RefreshTokenSession`

**Vai trò**

- quản lý vòng đời refresh token;
- hỗ trợ revoke, rotate, logout.

**Field chính**

- `Id`
- `UserId`
- `TokenHash`
- `ExpiresAtUtc`
- `RevokedAtUtc`
- `ReplacedByTokenHash`
- `CreatedByIp`
- `RevokedByIp`
- `UserAgent`
- `CreatedAtUtc`

### C. `AdminAccount`

Có thể dùng chung bảng với `UserAccount` theo `Role`, hoặc tách riêng tùy chiến lược auth của team.  
Nếu dùng chung, vẫn nên có policy tách rõ cho admin endpoints.

## 6.2. Onboarding aggregate

### `UserOnboardingProfile`

**Vai trò**

- lưu kết quả khảo sát ban đầu;
- làm input cho gợi ý jar/category/budget method;
- dùng làm context cho AI về sau.

**Field chính**

- `Id`
- `UserId`
- `MonthlyIncome`
- `OccupationType`
- `FinancialGoalTypes`
- `BudgetMethodPreference`
- `AgeRange`
- `SpendingChallenges`
- `RecommendedMethod`
- `CompletedAtUtc`

**Ghi chú**

- Theo `finjar_schema.sql`, các trường nhiều giá trị như `FinancialGoalTypes`, `SpendingChallenges` lưu bằng PostgreSQL `TEXT[]` trong phase 1.

## 6.3. Financial Account aggregate

### `FinancialAccount`

**Vai trò**

- đại diện cho nguồn tiền thật mà user đang theo dõi;
- có thể là tiền mặt thủ công, tài khoản ngân hàng liên kết, ví điện tử hoặc nguồn khác;
- là điểm gắn bắt buộc cho transaction và import job.

**Field chính**

- `Id`
- `UserId`
- `Name`
- `AccountType`
- `ConnectionMode`
- `ProviderCode`
- `ProviderName`
- `ExternalAccountId`
- `ExternalAccountRef`
- `MaskedAccountNumber`
- `AccountHolderName`
- `Currency`
- `CurrentBalance`
- `AvailableBalance`
- `BalanceAsOfUtc`
- `SyncStatus`
- `LastSyncedAtUtc`
- `LastSyncError`
- `AccessTokenRef`
- `RefreshTokenRef`
- `TokenExpiresAtUtc`
- `ConsentExpiresAtUtc`
- `LastSyncCursor`
- `WebhookSubscriptionId`
- `IsDefault`
- `IsActive`

**Rule nội bộ**

- `CurrentBalance >= 0`
- `Manual` account có thể không có provider metadata
- `LinkedApi` account phải có metadata provider đủ để sync/đối chiếu
- token thật không trả ra API và không lưu raw nếu tránh được; chỉ lưu encrypted/ref
- mọi thao tác theo `FinancialAccountId` phải kiểm tra ownership theo `UserId`

**Ghi chú non-custodial**

- `FinancialAccount` không phải ví do FinJar phát hành.
- App chỉ lưu dữ liệu theo dõi/đồng bộ/đối chiếu; không giữ tiền và không tự thực hiện chuyển khoản ngân hàng.
- Khi user phân bổ tiền vào jar, tiền thật vẫn ở nguồn tiền gốc; backend chỉ cập nhật lớp ngân sách nội bộ.

## 6.4. Category aggregate

### `Category`

**Vai trò**

- phân loại giao dịch;
- hỗ trợ phân tích spending;
- có hai loại: default category và custom category.

**Field chính**

- `Id`
- `Name`
- `Icon`
- `Color`
- `IsDefault`
- `OwnerUserId` nullable
- `DisplayOrder`
- `IsActive`
- `DeletedAtUtc`

**Rule nội bộ**

- `IsDefault = true` thì `OwnerUserId = null`
- category custom chỉ được sửa/xóa bởi owner
- default category soft delete để giữ lịch sử cũ

## 6.5. Budget Setup aggregate

### A. `JarSetupProfile`

**Vai trò**

- lưu cách setup ngân sách hiện tại của user;
- xác định user đang dùng `SixJars`, `Rule503020` hay `Custom`.

**Field chính**

- `Id`
- `UserId`
- `MethodType`
- `CreatedAtUtc`
- `UpdatedAtUtc`

**Ghi chú**

- `jar_setups` không lưu `InitialBalance`; số dư gốc nằm ở `financial_accounts`.
- Nếu user muốn phân bổ số dư vào hũ, backend tạo `JarAllocation` và `JarAllocationItem`.

### B. `Jar`

**Vai trò**

- đơn vị ngân sách chính;
- giữ balance hiện tại;
- là trung tâm của allocate, transfer, expense, goal contribution.

**Field chính**

- `Id`
- `UserId`
- `JarSetupProfileId` nullable
- `Name`
- `Percentage`
- `Balance`
- `Currency`
- `Color`
- `Icon`
- `IsDefault`
- `Status`

**Rule nội bộ**

- `Balance` không được âm
- tổng `Percentage` = `100` với custom method
- jar archived không được dùng cho giao dịch mới

### C. `JarTransfer`

**Vai trò**

- lưu lịch sử chuyển ngân sách nội bộ giữa các hũ;
- không thay thế cho `Transaction`, mà là record nghiệp vụ riêng.

**Field chính**

- `Id`
- `UserId`
- `FromJarId`
- `ToJarId`
- `Amount`
- `Note`
- `CreatedAtUtc`

**Rule nội bộ**

- `Amount > 0`
- `FromJarId != ToJarId`
- chỉ cập nhật `Jar.Balance` hai bên; không gọi API ngân hàng và không tạo transaction thu/chi.

### D. `JarAllocation`

**Vai trò**

- lưu sự kiện phân bổ ngân sách vào các hũ;
- giúp truy vết nguồn allocation.

**Field chính**

- `Id`
- `UserId`
- `SourceFinancialAccountId` nullable
- `TotalAmount`
- `Note`
- `CreatedAtUtc`

**Ghi chú**

- `SourceFinancialAccountId` map vào `jar_allocations.source_financial_account_id`.
- Field này chỉ quy chiếu allocation đến nguồn tiền đang theo dõi, không đại diện cho lệnh rút/chuyển tiền thật.

### E. `JarAllocationItem`

**Field chính**

- `Id`
- `AllocationId`
- `JarId`
- `Amount`
- `BalanceAfterAllocation`

## 6.6. Transaction aggregate

### `Transaction`

**Vai trò**

- lưu các giao dịch thu/chi được người dùng xác nhận;
- là nguồn dữ liệu chính cho dashboard, limits, category breakdown.

**Field chính**

- `Id`
- `UserId`
- `FinancialAccountId`
- `ImportJobId` nullable
- `Type`
- `Amount`
- `JarId` nullable với income
- `CategoryId` nullable
- `Note`
- `RawDescription`
- `TransactionDateUtc`
- `PostedAtUtc` nullable
- `SourceType`
- `ExternalTransactionId` nullable
- `RawPayloadJson` nullable
- `IsDeleted`
- `DeletedAtUtc`

**SourceType gợi ý**

- `Manual`
- `Imported`
- `OCR`
- `Synced`
- `System`

**Rule nội bộ**

- mọi transaction phải có `FinancialAccountId`
- `FinancialAccountId` là nguồn tiền phát sinh giao dịch, không phải ví nội bộ của FinJar
- `Expense` bắt buộc có `JarId`
- `Amount > 0`
- xóa transaction là soft delete
- update amount/source account/jar phải tính delta để đảo tác động cũ và áp dụng tác động mới
- `Transfer` giữa jar không nằm trong `TransactionType`; nó dùng `JarTransfer`

### Có cần bảng ledger không?

Có 2 hướng:

**Hướng A: đơn giản cho phase đầu**

- chỉ lưu `Jar.Balance`
- lưu `FinancialAccount.CurrentBalance` như số dư theo dõi/snapshot của nguồn tiền
- khi transfer/expense/contribution thì cập nhật trực tiếp balance
- lưu lịch sử qua `Transaction`, `JarTransfer`, `GoalContribution`, `JarAllocation`

**Hướng B: chuẩn hóa hơn**

- thêm bảng `BalanceLedgerEntry`
- mọi thay đổi số dư đều qua ledger
- `Jar.Balance` là snapshot/cache

Khuyến nghị cho phase đầu:

- dùng **Hướng A** để đội không bị quá tải;
- nếu sau này hệ thống cần audit tiền tệ sâu hơn thì thêm ledger ở phase sau.

## 6.7. Limits & Alerts aggregate

### A. `SpendingLimit`

**Vai trò**

- định nghĩa hạn mức chi tiêu theo jar hoặc category.

**Field chính**

- `Id`
- `UserId`
- `TargetType`
- `TargetId`
- `LimitAmount`
- `Period`
- `AlertAtPercentage`
- `IsActive`

### B. `LimitEvaluationSnapshot`

Không nhất thiết phải persist ngay từ đầu, nhưng có thể có nếu team muốn tối ưu query hoặc lưu lịch sử đánh giá.

**Field chính**

- `Id`
- `LimitId`
- `CurrentSpent`
- `CurrentPercentage`
- `Status`
- `EvaluatedAtUtc`

### C. `Notification`

**Vai trò**

- hộp thư thông báo in-app cho user.

**Field chính**

- `Id`
- `UserId`
- `Type`
- `Title`
- `Body`
- `IsRead`
- `ReadAtUtc`
- `BroadcastId` nullable
- `MetadataJson`
- `CreatedAtUtc`

**MetadataJson có thể chứa**

- `limitId`
- `goalId`
- `broadcastId`
- `currentPercentage`
- `reminderId`

## 6.8. Goals aggregate

### A. `FinancialGoal`

**Vai trò**

- quản lý mục tiêu tiết kiệm.

**Field chính**

- `Id`
- `UserId`
- `Title`
- `TargetAmount`
- `SavedAmount`
- `DueDate`
- `Status`
- `LinkedJarId` nullable
- `Note`

**Derived data**

- `ProgressPercentage`
- `RemainingAmount`
- `DaysRemaining`
- `SuggestedMonthlyContribution`

Các field derived có thể:

- tính trực tiếp khi query; hoặc
- cache một phần trên bảng nếu cần tối ưu.

### B. `GoalContribution`

**Vai trò**

- lưu lịch sử đóng góp vào goal;
- là basis để update `SavedAmount`.

**Field chính**

- `Id`
- `GoalId`
- `UserId`
- `Amount`
- `SourceJarId` nullable
- `SourceFinancialAccountId` nullable
- `Note`
- `CreatedAtUtc`

**Rule nội bộ**

- chỉ set một trong hai: `SourceJarId` hoặc `SourceFinancialAccountId`
- nếu có `SourceJarId` thì phải trừ `Jar.Balance` atomic
- nếu có `SourceFinancialAccountId` thì phải kiểm tra ownership; không mặc định trừ `FinancialAccount.CurrentBalance` trừ khi product chốt đây là giao dịch làm giảm số dư
- sau contribution phải cập nhật lại `SavedAmount` và `GoalStatus`
- contribution không đại diện cho lệnh chuyển tiền thật vào goal

## 6.9. Reminder aggregate

### `RecurringReminder`

**Vai trò**

- quản lý lịch nhắc thanh toán định kỳ.

**Field chính**

- `Id`
- `UserId`
- `Title`
- `Amount`
- `Frequency`
- `DayOfMonth` nullable
- `StartDate`
- `NextDueDate`
- `CategoryId` nullable
- `Note`
- `Status`
- `NotifyDaysBefore`

**Xử lý**

- Hangfire job quét reminder active
- sinh notification khi gần đến hạn
- update `NextDueDate` sau mỗi chu kỳ

## 6.10. Import aggregate

### A. `ImportJob`

**Vai trò**

- đại diện cho một phiên import file sao kê.

**Field chính**

- `Id`
- `UserId`
- `FinancialAccountId`
- `FileName`
- `OriginalContentType`
- `StoredFilePath`
- `BankCode`
- `Status`
- `Progress`
- `EstimatedRows`
- `ParsedCount`
- `FailedCount`
- `ErrorMessage`
- `UploadedAtUtc`
- `UpdatedAtUtc`

### B. `ImportedTransactionDraft`

**Vai trò**

- lưu dữ liệu parse ra để user review trước khi confirm.

**Field chính**

- `Id`
- `ImportJobId`
- `RowIndex`
- `Date`
- `Amount`
- `Type`
- `RawDescription`
- `SuggestedNote`
- `SuggestedCategoryId`
- `SuggestedJarId`
- `IsValid`
- `ValidationError`
- `NormalizedPayloadJson`

**Rule nội bộ**

- `FinancialAccountId` bắt buộc vì `import_jobs.financial_account_id` là `NOT NULL`
- chỉ khi user confirm mới sinh `Transaction` thật
- transaction sinh ra từ import kế thừa `FinancialAccountId` của import job
- nếu confirm nhiều row thì insert trong 1 transaction

## 6.11. Admin Operations aggregate

### A. `BroadcastNotification`

**Vai trò**

- quản lý đợt gửi thông báo hệ thống cho nhiều user.

**Field chính**

- `Id`
- `Title`
- `Body`
- `TargetAudience`
- `ScheduledAtUtc`
- `SentAtUtc`
- `Status`
- `TargetCount`
- `DeliveredCount`
- `CreatedByAdminId`
- `CreatedAtUtc`
- `UpdatedAtUtc`

### B. `AuditLog`

**Vai trò**

- theo dõi thao tác nhạy cảm của admin;
- append-only.

**Field chính**

- `Id`
- `ActorAccountId`
- `ActionType`
- `EntityType`
- `EntityId`
- `Description`
- `MetadataJson`
- `IpAddress`
- `CreatedAtUtc`

**Rule nội bộ**

- không update
- không delete

### C. `AiSetting`

**Vai trò**

- lưu cấu hình AI toàn hệ thống.

**Field chính**

- `Id`
- `ModelName`
- `SystemPrompt`
- `Temperature`
- `MaxTokens`
- `IsEnabled`
- `EncryptedApiKey`
- `LastUpdatedAtUtc`
- `LastUpdatedByAdminId`

## 6.12. Phase-sau models chưa có trong `finjar_schema.sql`

Các model dưới đây từng được nhắc trong scope mở rộng, nhưng **không có bảng trong `finjar_schema.sql` phase 1**. Vì vậy backend không nên scaffold persistence hoặc migration cho chúng trong giai đoạn hiện tại.

### A. `ChatSession` và `ChatMessage`

- trạng thái: optional/phase sau;
- nếu cần làm chatbot MVP trước khi có bảng riêng, có thể xử lý stateless hoặc chỉ dùng context từ dashboard/transactions/goals;
- khi muốn lưu lịch sử chat thật, cần bổ sung migration riêng cho `chat_sessions` và `chat_messages`.

## 7. Quan hệ nghiệp vụ quan trọng

## 7.1. User là root ownership

Hầu hết dữ liệu nghiệp vụ đều phải gắn với `UserId`:

- `Jar`
- `FinancialAccount`
- `Transaction`
- `Category` custom
- `FinancialGoal`
- `RecurringReminder`
- `Notification`
- `ImportJob`

Điều này giúp:

- filter dữ liệu đơn giản;
- kiểm tra ownership dễ;
- tránh leak dữ liệu giữa user.

## 7.2. Category có hai nguồn

- default category do admin quản lý;
- custom category do user tạo.

Backend nội bộ cần luôn kiểm tra:

- category tồn tại;
- category active;
- category thuộc user hiện tại hoặc là default.

## 7.3. Jar balance là derived-but-persisted state

`Jar.Balance` là trạng thái quan trọng và được persist để query nhanh.  
Nhưng nó là kết quả của các nghiệp vụ:

- allocate
- expense transaction
- transfer out/in
- goal contribution từ jar
- transaction delete/update

Vì vậy mọi thao tác chạm vào `Jar.Balance` đều phải đi qua application service hoặc domain method, không cho sửa tự do.

## 8. Mapping giữa API DTO và internal model

## 8.1. Request DTO không map thẳng vào entity

Ví dụ:

`POST /api/v1/transactions`

Frontend gửi:

```json
{
  "financialAccountId": "guid",
  "type": "Expense",
  "amount": 55000,
  "jarId": "guid",
  "categoryId": "guid",
  "note": "Cà phê sáng"
}
```

Backend nên map thành:

```text
CreateTransactionRequestDto
-> CreateTransactionCommand
-> Domain validation
-> Transaction entity
-> FinancialAccount balance update
-> Jar balance update
-> Limit evaluation
-> Optional notification creation
-> CreateTransactionResult
-> TransactionResponseDto
```

FE không cần biết backend đã tạo thêm các object gì bên trong.

## 8.2. Query response nên được build từ read model

Ví dụ `GET /dashboard` không nên cố map từ một aggregate duy nhất.  
Nó nên được compose từ nhiều nguồn:

- transactions
- jars
- goals
- limits
- categories

Backend có thể dùng:

- query service riêng;
- SQL projection;
- LINQ projection read-only.

## 8.3. Internal command/result mẫu

Ví dụ cho nghiệp vụ transfer:

```csharp
public sealed record TransferBetweenJarsCommand(
    Guid UserId,
    Guid FromJarId,
    Guid ToJarId,
    decimal Amount,
    string? Note);

public sealed record TransferBetweenJarsResult(
    Guid TransferId,
    Guid FromJarId,
    decimal FromJarNewBalance,
    Guid ToJarId,
    decimal ToJarNewBalance,
    DateTime CreatedAtUtc);
```

Controller chỉ nhận request DTO và trả response DTO.  
Handler mới là nơi load jars, validate balance, update balance, lưu transfer record.

## 9. Transaction boundary theo nghiệp vụ

## 9.1. Allocate ngân sách vào jars

Trong cùng transaction:

- load `sourceFinancialAccount` nếu command có `SourceFinancialAccountId`
- load tất cả jars của user
- tính amount theo percentage
- update balance từng jar
- tạo `JarAllocation`
- tạo `JarAllocationItem`
- không gọi API ngân hàng; đây chỉ là phân bổ ngân sách nội bộ

Nếu bất kỳ bước nào lỗi thì rollback toàn bộ.

## 9.2. Transfer giữa jars

Trong cùng transaction:

- load `fromJar`, `toJar`
- kiểm tra ownership
- kiểm tra `fromJar.Balance >= amount`
- trừ `fromJar.Balance`
- cộng `toJar.Balance`
- tạo `JarTransfer`
- không cập nhật `FinancialAccount.CurrentBalance` vì đây là chuyển ngân sách nội bộ

## 9.3. Create transaction

Trong cùng transaction:

- validate financial account
- validate category/jar nếu là expense
- kiểm tra balance nếu transaction làm giảm số dư
- tạo `Transaction`
- cập nhật `FinancialAccount.CurrentBalance` theo loại giao dịch
- nếu là expense có gắn jar thì trừ `Jar.Balance`
- nếu là expense thì evaluate `SpendingLimit`
- nếu vượt threshold thì tạo `Notification`

## 9.4. Update transaction

Trong cùng transaction:

- load transaction cũ
- tính `delta`
- update transaction
- update `FinancialAccount.CurrentBalance` theo delta hoặc đảo nguồn cũ/áp nguồn mới nếu đổi `FinancialAccountId`
- update `Jar.Balance` theo delta hoặc đảo jar cũ/áp jar mới nếu đổi `JarId`
- re-evaluate limit nếu là expense

## 9.5. Delete transaction

Trong cùng transaction:

- soft delete transaction
- hoàn/đảo tác động trên `FinancialAccount.CurrentBalance`
- hoàn balance cho jar nếu là expense
- re-evaluate limit nếu cần

## 9.6. Contribute goal

Trong cùng transaction:

- load goal
- nếu có `sourceJarId` thì load jar và check balance
- nếu có `sourceFinancialAccountId` thì load financial account và check ownership
- tạo `GoalContribution`
- tăng `FinancialGoal.SavedAmount`
- nếu có source jar thì trừ balance
- nếu có source financial account thì chỉ ghi nhận nguồn; không mặc định trừ `FinancialAccount.CurrentBalance` nếu không có transaction thật tương ứng
- cập nhật `GoalStatus`
- nếu hoàn thành thì có thể tạo notification

## 9.7. Confirm import

Trong cùng transaction:

- load import job
- load selected drafts
- validate lần cuối
- tạo hàng loạt `Transaction` với `FinancialAccountId` kế thừa từ import job
- cập nhật `FinancialAccount.CurrentBalance` theo các transaction được tạo
- trừ balance cho các expense liên quan
- update import job status

## 10. Domain events gợi ý

Không bắt buộc phải event-driven toàn diện ngay từ đầu, nhưng có thể phát domain events để tách side effects khỏi command handler chính.

Các event hữu ích:

- `UserRegisteredEvent`
- `UserLoggedInEvent`
- `AdminLoggedInEvent`
- `OnboardingCompletedEvent`
- `JarsSetupCompletedEvent`
- `IncomeAllocatedEvent`
- `JarTransferredEvent`
- `ExpenseTransactionCreatedEvent`
- `TransactionDeletedEvent`
- `LimitThresholdReachedEvent`
- `GoalCompletedEvent`
- `ReminderDueSoonEvent`
- `BroadcastQueuedEvent`
- `AiSettingsUpdatedEvent`

## 11. Read model và query model

Một số màn hình không nên query trực tiếp từ aggregate write model một cách ngây thơ.

## 11.1. DashboardReadModel

Có thể build từ projection:

- `BalanceSummary`
- `JarSummaryItem`
- `CategoryBreakdownItem`
- `RecentTransactionItem`
- `GoalProgressSnapshotItem`

## 11.2. TransactionListReadModel

Trả về:

- thông tin transaction
- financial account name/type
- jar name/color
- category name/icon
- pagination

## 11.3. AdminUserDetailReadModel

Compose từ:

- user profile
- onboarding summary
- jar count
- total balance
- transaction count
- goal count

Điểm chính là query model có thể join nhiều nguồn để FE chỉ gọi một API.

## 12. Xử lý validation và error mapping

## 12.1. Validation layer

Validation nên chia 3 tầng:

- **Request validation**: `FluentValidation`
- **Business validation**: trong application/domain
- **Persistence/integrity validation**: unique index, FK, transaction

Ví dụ:

- `amount > 0` là request validation
- `jar balance đủ` là business validation
- `username unique` là DB integrity + application check

## 12.2. Error code nội bộ

Nên có bộ error code chuẩn để mapping ra API error response:

- `USERNAME_ALREADY_EXISTS`
- `EMAIL_ALREADY_EXISTS`
- `INVALID_CREDENTIALS`
- `ACCOUNT_BANNED`
- `JARS_ALREADY_SETUP`
- `INSUFFICIENT_BALANCE`
- `CATEGORY_IN_USE`
- `LIMIT_ALREADY_EXISTS`
- `IMPORT_JOB_INVALID_STATE`
- `GOAL_ALREADY_COMPLETED`
- `FORBIDDEN_RESOURCE_ACCESS`

## 13. Đề xuất module/folder structure trong .NET 8

```text
src/
  PersonalFinance.Api/
    Controllers/
    Contracts/
    Middleware/
    Extensions/

  PersonalFinance.Application/
    Abstractions/
    Auth/
    Users/
    FinancialAccounts/
    Categories/
    Jars/
    Transactions/
    Dashboard/
    Limits/
    Notifications/
    Goals/
    Reminders/
    Importing/
    Admin/

  PersonalFinance.Domain/
    Common/
    Users/
    FinancialAccounts/
    Categories/
    Budgeting/
    Transactions/
    Limits/
    Goals/
    Notifications/
    Reminders/
    Importing/
    Admin/
    Events/

  PersonalFinance.Infrastructure/
    Persistence/
    Repositories/
    Identity/
    Security/
    Jobs/
    AI/
    OCR/
    Files/
    Messaging/
```

## 14. Bản đồ mapping nhanh giữa API và internal model

| API use case | Internal command/query | Aggregate chính | Side effects |
| --- | --- | --- | --- |
| Register | `RegisterUserCommand` | `UserAccount` | tạo refresh session nếu cần |
| Login | `LoginUserCommand` | `UserAccount`, `RefreshTokenSession` | update last login |
| Onboarding | `CompleteOnboardingCommand` | `UserOnboardingProfile` | gợi ý jars |
| Financial accounts | `Create/UpdateFinancialAccountCommand` | `FinancialAccount` | default source, balance tracking |
| Setup jars | `SetupJarsCommand` | `JarSetupProfile`, `Jar` | tạo jars mặc định/custom |
| Allocate | `AllocateIncomeToJarsCommand` | `FinancialAccount`, `Jar`, `JarAllocation` | update jar balances |
| Transfer | `TransferBetweenJarsCommand` | `Jar`, `JarTransfer` | update balances |
| Create transaction | `CreateTransactionCommand` | `FinancialAccount`, `Transaction`, `Jar` | update tracked balances, evaluate limits, notifications |
| Dashboard | `GetDashboardQuery` | read models | aggregate nhiều nguồn |
| Create limit | `CreateSpendingLimitCommand` | `SpendingLimit` | none |
| Goal contribute | `ContributeToGoalCommand` | `FinancialGoal`, `GoalContribution`, `Jar`, `FinancialAccount` | update status, notify |
| Reminder create | `CreateReminderCommand` | `RecurringReminder` | job scheduling |
| Import confirm | `ConfirmImportJobCommand` | `ImportJob`, `ImportedTransactionDraft`, `FinancialAccount`, `Transaction` | update tracked balances |
| Ban user | `UpdateUserStatusCommand` | `UserAccount`, `AuditLog` | audit append |
| Broadcast | `CreateBroadcastCommand` | `BroadcastNotification` | background dispatch |
| AI settings | `UpdateAiSettingsCommand` | `AiSetting` | encrypt key, audit |

## 15. Kết luận

Nếu `apis.md` trả lời câu hỏi: **frontend nên gọi backend như thế nào**,  
thì tài liệu này trả lời câu hỏi: **backend nên tự tổ chức bên trong ra sao để xử lý được những contract đó một cách sạch, an toàn và mở rộng được**.

Tư tưởng cốt lõi của internal model này là:

- API bên ngoài gọn và rõ cho FE;
- logic phức tạp được giữ trong backend;
- transaction boundaries rõ ràng cho nghiệp vụ tiền tệ;
- read model tách khỏi write model ở những màn hình tổng hợp;
- domain được chia theo use case thay vì bám table CRUD thuần túy.

Từ tài liệu này, team có thể đi tiếp sang 3 hướng rất thuận:

- thiết kế entity/table schema chi tiết cho PostgreSQL;
- scaffold command/query/service trong .NET 8;
- chia task implementation theo module `Auth`, `FinancialAccounts`, `Budgeting`, `Transactions`, `Goals`, `Import`, `Admin`.
