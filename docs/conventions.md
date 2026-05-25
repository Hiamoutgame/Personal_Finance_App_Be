# Conventions — Personal Finance App Backend

Tài liệu này là **single source of truth** cho mọi quy ước về API contract, naming, kiểu dữ liệu và xử lý lỗi. Khi `API V2.md`, `flow.md`, schema và code có chênh lệch về convention, **conventions.md được ưu tiên**. Các file khác phải tự đồng bộ lại theo file này.

Phạm vi: tất cả endpoint và DTO của `Personal_Finance_Management.Api` + tài liệu trong `docs/`.

---

## 1. URL & Routing

- **Prefix bắt buộc**: mọi endpoint dưới `/api/v1/...`. Không có endpoint nào nằm ngoài prefix này (kể cả health check sống ở `/health`, nhưng business endpoint thì luôn `/api/v1`).
- **Đặt tên**: dùng **kebab-case**, danh từ số nhiều cho resource:
  - `/api/v1/financial-accounts` (không phải `/FinancialAccount`)
  - `/api/v1/spending-limits`
  - `/api/v1/jars`, `/api/v1/transactions`, `/api/v1/goals`, `/api/v1/reminders`
- **Nested resource**: cho action gắn với một resource cha:
  - `POST /api/v1/jars/{id}/allocate`
  - `POST /api/v1/goals/{id}/contributions`
  - `POST /api/v1/financial-accounts/{id}/sync`
- **Admin**: tất cả endpoint admin nằm dưới `/api/v1/admin/...` và yêu cầu policy `Admin`.
- **Versioning**: chỉ có `v1`. Khi cần breaking change → mở `v2` song song, không sửa contract v1 đã ship.

## 2. JSON Naming

- **Request & response**: **camelCase** cho tất cả field (`firstName`, `transactionDate`, `isOnboardingCompleted`).
- **Không dùng**: PascalCase, snake_case ở mức payload.
- **ID**: `id` (PK của resource), `<entity>Id` cho FK (`jarId`, `categoryId`, `financialAccountId`).
- **Boolean**: prefix `is`/`has`/`can` (`isDeleted`, `hasUnread`, `canEdit`).
- **Currency code**: ISO-4217 (`"VND"`, `"USD"`), uppercase.

## 3. Kiểu dữ liệu chuẩn

| Loại                  | Kiểu trên wire (JSON)               | Ghi chú                                                                 |
| --------------------- | ----------------------------------- | ----------------------------------------------------------------------- |
| Tiền                  | `number` (decimal, tối đa 2 chữ số) | BE dùng `decimal` (.NET) ↔ `NUMERIC(18,2)` (Postgres). KHÔNG dùng float.|
| ID                    | `string` (GUID v4)                  | Lowercase hex với dấu gạch.                                             |
| Thời gian             | `string` ISO-8601 UTC               | Ví dụ `2026-05-25T08:30:00Z`. Server lưu `timestamptz`.                 |
| Ngày (không giờ)      | `string` `YYYY-MM-DD`               | Dùng cho `dueDate`, `startDate`.                                        |
| Enum                  | `string` PascalCase                 | Trùng giá trị với check-constraint DB. Xem mục **Enums**.               |
| Phần trăm             | `number` 0–100                      | Ví dụ `alertAtPercentage = 80`.                                         |

## 4. Sign convention cho `amount`

Đây là điểm dễ gây bug nhất. Quy ước **chốt**:

- **API layer** (request và response) luôn dùng `amount` **dương** (`> 0`). FE không bao giờ gửi số âm và không bao giờ phải xử lý dấu.
- **Server** quyết định dấu dựa trên `type`:
  - `type = "Income"` → ghi DB với `transactions_amount > 0`.
  - `type = "Expense"` → ghi DB với `transactions_amount < 0` (server tự nhân `-1`).
- **DB check-constraint** (`chk_transactions_amount_by_type`) bảo vệ tầng cuối cùng.
- Khi response trả `amount`, server luôn convert về **giá trị tuyệt đối**; FE nhìn vào `type` để biết là thu hay chi.

## 5. Pagination

- Query params: `?page=1&pageSize=20`. Mặc định `page=1`, `pageSize=20`. Tối đa `pageSize=100`.
- Response envelope cho list endpoint:

```json
{
  "items": [ /* ... */ ],
  "totalCount": 123,
  "page": 1,
  "pageSize": 20
}
```

- Không dùng cursor pagination ở v1.

## 6. Filter & Sort

- Filter: query params đặt tên theo field (`?categoryId=...&type=Expense&fromDate=2026-01-01&toDate=2026-01-31`).
- Date range: `fromDate` / `toDate` (inclusive, ISO date hoặc datetime).
- Sort: `?sort=field` hoặc `?sort=-field` (dấu `-` = desc). Chỉ implement khi endpoint thực sự cần.

## 7. HTTP Status Codes

| Trường hợp                                | Status              |
| ----------------------------------------- | ------------------- |
| GET thành công                            | 200 OK              |
| POST tạo mới thành công                   | 201 Created         |
| PATCH/PUT thành công, có body trả về      | 200 OK              |
| DELETE thành công, không body             | 204 No Content      |
| Validation lỗi (field/business rule)      | 422 Unprocessable Entity |
| Body sai cú pháp / thiếu field bắt buộc  | 400 Bad Request     |
| Thiếu/sai token                            | 401 Unauthorized    |
| Token hợp lệ nhưng không đủ quyền         | 403 Forbidden       |
| Resource không tồn tại / không thuộc user | 404 Not Found       |
| Xung đột (vd: trùng key, optimistic lock) | 409 Conflict        |
| Lỗi không lường trước                     | 500 Internal Server Error |

## 8. Error envelope

Mọi response lỗi (4xx, 5xx) đều có shape sau:

```json
{
  "code": "string",         // mã lỗi machine-readable, vd: "VALIDATION_FAILED", "NOT_FOUND"
  "message": "string",      // mô tả ngắn dành cho dev
  "field": "string | null", // nếu lỗi gắn với một field cụ thể
  "details": { }            // optional, object tùy lỗi
}
```

Mã lỗi chuẩn:

- `VALIDATION_FAILED` — 422
- `BAD_REQUEST` — 400
- `UNAUTHORIZED` — 401
- `FORBIDDEN` — 403
- `NOT_FOUND` — 404
- `CONFLICT` — 409
- `INTERNAL_ERROR` — 500

Backend implementation: `AppValidationException`, `NotFoundException`, `UnauthorizedException`, `ConflictException`. Tất cả map qua `GlobalExceptionHandlerMiddleware`.

## 9. Authentication & Authorization

- **JWT Bearer** trong header `Authorization: Bearer <token>`.
- Policy:
  - `User` — endpoint dành cho người dùng cuối.
  - `Admin` — endpoint quản trị (mọi route dưới `/api/v1/admin/...`).
- Một số endpoint là `Public` (`/auth/register`, `/auth/login`); ghi rõ trong API V2.md.
- Mọi query dữ liệu nghiệp vụ **bắt buộc kèm `userId`** lấy từ token. Không bao giờ tin `userId` từ body/query (tránh IDOR).

## 10. Enums (chuẩn DB — không tự đổi)

Các enum dưới đây phải khớp giữa code, docs và DB check-constraint. Khi cần thêm giá trị mới: tạo migration + cập nhật docs + cập nhật mapper, không thiếu bước nào.

| Enum                          | Giá trị hợp lệ                                                                    |
| ----------------------------- | --------------------------------------------------------------------------------- |
| `Account.status`              | `Active`, `Banned`                                                                |
| `BudgetMethod`                | `SixJars`, `Rule503020`, `Custom`, `Undecided`                                    |
| `FinancialAccount.accountType`| `Cash`, `Bank`, `EWallet`, `Other`                                                |
| `FinancialAccount.connectionMode` | `Manual`, `LinkedApi`                                                         |
| `FinancialAccount.syncStatus` | `NeverSynced`, `Synced`, `Syncing`, `Error`, `Disconnected`                       |
| `Jar.status`                  | `Active`, `Paused`, `Archived`                                                    |
| `Transaction.type`            | `Income`, `Expense` *(Transfer KHÔNG có ở v1 — xem mục 11)*                       |
| `Transaction.sourceType`      | `Manual`, `Imported`, `OCR`, `Jar`, `System`                                      |
| `ImportJob.status`            | `Pending`, `Processing`, `AwaitingReview`, `Completed`, `Failed`                  |
| `SpendingLimit.period`        | `Daily`, `Monthly`                                                                |
| `Goal.status`                 | `Active`, `Completed`, `Cancelled`                                                |
| `Reminder.frequency`          | `Daily`, `Weekly`, `Monthly`, `Quarterly`, `Yearly`                               |
| `Reminder.status`             | `Active`, `Paused`, `Completed`, `Cancelled`                                      |
| `Broadcast.status`            | `Queued`, `Sent`, `Failed`, `Cancelled`                                           |
| `Notification.type`           | `SpendingAlert`, `GoalUpdate`, `Reminder`, `System`, `Broadcast`                  |
| `BankConnectionSession.status`| `Pending`, `Authorized`, `Completed`, `Failed`, `Expired`                         |

## 11. Quyết định scope v1

Để dứt điểm các điểm mơ hồ trong docs cũ:

- **Transfer transaction**: **KHÔNG có** ở v1. Schema vẫn giữ field `from_jar_id` / `to_jar_id` để hỗ trợ allocate giữa các jar trong tương lai, nhưng `Transaction.type` chỉ là `Income` hoặc `Expense`. Mọi mô tả "Transfer" trong docs cũ phải bị loại bỏ hoặc chuyển sang backlog.
- **Phân bổ tiền vào jar**: dùng endpoint riêng `POST /api/v1/jars/{id}/allocate` (không phải tạo một transaction `Transfer`). Endpoint này điều chỉnh `jar.balance` mà không tạo bản ghi `transactions`.
- **Đóng góp vào goal**: dùng `POST /api/v1/goals/{id}/contributions`. Khi `sourceJarId` được chỉ định, server đồng thời giảm `jar.balance`.
- **Casso flow**: chốt dùng `/api/v1/financial-accounts/casso/connect` → `/api/v1/financial-accounts/casso/callback` → `/api/v1/financial-accounts/{id}/sync`. Endpoint legacy `/Transactions/Casso` đánh dấu `@deprecated`, sẽ remove ở phase 2.
- **Jar balance**: là **state lưu trên DB**, được cập nhật bởi service khi có allocate/transaction. Client không tự set `balance` qua PATCH.
- **Goal progress**: `progressPercentage = min(100, round(savedAmount / targetAmount * 100, 2))`.

## 12. Background jobs (sẽ implement ở phase 4)

- `ReminderDispatcherJob` — cron mỗi 5 phút, sinh `Notification` cho reminder đến hạn (xét `start_date`, `frequency`, `notify_days_before`).
- `LimitAlertJob` — chạy đồng bộ sau mỗi transaction (best-effort) + recurring mỗi giờ. So sánh tổng chi trong kỳ vs `limit_amount × alert_at_percentage / 100`, tạo `Notification` type `SpendingAlert`.
- `BroadcastFanoutJob` — kích hoạt khi admin tạo broadcast, fan-out tạo `Notification` cho từng user thuộc `target_audience` và cập nhật `delivered_count`.
- `ImportJobProcessor` — pick `import_jobs` ở trạng thái `Pending`, chuyển sang `Processing`, parse, sinh `import_transaction_drafts`, kết thúc bằng `AwaitingReview` hoặc `Failed`.

## 13. Timezone

- Server lưu `timestamptz` (UTC). API trả ISO-8601 UTC (`...Z`).
- FE chịu trách nhiệm hiển thị theo timezone của user (mặc định `Asia/Ho_Chi_Minh`).
- Filter `fromDate` / `toDate` ở dạng `YYYY-MM-DD` được hiểu là **theo timezone của user** (FE phải truyền kèm offset nếu cần; nếu chỉ truyền date, server giả định `Asia/Ho_Chi_Minh`).

## 14. Soft delete

Các bảng có `is_deleted` / `deleted_at` (`transactions`, `categories`...): mọi list/get endpoint **mặc định ẩn** bản ghi đã xóa. Chỉ admin endpoint có cờ `?includeDeleted=true` nếu cần.

## 15. Idempotency

- Endpoint nhập sao kê / import: client có thể truyền header `Idempotency-Key`. Server lưu key + checksum trong vòng 24h để chặn double-submit.
- Casso webhook: dùng `external_transaction_id` làm dedupe key.

---

**Khi bạn sửa convention nào trong file này, bắt buộc:**

1. Cập nhật `API V2.md` để mọi endpoint còn liên quan tuân thủ.
2. Nếu liên quan enum → cập nhật `finjar_schema.sql` (migration mới, không sửa migration cũ).
3. Cập nhật `flow.md` nếu user-facing flow thay đổi.
4. Mở issue rõ ràng nếu code chưa kịp theo — không để docs đi trước code mà không tracking.
