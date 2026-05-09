# API v2 - Tổng hợp endpoint hiện có trong controller

Tài liệu này tổng hợp theo code controller hiện tại trong `Personal_Finance_Management.Api\Controllers` và DTO trong `Personal_Finance_Management.Service`.

Quy ước trong tài liệu:

- Field được ghi theo JSON name dự kiến khi serialize từ ASP.NET Core mặc định: PascalCase trong DTO sẽ thành camelCase, field đã lower camelCase giữ nguyên.
- Không dùng dữ liệu mẫu. Schema chỉ ghi kiểu dữ liệu.
- `Bearer` nghĩa là endpoint cần access token. Nếu controller dùng `[Authorize]` mà chưa chỉ rõ policy thì ghi `Bearer`.
- Chỗ chưa rõ hoặc chưa implement được đánh dấu `[TODO]` và để dòng `Cần bổ sung: ____`.

## 1. Auth, User, Onboarding

### Auth

| REST API | Request | Response |
| --- | --- | --- |
| `POST /api/v1/auth/register` | body `{ username: string, email: string, password: string, firstName: string, lastName: string }` | `201 Created` `{ id: guid, username: string, firstName: string, lastName: string, email: string, accessToken: string }` |
| `POST /api/v1/auth/login` | body `{ email: string, password: string }` | `200 OK` `{ id: guid, username: string, firstName: string, lastName: string, email: string, accessToken: string }` |
| `POST /api/v1/auth/logout` | no body | `200 OK` `{ message: string }` |

Ghi chú đã chốt:

- Register chốt response status là `201 Created`; controller đã cập nhật theo quyết định này.
- Login chốt request là `email + password`.
- Không thêm endpoint riêng `POST /api/v1/admin/auth/login`; admin dùng auth login hiện có và phân quyền bằng role/authorize.

### User Profile

| REST API | Request | Response |
| --- | --- | --- |
| `GET /User/me` | Bearer User, no body | `{ id: guid, userName: string, firstName: string, lastName: string, email: string, phone: string?, avatarUrl: string?, preferredCurrency: string, isOnboardingCompleted: boolean }` |
| `PATCH /User/me` | Bearer User, body `{ firstName: string?, lastName: string?, phone: string?, avatarUrl: string? }` | `{ id: guid, fullName: string, phone: string, avatarUrl: string }` |
| `GET /User/me/setup` | Bearer User, no body | `{ isOnboardingCompleted: boolean, monthlyIncome: decimal?, budgetMethod: string, defaultFinancialAccountId: guid?, jarCount: int, financialAccountCount: int, limitCount: int, activeGoalCount: int }` |

### Onboarding

| REST API | Request | Response |
| --- | --- | --- |
| `POST /Onboarding` | Bearer User, body `{ monthlyIncome: int, occupationType: string, financialGoalTypes: string[], budgetMethodPreference: string, ageRange: string, spendingChallenges: string[] }` | `{ recommendedMethod: string, recommendedCategories: { name: string, icon: string }[], recommendedJars: { name: string }[]?, defaultFinancialAccount: { name: string, accountType: string } }` |

## 2. Financial Account, Jar, Category

### Financial Account

| REST API | Request | Response |
| --- | --- | --- |
| `GET /FinancialAccount` | Bearer, no body | `{ data: { id: guid, name: string, accountType: string, connectionMode: string, providerName: string?, maskedAccountNumber: string?, currency: string, currentBalance: decimal, syncStatus: string, isDefault: boolean, isActive: boolean }[] }` |
| `POST /FinancialAccount/Manual` | Bearer, body `{ name: string, accountType: string, currentBalance: decimal, currency: string?, isDefault: boolean }` | `{ id: guid, name: string, accountType: string, connectionMode: string, currentBalance: decimal, currency: string, isDefault: boolean, isActive: boolean }` |
| `POST /FinancialAccount/LinkApi` | Bearer, body `{ bankName: string, bankCode: string?, accountNumber: string, accountHolderName: string?, isDefault: boolean }` | `{ id: guid, name: string, accountType: string, connectionMode: string, providerName: string, maskedAccountNumber: string, currentBalance: decimal, currency: string, syncStatus: string, isDefault: boolean, isActive: boolean }` |
| `PATCH /FinancialAccount/{id}` | Bearer, route `{ id: guid }`, body `{ name: string?, currentBalance: decimal?, isDefault: boolean? }` | `{ id: guid, name: string, currentBalance: decimal, isDefault: boolean, updatedAt: datetimeOffset }` |
| `DELETE /FinancialAccount/{id}` | Bearer, route `{ id: guid }` | `{ message: string }` |

Ghi chú:

- Route và method đang ghi theo controller hiện tại.
- Update response hiện có `updatedAt` theo DTO hiện tại.

### Jar

| REST API | Request | Response |
| --- | --- | --- |
| `GET /Jar` | Bearer, no body | `{ methodType: string, totalJarBalance: decimal, unallocatedBalance: decimal, data: { id: guid, name: string, balance: decimal, color: string, icon: string, status: string }[] }` |
| `POST /Jar` | Bearer, body `{ name: string, color: string, icon: string }` | `{ id: guid, name: string, balance: decimal, status: string }` |
| `PATCH /Jar/{id}` | Bearer, route `{ id: guid }`, body `{ name: string?, color: string?, icon: string? }` | `{ id: guid, name: string, color: string, icon: string, status: string }` |
| `DELETE /Jar/{id}` | Bearer, route `{ id: guid }` | `{ message: string }` |

Ghi chú:

- Route đang ghi theo controller hiện tại.
- Public jar setup/allocate/transfer endpoints không có trong controller hiện tại.

### Category

| REST API | Request | Response |
| --- | --- | --- |
| `GET /api/v1/categories` | Bearer User, no body | `{ defaultCategories: { id: guid, name: string, icon: string?, color: string? }[], customCategories: { id: guid, name: string, icon: string?, color: string? }[] }` |
| `POST /api/v1/categories` | Bearer User, body `{ name: string, icon: string?, color: string? }` | `201 Created` `{ id: guid, name: string, icon: string?, color: string? }` |
| `PATCH /api/v1/categories/{id}` | Bearer User, route `{ id: guid }`, body `{ name: string?, icon: string?, color: string? }` | `{ id: guid, name: string, icon: string?, color: string? }` |
| `DELETE /api/v1/categories/{id}` | Bearer User, route `{ id: guid }` | `{ message: string }` |

## 3. Transactions, Import, Casso

### Transactions

| REST API | Request | Response |
| --- | --- | --- |
| `GET /Transactions` | Bearer, query `{ pageIndex: int, pageSize: int, financialAccountId: guid?, type: string?, jarId: guid?, categoryId: guid?, fromDate: date?, toDate: date?, keyword: string?, sortBy: string?, sortDir: string? }` | `{ data: { id: guid, type: string, transactionsAmount: decimal, note: string?, date: datetimeOffset, financialAccount: { id: guid?, name: string? }, jar: { id: guid?, name: string? }?, category: { id: guid?, name: string? }? }[], pagination: { page: int, pageSize: int, totalCount: int, totalPages: int } }` |
| `POST /Transactions` | Bearer, body `{ financialAccountId: guid?, type: string, transactionsAmount: decimal, categoryId: guid?, fromJarId: guid?, toJarId: guid?, note: string?, date: datetimeOffset }` | `{ id: guid, financialAccountId: guid?, type: string, transactionsAmount: decimal, date: datetimeOffset }` |
| `PATCH /Transactions/{id}` | Bearer, route `{ id: guid }`, body `{ transactionsAmount: decimal?, categoryId: guid?, note: string? }` | `{ id: guid, type: string, transactionsAmount: decimal, date: datetimeOffset }` |
| `DELETE /Transactions/{id}` | Bearer, route `{ id: guid }` | `{ message: string }` |

Ghi chú:

- Route đang ghi theo controller hiện tại.
- FE gửi `transactionsAmount` là số dương cho cả `Income` và `Expense`.
- Request create hiện có `fromJarId` và `toJarId` theo DTO hiện tại.

### Casso Transaction Integration

| REST API | Request | Response |
| --- | --- | --- |
| `GET /Transactions/Casso` | Bearer, query `{ financialAccountId: guid, fromDate: date?, toDate: date?, page: int, pageSize: int, sort: string? }` | `{ receivedCount: int, createdCount: int, skippedCount: int, message: string }` |
| `POST /Transactions/Casso` | Anonymous, headers `{ secure-token: string?, X-Casso-Signature: string? }`, body `{ error: int, data: json }` | `{ receivedCount: int, createdCount: int, skippedCount: int, message: string }` |

Ghi chú:

- Đây là endpoint integration Casso, route đang ghi theo controller hiện tại.

### Import/OCR

| REST API | Request | Response |
| --- | --- | --- |
| `POST /api/v1/imports/image` | Bearer User, `multipart/form-data` `{ file: file, layout: string?, runOcr: boolean }` | `{ message: string, fileName: string, originalFileName: string, storedFilePath: string, contentType: string?, sizeInBytes: long, ocrJsonFileName: string?, storedOcrJsonPath: string?, rawOcrJson: string?, ocrResult: { isSuccess: boolean, text: string?, layout: string?, engine: string, rawJson: string?, statusCode: int?, errorMessage: string? }? }` |

TODO:

- `[TODO]` Controller hiện chỉ có `POST /api/v1/imports/image`, chưa có full flow `POST /api/v1/imports`, `GET /api/v1/imports/{id}`, `GET /api/v1/imports/{id}/preview`, `POST /api/v1/imports/{id}/confirm`. Cần bổ sung: ____

## 4. Dashboard, AI

### Personal Dashboard

| REST API | Request | Response |
| --- | --- | --- |
| `GET /user/dashboard` | Bearer, no body | `{ balanceSummary: { totalBalance: decimal, allocatedBalance: decimal, unallocatedBalance: decimal, totalIncome: decimal, totalExpense: decimal, netChange: decimal }, financialAccounts: { id: guid, name: string, currentBalance: decimal, isDefault: boolean }[], jarSummary: { jarId: guid, jarName: string, balance: decimal, spent: decimal, spentPercentage: decimal }[], categoryBreakdown: { categoryId: guid, categoryName: string, totalAmount: decimal, percentage: decimal }[], recentTransactions: { id: guid, type: string, transactionsAmount: decimal, note: string?, date: datetimeOffset }[], goalProgress: { goalId: guid, title: string, progressPercentage: decimal, daysRemaining: decimal }[] }` |

### AI Chat

| REST API | Request | Response |
| --- | --- | --- |
| `POST /api/v1/ai/chat` | Bearer User, body `{ message: string, recentMessages: { sender: string, content: string }[]? }` | `{ answer: string, suggestions: string[]?, source: string }` |

## 5. Limits, Goals, Reminders, Notifications

### Limits

| REST API | Request | Response |
| --- | --- | --- |
| `GET /api/v1/limits` | Bearer, no body | `{ data: { id: guid, targetType: "Jar" \| "Category", targetId: guid, targetName: string, limitAmount: decimal, period: string, alertAtPercentage: decimal, currentSpent: decimal, currentPercentage: double, status: string }[] }` |
| `POST /api/v1/limits` | Bearer, body `{ targetType: "Jar" \| "Category", targetId: guid, limitAmount: decimal, period: string, alertAtPercentage: decimal }` | `201 Created` `{ id: guid, targetType: "Jar" \| "Category", targetId: guid, limitAmount: decimal, period: string, alertAtPercentage: decimal }` |
| `PATCH /api/v1/limits/{id}` | Bearer, route `{ id: guid }`, body `{ limitAmount: decimal?, alertAtPercentage: decimal? }` | `{ id: guid, limitAmount: decimal, alertAtPercentage: decimal }` |
| `DELETE /api/v1/limits/{id}` | Bearer, route `{ id: guid }` | `{ message: string }` |

### Goals

| REST API | Request | Response |
| --- | --- | --- |
| `GET /api/v1/goals` | Bearer, no body | `{ data: { id: guid, title: string, targetAmount: decimal, savedAmount: decimal, progressPercentage: double, dueDate: datetime, status: string, suggestedMonthlyContribution: decimal }[] }` |
| `GET /api/v1/goals/{id}` | Bearer, route `{ id: guid }` | `{ id: guid, title: string, targetAmount: decimal, savedAmount: decimal, progressPercentage: double, dueDate: datetime, daysRemaining: int, status: string, suggestedMonthlyContribution: decimal, linkedJarId: guid? }` |
| `POST /api/v1/goals` | Bearer, body `{ title: string, targetAmount: decimal, dueDate: datetime, linkedJarId: guid?, note: string? }` | `201 Created` `{ id: guid, title: string, targetAmount: decimal, savedAmount: decimal, progressPercentage: double, status: string, dueDate: datetime }` |
| `PATCH /api/v1/goals/{id}` | Bearer, route `{ id: guid }`, body `{ title: string?, targetAmount: decimal?, dueDate: datetime?, linkedJarId: guid?, note: string? }` | `{ id: guid, title: string, targetAmount: decimal, dueDate: datetime, status: string }` |
| `DELETE /api/v1/goals/{id}` | Bearer, route `{ id: guid }` | `{ message: string }` |

Ghi chú đã chốt:

- Không dùng endpoint `POST /api/v1/goals/{id}/contributions`; flow đóng góp mục tiêu đã gộp qua transaction.

### Reminders

| REST API | Request | Response |
| --- | --- | --- |
| `GET /api/v1/reminders` | Bearer, no body | `{ data: { id: guid, title: string, amount: decimal, frequency: string, nextDueDate: datetimeOffset, status: string }[] }` |
| `POST /api/v1/reminders` | Bearer, body `{ title: string, amount: decimal, frequency: string, dayOfMonth: short?, startDate: datetimeOffset, categoryId: guid?, notifyDaysBefore: short?, note: string? }` | `{ id: guid, title: string, amount: decimal, frequency: string, nextDueDate: datetimeOffset, status: string }` |
| `PATCH /api/v1/reminders/{id}` | Bearer, route `{ id: guid }`, body `{ title: string?, amount: decimal?, frequency: string?, dayOfMonth: int?, status: "Active" \| "Paused" \| "Completed" \| "Cancelled"?, notifyDaysBefore: int?, note: string? }` | `{ id: guid, title: string, frequency: string, nextDueDate: datetimeOffset, status: string }` |
| `DELETE /api/v1/reminders/{id}` | Bearer, route `{ id: guid }` | `{ message: string }` |

Ghi chú đã chốt:

- Reminder status hợp lệ là `Active`, `Paused`, `Completed`, `Cancelled`.
- `DELETE /api/v1/reminders/{id}` chuyển status sang `Cancelled`.

### Notifications

| REST API | Request | Response |
| --- | --- | --- |
| `GET /api/v1/notifications` | Bearer, query `{ type: string?, status: string?, pageSize: int, pageIndex: int }` | `{ items: { id: guid, type: string, title: string, body: string, isRead: boolean, occurredAt: datetimeOffset }[], totalItems: int, pageSize: int, pageIndex: int, unreadCount: int }` |
| `PATCH /api/v1/notifications/status` | Bearer, body `{ ids: guid[]?, isRead: boolean, markAll: boolean }` | `{ updatedCount: int, unreadCount: int }` |

## 6. Admin APIs

### Admin User Management

| REST API | Request | Response |
| --- | --- | --- |
| `GET /api/v1/admin/users` | Bearer Admin, query `{ pageIndex: int, pageSize: int, status: "Active" \| "Banned"?, keyword: string? }` | `{ data: { id: guid, userName: string, firstName: string, lastName: string, email: string, phone: string?, avatarUrl: string?, preferredCurrency: string, isOnboardingCompleted: boolean, status: string, statusReason: string?, createdAt: datetimeOffset, lastLoginAt: datetimeOffset? }[], pagination: { page: int, pageSize: int, totalCount: int, totalPages: int } }` |
| `GET /api/v1/admin/users/{id}` | Bearer Admin, route `{ id: guid }` | `{ id: guid, userName: string, firstName: string, lastName: string, email: string, phone: string?, avatarUrl: string?, preferredCurrency: string, isOnboardingCompleted: boolean, status: string, statusReason: string?, createdAt: datetimeOffset, lastLoginAt: datetimeOffset? }` |
| `PATCH /api/v1/admin/users/{id}/status` | Bearer Admin, route `{ id: guid }`, body `{ status: "Active" \| "Banned", statusReason: string? }` | `{ id: guid, userName: string, firstName: string, lastName: string, email: string, phone: string?, avatarUrl: string?, preferredCurrency: string, isOnboardingCompleted: boolean, status: string, statusReason: string?, createdAt: datetimeOffset, lastLoginAt: datetimeOffset? }` |
| `PATCH /api/v1/change-role/{accountId}` | Bearer Admin, route `{ accountId: guid }`, query/body binding `{ role: AccountRole }` | `string` |

Ghi chú:

- `GET /api/v1/admin/users` chỉ trả account role `User`.
- Change role route đang ghi đúng theo controller hiện tại là `/api/v1/change-role/{accountId}`; hiện giữ theo controller.
- `PATCH /api/v1/admin/users/{id}/status` không toggle ngầm; admin gửi status đích rõ ràng.

### Admin Categories

| REST API | Request | Response |
| --- | --- | --- |
| `GET /api/v1/admin/categories` | Bearer Admin, query `{ isActive: boolean? }` | `{ data: { id: guid, name: string, icon: string?, color: string?, order: int, isActive: boolean }[] }` |
| `POST /api/v1/admin/categories` | Bearer Admin, body `{ name: string, icon: string?, color: string?, order: int }` | `201 Created` `{ id: guid, name: string, icon: string?, color: string?, order: int, isActive: boolean }` |
| `PATCH /api/v1/admin/categories/{id}` | Bearer Admin, route `{ id: guid }`, body `{ name: string?, icon: string?, color: string?, order: int?, isActive: boolean? }` | `{ id: guid, name: string, icon: string?, color: string?, order: int, isActive: boolean }` |
| `DELETE /api/v1/admin/categories/{id}` | Bearer Admin, route `{ id: guid }` | `{ message: string }` |

### Admin Broadcasts

| REST API | Request | Response |
| --- | --- | --- |
| `POST /api/v1/admin/broadcasts` | Bearer Admin, body `{ title: string, body: string, targetAudience: string, scheduledAt: datetimeOffset? }` | `{ id: guid, title: string, body: string, targetAudience: string, status: string, scheduledAt: datetimeOffset?, sentAt: datetimeOffset?, targetCount: int, deliveredCount: int }` |
| `GET /api/v1/admin/broadcasts` | Bearer Admin, query `{ pageIndex: int, pageSize: int, status: string }` | `{ items: { id: guid, title: string, body: string, targetAudience: string, status: string, scheduledAt: datetimeOffset?, sentAt: datetimeOffset?, targetCount: int, deliveredCount: int }[], pagination: { pageIndex: int, pageSize: int, totalCount: int, totalPages: int } }` |

### Admin Dashboard, Audit Log, AI Settings

| REST API | Request | Response |
| --- | --- | --- |
| `GET /api/v1/admin/dashboard` | Bearer Admin, no body | `{ summary: { totalUsers: int, newUsersThisMonth: int, activeUsersLast30Days: int, bannedUsers: int, totalTransactions: int, transactionsThisMonth: int, totalJars: int, activeGoals: int, pendingImportJobs: int }, recentUsers: { id: guid, username: string, firstName: string, lastName: string, email: string, status: string, isOnboardingCompleted: boolean, lastLoginAt: datetimeOffset? }[], recentTransactions: { id: guid, type: string, transactionsAmount: decimal, note: string?, transactionDate: datetimeOffset, user: { id: guid, username: string, firstName: string, lastName: string }, financialAccount: { id: guid, name: string, accountType: string }?, category: { id: guid, name: string }? }[] }` |
| `GET /api/v1/admin/audit-logs` | Bearer Admin, query `{ adminId: guid?, actionType: string?, entityType: string?, fromDate: datetimeOffset?, toDate: datetimeOffset?, page: int, pageSize: int }` | `{ items: { id: guid, adminUsername: string, actionType: string, entityType: string, description: string, createdAt: datetimeOffset }[], pagination: { pageIndex: int, pageSize: int, totalCount: int, totalPages: int } }` |
| `GET /api/v1/admin/ai-settings` | Bearer Admin, no body | `{ modelName: string, systemPrompt: string, temperature: decimal, maxTokens: int, isEnabled: boolean, apiKeyMasked: string? }` |
| `PATCH /api/v1/admin/ai-settings` | Bearer Admin, body `{ modelName: string?, systemPrompt: string?, temperature: decimal?, maxTokens: int?, isEnabled: boolean? }` | `{ modelName: string, isEnabled: boolean }` |

Security note:

- Admin AI settings response chỉ có `apiKeyMasked`, không có raw API key trong DTO response hiện tại.

## 7. Health, Legacy/Test Endpoints

### Health

| REST API | Request | Response |
| --- | --- | --- |
| `GET /health` | no body | `{ status: string }` |
| `GET /health/db/local` | no body | success `{ status: string, target: string, database: string, environment: string }`; failure `{ status: string, target: string, database: string, environment: string, error: string? }` |
| `GET /health/db/render` | no body | success `{ status: string, target: string, database: string, environment: string }`; failure `{ status: string, target: string, database: string, environment: string, error: string? }` |

## 8. Lưu ý cuối - TODO để fill

Các mục dưới đây là chỗ đang thiếu hoặc cần sửa code để khớp quyết định đã chốt.

1. Auth:
    - Register đã chốt `201 Created`; controller đã cập nhật từ `Ok(...)` sang `201 Created`.
    - Login đã chốt `email + password`.
    - Không thêm admin auth login riêng.
2. Import:
    - Controller chỉ có OCR image upload.
    - Full statement import/status/preview/confirm chưa có.
    - Scope cần fill: ____
3. Secrets/config:
    - `appsettings*.json` trong repo đang có secret thật-looking. Không đưa vào API docs. Nên rotate và chuyển sang env/secret manager.