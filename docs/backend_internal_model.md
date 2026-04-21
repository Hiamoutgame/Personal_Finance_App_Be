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
- AI chat session.

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

Các entity lưu DB nên có một base structure thống nhất:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
}
```

Không phải mọi entity đều cần soft delete, nhưng đây là base hữu ích cho:

- transactions
- categories
- notifications
- reminders
- admin categories

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
- `BudgetMethodType`: `SixJars`, `Rule503020`, `Custom`, `Undecided`
- `JarStatus`: `Active`, `Paused`, `Archived`
- `TransactionType`: `Income`, `Expense`, `Transfer`
- `LimitTargetType`: `Jar`, `Category`
- `LimitPeriodType`: `Daily`, `Monthly`
- `NotificationType`: `SpendingAlert`, `GoalUpdate`, `System`, `Broadcast`
- `GoalStatus`: `Active`, `Completed`, `Cancelled`
- `ReminderFrequency`: `Daily`, `Weekly`, `Monthly`, `Quarterly`, `Yearly`
- `ImportJobStatus`: `Pending`, `Processing`, `AwaitingReview`, `Completed`, `Failed`
- `ChatContextType`: `General`, `SpendingAdvice`, `GoalPlanning`

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
- `FirstName`
- `LastName`
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
- 1 user có nhiều jars
- 1 user có nhiều transactions
- 1 user có nhiều goals
- 1 user có nhiều notifications
- 1 user có nhiều reminders
- 1 user có nhiều chat sessions

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
- `FinancialGoalTypes` hoặc bảng con
- `BudgetMethodPreference`
- `AgeRange`
- `SpendingChallenges` hoặc bảng con
- `RecommendedMethod`
- `CompletedAtUtc`

**Ghi chú**

- Với các trường nhiều giá trị như `FinancialGoalTypes`, `SpendingChallenges`, có thể lưu:
  - bảng join riêng; hoặc
  - PostgreSQL array/jsonb nếu team muốn tối ưu tốc độ triển khai.

## 6.3. Category aggregate

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

## 6.4. Budget Setup aggregate

### A. `JarSetupProfile`

**Vai trò**

- lưu cách setup ngân sách hiện tại của user;
- xác định user đang dùng `SixJars`, `Rule503020` hay `Custom`.

**Field chính**

- `Id`
- `UserId`
- `MethodType`
- `InitialBalance`
- `CreatedAtUtc`

### B. `Jar`

**Vai trò**

- đơn vị ngân sách chính;
- giữ balance hiện tại;
- là trung tâm của allocate, transfer, expense, goal contribution.

**Field chính**

- `Id`
- `UserId`
- `JarSetupProfileId`
- `Name`
- `Percentage`
- `Balance`
- `AllocatedAmount`
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

- lưu lịch sử chuyển tiền giữa các hũ;
- không thay thế cho `Transaction`, mà là record nghiệp vụ riêng.

**Field chính**

- `Id`
- `UserId`
- `FromJarId`
- `ToJarId`
- `Amount`
- `Note`
- `CreatedAtUtc`

### D. `JarAllocation`

**Vai trò**

- lưu sự kiện phân bổ thu nhập vào các hũ;
- giúp truy vết nguồn allocation.

**Field chính**

- `Id`
- `UserId`
- `TotalAmount`
- `IncomeTransactionId` nullable
- `Note`
- `CreatedAtUtc`

### E. `JarAllocationItem`

**Field chính**

- `Id`
- `AllocationId`
- `JarId`
- `Amount`
- `BalanceAfterAllocation`

## 6.5. Transaction aggregate

### `Transaction`

**Vai trò**

- lưu các giao dịch thu/chi được người dùng xác nhận;
- là nguồn dữ liệu chính cho dashboard, limits, category breakdown.

**Field chính**

- `Id`
- `UserId`
- `Type`
- `Amount`
- `JarId` nullable với income
- `CategoryId`
- `Note`
- `TransactionDateUtc`
- `SourceType`
- `SourceReferenceId`
- `IsDeleted`
- `DeletedAtUtc`

**SourceType gợi ý**

- `Manual`
- `Imported`
- `OCR`
- `System`

**Rule nội bộ**

- `Expense` bắt buộc có `JarId`
- `Amount > 0`
- xóa transaction là soft delete
- update amount phải tính delta chứ không recalc toàn bộ

### Có cần bảng ledger không?

Có 2 hướng:

**Hướng A: đơn giản cho phase đầu**

- chỉ lưu `Jar.Balance`
- khi transfer/expense/contribution thì cập nhật trực tiếp balance
- lưu lịch sử qua `Transaction`, `JarTransfer`, `GoalContribution`, `JarAllocation`

**Hướng B: chuẩn hóa hơn**

- thêm bảng `BalanceLedgerEntry`
- mọi thay đổi số dư đều qua ledger
- `Jar.Balance` là snapshot/cache

Khuyến nghị cho phase đầu:

- dùng **Hướng A** để đội không bị quá tải;
- nếu sau này hệ thống cần audit tiền tệ sâu hơn thì thêm ledger ở phase sau.

## 6.6. Limits & Alerts aggregate

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
- `MetadataJson`
- `CreatedAtUtc`

**MetadataJson có thể chứa**

- `limitId`
- `goalId`
- `broadcastId`
- `currentPercentage`
- `reminderId`

## 6.7. Goals aggregate

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
- `Note`
- `CreatedAtUtc`

**Rule nội bộ**

- nếu có `SourceJarId` thì phải trừ `Jar.Balance` atomic
- sau contribution phải cập nhật lại `SavedAmount` và `GoalStatus`

## 6.8. Reminder aggregate

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

## 6.9. Import aggregate

### A. `ImportJob`

**Vai trò**

- đại diện cho một phiên import file sao kê.

**Field chính**

- `Id`
- `UserId`
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

- chỉ khi user confirm mới sinh `Transaction` thật
- nếu confirm nhiều row thì insert trong 1 transaction

## 6.10. Admin Operations aggregate

### A. `BroadcastNotification`

**Vai trò**

- quản lý đợt gửi thông báo hệ thống cho nhiều user.

**Field chính**

- `Id`
- `Title`
- `Body`
- `TargetAudience`
- `ScheduledAtUtc`
- `Status`
- `TargetCount`
- `CreatedByAdminId`
- `CreatedAtUtc`

### B. `AuditLog`

**Vai trò**

- theo dõi thao tác nhạy cảm của admin;
- append-only.

**Field chính**

- `Id`
- `AdminId`
- `AdminUsername`
- `ActionType`
- `EntityType`
- `EntityId`
- `Description`
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

## 6.11. Chat aggregate

### A. `ChatSession`

**Vai trò**

- lưu một phiên chat của user với AI.

**Field chính**

- `Id`
- `UserId`
- `ContextType`
- `ContextSnapshotJson`
- `CreatedAtUtc`
- `LastMessageAtUtc`

### B. `ChatMessage`

**Vai trò**

- lưu lịch sử multi-turn chat.

**Field chính**

- `Id`
- `SessionId`
- `Role`
- `Content`
- `TokenCount` nullable
- `CreatedAtUtc`

**Rule nội bộ**

- mỗi AI call lấy tối đa `N` messages gần nhất
- `N` mặc định là `20`, có thể cấu hình

## 7. Quan hệ nghiệp vụ quan trọng

## 7.1. User là root ownership

Hầu hết dữ liệu nghiệp vụ đều phải gắn với `UserId`:

- `Jar`
- `Transaction`
- `Category` custom
- `FinancialGoal`
- `RecurringReminder`
- `Notification`
- `ImportJob`
- `ChatSession`

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

## 9.1. Allocate income vào jars

Trong cùng transaction:

- load tất cả jars của user
- tính amount theo percentage
- update balance từng jar
- tạo `JarAllocation`
- tạo `JarAllocationItem`

Nếu bất kỳ bước nào lỗi thì rollback toàn bộ.

## 9.2. Transfer giữa jars

Trong cùng transaction:

- load `fromJar`, `toJar`
- kiểm tra ownership
- kiểm tra `fromJar.Balance >= amount`
- trừ `fromJar.Balance`
- cộng `toJar.Balance`
- tạo `JarTransfer`

## 9.3. Create expense transaction

Trong cùng transaction:

- validate category
- validate jar
- kiểm tra balance
- tạo `Transaction`
- trừ `Jar.Balance`
- evaluate `SpendingLimit`
- nếu vượt threshold thì tạo `Notification`

## 9.4. Update transaction

Trong cùng transaction:

- load transaction cũ
- tính `delta`
- update transaction
- update `Jar.Balance` theo delta
- re-evaluate limit nếu là expense

## 9.5. Delete transaction

Trong cùng transaction:

- soft delete transaction
- hoàn balance cho jar nếu là expense
- re-evaluate limit nếu cần

## 9.6. Contribute goal

Trong cùng transaction:

- load goal
- nếu có `sourceJarId` thì load jar và check balance
- tạo `GoalContribution`
- tăng `FinancialGoal.SavedAmount`
- nếu có source jar thì trừ balance
- cập nhật `GoalStatus`
- nếu hoàn thành thì có thể tạo notification

## 9.7. Confirm import

Trong cùng transaction:

- load import job
- load selected drafts
- validate lần cuối
- tạo hàng loạt `Transaction`
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
    Chat/

  PersonalFinance.Domain/
    Common/
    Users/
    Categories/
    Budgeting/
    Transactions/
    Limits/
    Goals/
    Notifications/
    Reminders/
    Importing/
    Admin/
    Chat/
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
| Setup jars | `SetupJarsCommand` | `JarSetupProfile`, `Jar` | tạo jars mặc định/custom |
| Allocate | `AllocateIncomeToJarsCommand` | `Jar`, `JarAllocation` | update balances |
| Transfer | `TransferBetweenJarsCommand` | `Jar`, `JarTransfer` | update balances |
| Create transaction | `CreateTransactionCommand` | `Transaction`, `Jar` | evaluate limits, notifications |
| Dashboard | `GetDashboardQuery` | read models | aggregate nhiều nguồn |
| Create limit | `CreateSpendingLimitCommand` | `SpendingLimit` | none |
| Goal contribute | `ContributeToGoalCommand` | `FinancialGoal`, `GoalContribution`, `Jar` | update status, notify |
| Reminder create | `CreateReminderCommand` | `RecurringReminder` | job scheduling |
| Import confirm | `ConfirmImportJobCommand` | `ImportJob`, `ImportedTransactionDraft`, `Transaction` | update balances |
| Ban user | `UpdateUserStatusCommand` | `UserAccount`, `AuditLog` | audit append |
| Broadcast | `CreateBroadcastCommand` | `BroadcastNotification` | background dispatch |
| AI settings | `UpdateAiSettingsCommand` | `AiSetting` | encrypt key, audit |
| Chat message | `SendChatMessageCommand` | `ChatSession`, `ChatMessage` | AI call, persist response |

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
- chia task implementation theo module `Auth`, `Budgeting`, `Transactions`, `Goals`, `Import`, `Admin`, `Chat`.
