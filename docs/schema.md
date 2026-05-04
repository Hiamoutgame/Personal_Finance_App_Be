# FinJar Database Schema

Tai lieu nay mo ta schema PostgreSQL hien tai trong `docs/finjar_schema.sql`.
Scope hien tai uu tien MVP 1 thang: quan ly tai khoan, onboarding, nguon tien, hu ngan sach, giao dich, import sao ke, muc tieu, han muc, nhac nho, thong bao va cau hinh AI.

## Quy uoc chung

- PostgreSQL.
- Tat ca bang nghiep vu dung `Guid` primary key, bao gom `roles.id`.
- Thoi gian dung `timestamptz`.
- Tien dung `numeric(18,2)`.
- Metadata/payload cau truc dung `json`.
- Enum luu bang `varchar`, backend phai validate cung gia tri trong schema.
- Soft delete chi ap dung cho bang co field tuong ung, hien tai la `transactions` va `categories`.

## Migration 1 - Identity va Audit

### `roles`

Muc dich: luu vai tro he thong.

| Field         | Type          | Ghi chu          |
| ------------- | ------------- | ---------------- |
| `id`          | `Guid`        | PK               |
| `code`        | `varchar(30)` | unique, not null |
| `name`        | `varchar(50)` | not null         |
| `description` | `text`        | nullable         |
| `created_at`  | `timestamptz` | default `now()`  |

Seed mac dinh:

- `00000000-0000-0000-0000-000000000001 / User`
- `00000000-0000-0000-0000-000000000002 / Admin`

### `accounts`

Muc dich: tai khoan dang nhap cua user/admin.

| Field                     | Type           | Ghi chu                                  |
| ------------------------- | -------------- | ---------------------------------------- |
| `id`                      | `Guid`         | PK                                       |
| `role_id`                 | `Guid`         | FK `roles.id`                            |
| `username`                | `varchar(50)`  | unique, not null                         |
| `email`                   | `varchar(255)` | unique, not null                         |
| `password_hash`           | `text`         | mat khau da hash, khong luu plain text   |
| `first_name`              | `varchar(150)` | not null                                 |
| `last_name`               | `varchar(150)` | not null                                 |
| `phone`                   | `varchar(20)`  | nullable                                 |
| `avatar_url`              | `text`         | nullable                                 |
| `status`                  | `varchar(20)`  | `Active` hoac `Banned`, default `Active` |
| `status_reason`           | `text`         | nullable                                 |
| `preferred_currency`      | `char(3)`      | default `VND`                            |
| `is_onboarding_completed` | `boolean`      | default `false`                          |
| `last_login_at`           | `timestamptz`  | nullable                                 |
| `created_at`              | `timestamptz`  | default `now()`                          |
| `updated_at`              | `timestamptz`  | default `now()`                          |

### `audit_logs`

Muc dich: ghi lai hanh dong quan trong, dac biet la thao tac admin.

| Field              | Type          | Ghi chu          |
| ------------------ | ------------- | ---------------- |
| `id`               | `Guid`        | PK               |
| `actor_account_id` | `Guid`        | FK `accounts.id` |
| `action_type`      | `varchar(50)` | not null         |
| `entity_type`      | `varchar(50)` | not null         |
| `entity_id`        | `Guid`        | nullable         |
| `description`      | `text`        | not null         |
| `metadata_json`    | `json`        | nullable         |
| `ip_address`       | `varchar(45)` | IPv4/IPv6 text   |
| `created_at`       | `timestamptz` | default `now()`  |

## Migration 2 - Onboarding va Financial Setup

### `onboarding_profiles`

Muc dich: luu ket qua onboarding cua user.

| Field                      | Type            | Ghi chu                                        |
| -------------------------- | --------------- | ---------------------------------------------- |
| `id`                       | `Guid`          | PK                                             |
| `user_id`                  | `Guid`          | unique FK `accounts.id`                        |
| `monthly_income`           | `numeric(18,2)` | nullable, `>= 0` neu co                        |
| `occupation_type`          | `varchar(50)`   | nullable                                       |
| `financial_goal_types`     | `text`          | nullable                                       |
| `budget_method_preference` | `varchar(30)`   | `SixJars`, `Rule503020`, `Custom`, `Undecided` |
| `age_range`                | `varchar(30)`   | nullable                                       |
| `spending_challenges`      | `text`          | nullable                                       |
| `recommended_method`       | `varchar(30)`   | nullable, cung nhom value voi method           |
| `completed_at`             | `timestamptz`   | not null                                       |
| `created_at`               | `timestamptz`   | default `now()`                                |
| `updated_at`               | `timestamptz`   | default `now()`                                |

### `jar_setups`

Muc dich: luu method budgeting hien tai cua user.

| Field         | Type          | Ghi chu                                        |
| ------------- | ------------- | ---------------------------------------------- |
| `id`          | `Guid`        | PK                                             |
| `user_id`     | `Guid`        | unique FK `accounts.id`                        |
| `method_type` | `varchar(30)` | `SixJars`, `Rule503020`, `Custom`, `Undecided` |
| `created_at`  | `timestamptz` | default `now()`                                |
| `updated_at`  | `timestamptz` | default `now()`                                |

### `financial_accounts`

Muc dich: nguon tien user theo doi nhu tien mat, ngan hang, vi dien tu.

| Field                     | Type            | Ghi chu                                                     |
| ------------------------- | --------------- | ----------------------------------------------------------- |
| `id`                      | `Guid`          | PK                                                          |
| `user_id`                 | `Guid`          | FK `accounts.id`                                            |
| `name`                    | `varchar(100)`  | not null                                                    |
| `account_type`            | `varchar(20)`   | `Cash`, `Bank`, `EWallet`, `Other`                          |
| `connection_mode`         | `varchar(20)`   | `Manual`, `LinkedApi`                                       |
| `provider_code`           | `varchar(50)`   | nullable                                                    |
| `provider_name`           | `varchar(100)`  | nullable                                                    |
| `external_account_id`     | `varchar(150)`  | nullable                                                    |
| `external_account_ref`    | `varchar(150)`  | nullable                                                    |
| `masked_account_number`   | `varchar(50)`   | nullable                                                    |
| `account_holder_name`     | `varchar(150)`  | nullable                                                    |
| `currency`                | `char(3)`       | default `VND`                                               |
| `current_balance`         | `numeric(18,2)` | default `0`                                                 |
| `sync_status`             | `varchar(20)`   | `NeverSynced`, `Synced`, `Syncing`, `Error`, `Disconnected` |
| `last_synced_at`          | `timestamptz`   | nullable                                                    |
| `last_sync_error`         | `text`          | nullable                                                    |
| `access_token_ref`        | `text`          | nullable                                                    |
| `token_expires_at`        | `timestamptz`   | nullable                                                    |
| `consent_expires_at`      | `timestamptz`   | nullable                                                    |
| `last_sync_cursor`        | `text`          | nullable                                                    |
| `webhook_subscription_id` | `varchar(150)`  | nullable                                                    |
| `is_default`              | `boolean`       | default `false`                                             |
| `is_active`               | `boolean`       | default `true`                                              |
| `created_at`              | `timestamptz`   | default `now()`                                             |
| `updated_at`              | `timestamptz`   | default `now()`                                             |

Ghi chu: schema MVP khong con `available_balance` va `balance_as_of`.

### `categories`

Muc dich: danh muc default va custom cho giao dich.

| Field           | Type           | Ghi chu                   |
| --------------- | -------------- | ------------------------- |
| `id`            | `Guid`         | PK                        |
| `name`          | `varchar(100)` | not null                  |
| `icon`          | `varchar(50)`  | nullable                  |
| `color`         | `varchar(20)`  | nullable                  |
| `is_default`    | `boolean`      | default `false`           |
| `owner_user_id` | `Guid`         | nullable FK `accounts.id` |
| `display_order` | `int`          | default `0`               |
| `is_active`     | `boolean`      | default `true`            |
| `deleted_at`    | `timestamptz`  | nullable                  |
| `created_at`    | `timestamptz`  | default `now()`           |
| `updated_at`    | `timestamptz`  | default `now()`           |

### `jars`

Muc dich: hu ngan sach noi bo cua user.

| Field          | Type            | Ghi chu                        |
| -------------- | --------------- | ------------------------------ |
| `id`           | `Guid`          | PK                             |
| `user_id`      | `Guid`          | FK `accounts.id`               |
| `jar_setup_id` | `Guid`          | nullable FK `jar_setups.id`    |
| `name`         | `varchar(100)`  | not null                       |
| `balance`      | `numeric(18,2)` | default `0`                    |
| `currency`     | `char(3)`       | default `VND`                  |
| `color`        | `varchar(20)`   | nullable                       |
| `icon`         | `varchar(50)`   | nullable                       |
| `is_default`   | `boolean`       | default `false`                |
| `status`       | `varchar(20)`   | `Active`, `Paused`, `Archived` |
| `created_at`   | `timestamptz`   | default `now()`                |
| `updated_at`   | `timestamptz`   | default `now()`                |

Ghi chu: schema MVP khong con field `percentage`. Ty le 6 jars/50-30-20 la rule ung dung, khong persist trong bang `jars`.

## Migration 3 - Import va Transactions

### `import_jobs`

Muc dich: dai dien mot lan user import sao ke.

| Field                   | Type           | Ghi chu                                                          |
| ----------------------- | -------------- | ---------------------------------------------------------------- |
| `id`                    | `Guid`         | PK                                                               |
| `user_id`               | `Guid`         | FK `accounts.id`                                                 |
| `financial_account_id`  | `Guid`         | FK `financial_accounts.id`                                       |
| `file_name`             | `varchar(255)` | not null                                                         |
| `original_content_type` | `varchar(100)` | nullable                                                         |
| `stored_file_path`      | `text`         | not null                                                         |
| `bank_code`             | `varchar(50)`  | nullable                                                         |
| `status`                | `varchar(30)`  | `Pending`, `Processing`, `AwaitingReview`, `Completed`, `Failed` |
| `progress`              | `int`          | `0..100`                                                         |
| `estimated_rows`        | `int`          | nullable                                                         |
| `parsed_count`          | `int`          | default `0`                                                      |
| `failed_count`          | `int`          | default `0`                                                      |
| `error_message`         | `text`         | nullable                                                         |
| `uploaded_at`           | `timestamptz`  | default `now()`                                                  |
| `updated_at`            | `timestamptz`  | default `now()`                                                  |

### `transactions`

Muc dich: luu giao dich thu/chi va cac thao tac lien quan hu trong scope MVP.

| Field                          | Type            | Ghi chu                                      |
| ------------------------------ | --------------- | -------------------------------------------- |
| `id`                           | `Guid`          | PK                                           |
| `user_id`                      | `Guid`          | FK `accounts.id`                             |
| `financial_account_id`         | `Guid`          | FK `financial_accounts.id`                   |
| `category_id`                  | `Guid`          | nullable FK `categories.id`                  |
| `import_job_id`                | `Guid`          | nullable FK `import_jobs.id`                 |
| `external_transaction_id`      | `varchar(150)`  | nullable                                     |
| `type`                         | `varchar(20)`   | `Income`, `Expense`                          |
| `transactions_amount`          | `numeric(18,2)` | duong voi Income, am voi Expense             |
| `note`                         | `text`          | nullable                                     |
| `raw_description`              | `text`          | nullable                                     |
| `transaction_date`             | `timestamptz`   | thoi diem giao dich thuc te                  |
| `source_type`                  | `varchar(20)`   | `Manual`, `Imported`, `OCR`, `Jar`, `System` |
| `posted_at`                    | `timestamptz`   | nullable                                     |
| `jar_balance_after_allocation` | `numeric(18,2)` | nullable                                     |
| `from_jar_id`                  | `Guid`          | nullable FK `jars.id`                        |
| `to_jar_id`                    | `Guid`          | nullable FK `jars.id`                        |
| `raw_payload_json`             | `json`          | nullable                                     |
| `is_deleted`                   | `boolean`       | default `false`                              |
| `deleted_at`                   | `timestamptz`   | nullable                                     |
| `created_at`                   | `timestamptz`   | default `now()`                              |
| `updated_at`                   | `timestamptz`   | default `now()`                              |

Quy uoc nghiep vu:

- `Income` luu `transactions_amount > 0`.
- `Expense` luu `transactions_amount < 0`.
- `source_type = 'Jar'` dung cho thao tac noi bo voi hu.
- Chuyen giua hu dung `from_jar_id` va `to_jar_id`.
- Phan bo vao hu co the dung `to_jar_id` va `jar_balance_after_allocation`.
- Schema MVP khong con bang rieng `jar_allocations`, `jar_allocation_items`, `jar_transfers`.

### `import_transaction_drafts`

Muc dich: dong giao dich nhap sau khi parse/import, cho user review truoc khi confirm.

| Field                     | Type            | Ghi chu                           |
| ------------------------- | --------------- | --------------------------------- |
| `id`                      | `Guid`          | PK                                |
| `import_job_id`           | `Guid`          | FK `import_jobs.id`               |
| `row_index`               | `int`           | unique theo `import_job_id`       |
| `transaction_date`        | `timestamptz`   | nullable                          |
| `amount`                  | `numeric(18,2)` | nullable                          |
| `type`                    | `varchar(20)`   | nullable, `Income` hoac `Expense` |
| `raw_description`         | `text`          | nullable                          |
| `edited_note`             | `text`          | nullable                          |
| `edited_category_id`      | `Guid`          | nullable FK `categories.id`       |
| `edited_jar_id`           | `Guid`          | nullable FK `jars.id`             |
| `is_valid`                | `boolean`       | default `true`                    |
| `validation_error`        | `text`          | nullable                          |
| `normalized_payload_json` | `json`          | nullable                          |
| `created_at`              | `timestamptz`   | default `now()`                   |
| `updated_at`              | `timestamptz`   | default `now()`                   |

## Migration 4 - Limits, Goals, Reminders, Notifications

### `spending_limits`

| Field                 | Type            | Ghi chu                     |
| --------------------- | --------------- | --------------------------- |
| `id`                  | `Guid`          | PK                          |
| `user_id`             | `Guid`          | FK `accounts.id`            |
| `jar_id`              | `Guid`          | nullable FK `jars.id`       |
| `category_id`         | `Guid`          | nullable FK `categories.id` |
| `limit_amount`        | `numeric(18,2)` | `> 0`                       |
| `period`              | `varchar(20)`   | `Daily`, `Monthly`          |
| `alert_at_percentage` | `numeric(5,2)`  | `> 0` va `<= 100`           |
| `is_active`           | `boolean`       | default `true`              |
| `created_at`          | `timestamptz`   | default `now()`             |
| `updated_at`          | `timestamptz`   | default `now()`             |

Schema yeu cau it nhat mot trong hai field `jar_id` hoac `category_id` co gia tri.

### `goals`

| Field           | Type            | Ghi chu                            |
| --------------- | --------------- | ---------------------------------- |
| `id`            | `Guid`          | PK                                 |
| `user_id`       | `Guid`          | FK `accounts.id`                   |
| `title`         | `varchar(150)`  | not null                           |
| `target_amount` | `numeric(18,2)` | `> 0`                              |
| `saved_amount`  | `numeric(18,2)` | default `0`                        |
| `due_date`      | `date`          | not null                           |
| `status`        | `varchar(20)`   | `Active`, `Completed`, `Cancelled` |
| `linked_jar_id` | `Guid`          | nullable FK `jars.id`              |
| `note`          | `text`          | nullable                           |
| `created_at`    | `timestamptz`   | default `now()`                    |
| `updated_at`    | `timestamptz`   | default `now()`                    |

### `goal_contributions`

| Field           | Type            | Ghi chu               |
| --------------- | --------------- | --------------------- |
| `id`            | `Guid`          | PK                    |
| `goal_id`       | `Guid`          | FK `goals.id`         |
| `user_id`       | `Guid`          | FK `accounts.id`      |
| `source_jar_id` | `Guid`          | nullable FK `jars.id` |
| `amount`        | `numeric(18,2)` | `> 0`                 |
| `note`          | `text`          | nullable              |
| `created_at`    | `timestamptz`   | default `now()`       |

Ghi chu: schema MVP khong con `source_financial_account_id` trong `goal_contributions`. Neu dong gop den tu nguon tien that, nen tao `transactions` tuong ung va lien ket nghiep vu o service layer.

### `reminders`

| Field                | Type            | Ghi chu                                                       |
| -------------------- | --------------- | ------------------------------------------------------------- |
| `id`                 | `Guid`          | PK                                                            |
| `user_id`            | `Guid`          | FK `accounts.id`                                              |
| `title`              | `varchar(150)`  | not null                                                      |
| `amount`             | `numeric(18,2)` | nullable                                                      |
| `frequency`          | `varchar(20)`   | nullable, `Daily`, `Weekly`, `Monthly`, `Quarterly`, `Yearly` |
| `day_of_month`       | `smallint`      | nullable, `1..31`                                             |
| `start_date`         | `date`          | default `current_date`                                        |
| `category_id`        | `Guid`          | nullable FK `categories.id`                                   |
| `note`               | `text`          | nullable                                                      |
| `status`             | `varchar(20)`   | `Active`, `Paused`, `Completed`, `Cancelled`                  |
| `notify_days_before` | `smallint`      | nullable, default `1`                                         |
| `created_at`         | `timestamptz`   | default `now()`                                               |
| `updated_at`         | `timestamptz`   | default `now()`                                               |

Ghi chu: schema MVP khong luu `next_due_date`; backend tinh lan nhac tiep theo tu `start_date`, `frequency`, `day_of_month` va `notify_days_before`.

### `broadcasts`

| Field                 | Type           | Ghi chu                                 |
| --------------------- | -------------- | --------------------------------------- |
| `id`                  | `Guid`         | PK                                      |
| `created_by_admin_id` | `Guid`         | FK `accounts.id`                        |
| `title`               | `varchar(200)` | not null                                |
| `body`                | `text`         | not null                                |
| `target_audience`     | `varchar(50)`  | default `All`                           |
| `status`              | `varchar(20)`  | `Queued`, `Sent`, `Failed`, `Cancelled` |
| `scheduled_at`        | `timestamptz`  | nullable                                |
| `sent_at`             | `timestamptz`  | nullable                                |
| `target_count`        | `int`          | default `0`                             |
| `delivered_count`     | `int`          | default `0`                             |
| `created_at`          | `timestamptz`  | default `now()`                         |
| `updated_at`          | `timestamptz`  | default `now()`                         |

### `notifications`

| Field           | Type           | Ghi chu                                                          |
| --------------- | -------------- | ---------------------------------------------------------------- |
| `id`            | `Guid`         | PK                                                               |
| `user_id`       | `Guid`         | FK `accounts.id`                                                 |
| `type`          | `varchar(30)`  | `SpendingAlert`, `GoalUpdate`, `Reminder`, `System`, `Broadcast` |
| `title`         | `varchar(200)` | not null                                                         |
| `body`          | `text`         | not null                                                         |
| `is_read`       | `boolean`      | default `false`                                                  |
| `read_at`       | `timestamptz`  | nullable                                                         |
| `broadcast_id`  | `Guid`         | nullable FK `broadcasts.id`                                      |
| `metadata_json` | `json`         | nullable                                                         |
| `created_at`    | `timestamptz`  | default `now()`                                                  |

## Migration 5 - Import Draft Review

`import_transaction_drafts` nam o migration nay trong schema SQL de tach phan review import khoi transaction core.

## Migration 6 - AI Settings

### `ai_settings`

| Field                 | Type           | Ghi chu                   |
| --------------------- | -------------- | ------------------------- |
| `id`                  | `Guid`         | PK                        |
| `updated_by_admin_id` | `Guid`         | nullable FK `accounts.id` |
| `model_name`          | `varchar(100)` | not null                  |
| `system_prompt`       | `text`         | not null                  |
| `temperature`         | `numeric(3,2)` | `0..2`, default `0.7`     |
| `max_tokens`          | `int`          | `> 0`, default `1000`     |
| `api_key_encrypted`   | `text`         | nullable                  |
| `is_enabled`          | `boolean`      | default `true`            |
| `updated_at`          | `timestamptz`  | default `now()`           |

## Quan he chinh

- `roles 1-N accounts`
- `accounts 1-1 onboarding_profiles`
- `accounts 1-1 jar_setups`
- `accounts 1-N financial_accounts`
- `accounts 1-N categories`
- `accounts 1-N jars`
- `accounts 1-N import_jobs`
- `accounts 1-N transactions`
- `accounts 1-N spending_limits`
- `accounts 1-N goals`
- `accounts 1-N goal_contributions`
- `accounts 1-N reminders`
- `accounts 1-N notifications`
- `accounts 1-N broadcasts` voi admin account
- `financial_accounts 1-N import_jobs`
- `financial_accounts 1-N transactions`
- `jar_setups 1-N jars`
- `jars 1-N spending_limits`
- `jars 1-N goals` qua `goals.linked_jar_id`
- `jars 1-N goal_contributions` qua `goal_contributions.source_jar_id`
- `jars 1-N transactions` qua `transactions.from_jar_id` va `transactions.to_jar_id`
- `categories 1-N transactions`
- `categories 1-N spending_limits`
- `categories 1-N reminders`
- `import_jobs 1-N import_transaction_drafts`
- `import_jobs 1-N transactions`
- `goals 1-N goal_contributions`
- `broadcasts 1-N notifications`

## Bang/cot da loai khoi scope MVP 1 thang

Cac ten sau khong con nam trong schema hien tai va khong nen duoc tham chieu nhu persisted table/column:

- `financial_accounts.available_balance`
- `financial_accounts.balance_as_of`
- `jars.percentage`
- `jar_allocations`
- `jar_allocation_items`
- `jar_transfers`
- `transactions.jar_id`
- `transactions.amount`
- `import_transaction_drafts.suggested_note`
- `import_transaction_drafts.suggested_category_id`
- `import_transaction_drafts.suggested_jar_id`
- `goal_contributions.source_financial_account_id`
- `reminders.next_due_date`
