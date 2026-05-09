# Flow FE tích hợp API - Personal Finance App

Tài liệu này viết cho FE đọc để biết nên dựng màn hình nào, user thao tác ra sao, click nút nào thì gọi API nào, và sau response thì điều hướng/cập nhật UI thế nào.

Nguồn route và DTO hiện tại: `docs/API V2.md`.

Lưu ý quan trọng:

- Route trong file này bám theo backend hiện tại. Một số route chưa nằm dưới `/api/v1`, ví dụ `/User/me`, `/Onboarding`, `/FinancialAccount`, `/Jar`, `/Transactions`, `/user/dashboard`.
- Các API có ghi `Bearer` phải gửi header `Authorization: Bearer <accessToken>`.
- FE gửi `transactionsAmount` là số dương cho cả `Income` và `Expense`.
- App không giữ tiền thật. `FinancialAccount` và `Jar` chỉ là sổ theo dõi nội bộ.
- Admin dùng chung API login với user, sau đó phân quyền bằng role trong JWT.

---

## 1. Cách FE nên tổ chức navigation

### 1.1 Public routes

| Route UI gợi ý | Mục đích | API chính |
| --- | --- | --- |
| `/register` | Đăng ký user mới | `POST /api/v1/auth/register` |
| `/login` | Đăng nhập user/admin | `POST /api/v1/auth/login` |

### 1.2 User routes sau khi đăng nhập

| Route UI gợi ý | Mục đích | API chính |
| --- | --- | --- |
| `/setup-check` | Gate kiểm tra onboarding | `GET /User/me/setup` |
| `/onboarding` | User hoàn tất hồ sơ ban đầu | `POST /Onboarding` |
| `/dashboard` | Tổng quan tài chính cá nhân | `GET /user/dashboard` |
| `/accounts` | Nguồn tiền | `/FinancialAccount...` |
| `/jars` | Hũ chi tiêu | `/Jar...` |
| `/categories` | Danh mục | `/api/v1/categories...` |
| `/transactions` | Giao dịch | `/Transactions...` |
| `/imports/ocr` | Upload ảnh hóa đơn/OCR | `POST /api/v1/imports/image` |
| `/limits` | Hạn mức chi tiêu | `/api/v1/limits...` |
| `/goals` | Mục tiêu tiết kiệm | `/api/v1/goals...` |
| `/reminders` | Nhắc lịch thanh toán | `/api/v1/reminders...` |
| `/notifications` | Inbox thông báo | `/api/v1/notifications...` |
| `/ai-chat` | Chat tư vấn AI | `POST /api/v1/ai/chat` |
| `/profile` | Hồ sơ user | `/User/me...` |

### 1.3 Admin routes sau khi đăng nhập

| Route UI gợi ý | Mục đích | API chính |
| --- | --- | --- |
| `/admin/dashboard` | Dashboard vận hành | `GET /api/v1/admin/dashboard` |
| `/admin/users` | Quản lý user | `/api/v1/admin/users...` |
| `/admin/categories` | Quản lý default category | `/api/v1/admin/categories...` |
| `/admin/broadcasts` | Gửi thông báo toàn hệ thống | `/api/v1/admin/broadcasts...` |
| `/admin/audit-logs` | Xem audit log | `GET /api/v1/admin/audit-logs` |
| `/admin/ai-settings` | Cấu hình AI | `/api/v1/admin/ai-settings...` |

---

## 2. App boot và auth guard

### 2.1 Khi mở app

1. FE kiểm tra local storage/session storage có `accessToken` không.
2. Nếu không có token: đưa user về `/login`.
3. Nếu có token:
   - decode JWT để đọc role nếu FE cần phân route admin/user;
   - với user thường, gọi `GET /User/me/setup`;
   - nếu token lỗi/401 thì clear token và quay về `/login`.

### 2.2 Điều hướng sau khi có token

| Điều kiện | Điều hướng |
| --- | --- |
| JWT role là `Admin` | `/admin/dashboard` |
| JWT role là `User` và `isOnboardingCompleted = false` | `/onboarding` |
| JWT role là `User` và `isOnboardingCompleted = true` | `/dashboard` |

Ghi chú FE:

- Response login/register hiện không trả role riêng trong body. Nếu cần phân biệt Admin/User ngay sau login, FE nên decode claim role từ JWT.
- Sau mọi mutation lớn như tạo transaction, xóa account, tạo goal, nên refresh màn hình hiện tại và dashboard nếu user đang ở dashboard.

---

## 3. Auth flow

### 3.1 Đăng ký user mới

UI `/register`:

| Field | Input |
| --- | --- |
| Username | text input |
| Email | email input |
| Password | password input |
| First name | text input |
| Last name | text input |

Flow:

1. User nhập `username`, `email`, `password`, `firstName`, `lastName`.
2. User click nút `Đăng ký`.
3. FE validate cơ bản: required fields, email format, password không rỗng.
4. FE gọi:

```http
POST /api/v1/auth/register
```

Body:

```json
{
  "username": "string",
  "email": "string",
  "password": "string",
  "firstName": "string",
  "lastName": "string"
}
```

5. Nếu thành công `201 Created`:
   - lưu `accessToken`;
   - gọi `GET /User/me/setup` hoặc chuyển thẳng `/onboarding`;
   - khuyến nghị vẫn gọi `/User/me/setup` để dùng chung logic gate.
6. Nếu lỗi:
   - hiển thị lỗi ở form;
   - giữ user ở `/register`;
   - không clear input trừ password nếu muốn an toàn hơn.

UX mong muốn:

- Nút `Đăng ký` disabled khi đang submit.
- Có link sang `/login`.
- Sau đăng ký thành công không bắt user login lại.

### 3.2 Đăng nhập

UI `/login`:

| Field | Input |
| --- | --- |
| Email | email input |
| Password | password input |

Flow:

1. User nhập `email`, `password`.
2. User click `Đăng nhập`.
3. FE gọi:

```http
POST /api/v1/auth/login
```

Body:

```json
{
  "email": "string",
  "password": "string"
}
```

4. Nếu thành công:
   - lưu `accessToken`;
   - decode JWT để lấy role.
5. Nếu role là `Admin`: chuyển `/admin/dashboard`.
6. Nếu role là `User`: gọi `GET /User/me/setup`.
7. Nếu `isOnboardingCompleted = false`: chuyển `/onboarding`.
8. Nếu `isOnboardingCompleted = true`: chuyển `/dashboard`.
9. Nếu lỗi login: show message `Email hoặc mật khẩu không đúng`.

### 3.3 Logout

UI:

- Có nút logout trong user menu/sidebar.

Flow:

1. User click `Đăng xuất`.
2. FE gọi nếu đang có token:

```http
POST /api/v1/auth/logout
```

3. Dù API thành công hay lỗi, FE có thể clear token local và chuyển `/login`.

---

## 4. Setup gate và onboarding

### 4.1 Setup gate

API:

```http
GET /User/me/setup
Authorization: Bearer <token>
```

FE dùng response để điều hướng:

| Field response | FE dùng để làm gì |
| --- | --- |
| `isOnboardingCompleted` | quyết định vào onboarding hay dashboard |
| `monthlyIncome` | hiển thị lại nếu có setup summary |
| `budgetMethod` | hiển thị method hiện tại |
| `defaultFinancialAccountId` | chọn default account khi tạo transaction |
| `jarCount` | cảnh báo user chưa có hũ |
| `financialAccountCount` | cảnh báo user chưa có nguồn tiền |
| `limitCount` | gợi ý tạo limit |
| `activeGoalCount` | gợi ý tạo goal |

Flow:

1. Sau login/register, FE gọi setup.
2. Nếu chưa onboarding: `/onboarding`.
3. Nếu đã onboarding: `/dashboard`.

### 4.2 Onboarding

UI `/onboarding` nên là wizard 3-5 bước:

| Step | UI |
| --- | --- |
| Thu nhập | input `monthlyIncome` |
| Nghề nghiệp | select/input `occupationType` |
| Mục tiêu tài chính | multi-select `financialGoalTypes` |
| Phương pháp ngân sách | segmented control `SixJars`, `Rule503020`, `Custom` |
| Độ tuổi và khó khăn chi tiêu | select `ageRange`, multi-select `spendingChallenges` |

Submit:

```http
POST /Onboarding
Authorization: Bearer <token>
```

Body:

```json
{
  "monthlyIncome": 0,
  "occupationType": "string",
  "financialGoalTypes": ["string"],
  "budgetMethodPreference": "SixJars",
  "ageRange": "string",
  "spendingChallenges": ["string"]
}
```

Sau response:

1. FE có thể hiển thị màn hình `Onboarding result`:
   - recommended method;
   - recommended categories;
   - recommended jars;
   - default financial account.
2. User click `Vào dashboard`.
3. FE chuyển `/dashboard`.
4. Dashboard gọi `GET /user/dashboard`.

UX:

- Nếu user chọn `Custom`, nói rõ họ sẽ tự tạo hũ ở màn `/jars`.
- Không cần màn hình tạo Cash account riêng vì backend tạo default Cash sau onboarding.

---

## 5. Dashboard flow

UI `/dashboard`:

Gọi khi vào màn:

```http
GET /user/dashboard
Authorization: Bearer <token>
```

Render các vùng:

| Vùng UI | Data |
| --- | --- |
| Tổng quan số dư | `balanceSummary.totalBalance`, `allocatedBalance`, `unallocatedBalance` |
| Thu/chi | `totalIncome`, `totalExpense`, `netChange` |
| Nguồn tiền | `financialAccounts` |
| Hũ | `jarSummary` |
| Chi theo category | `categoryBreakdown` |
| Giao dịch gần đây | `recentTransactions` |
| Mục tiêu | `goalProgress` |

CTA nên có trên dashboard:

- `Thêm giao dịch` -> mở modal transaction hoặc `/transactions/new`.
- `Thêm nguồn tiền` -> `/accounts`.
- `Tạo hũ` -> `/jars`.
- `Tạo hạn mức` -> `/limits`.
- `Tạo mục tiêu` -> `/goals`.

Empty state:

- Nếu `financialAccounts` rỗng: CTA `Tạo nguồn tiền`.
- Nếu `jarSummary` rỗng: CTA `Tạo hũ`.
- Nếu `recentTransactions` rỗng: CTA `Thêm giao dịch đầu tiên`.

---

## 6. Profile flow

### 6.1 Xem profile

UI `/profile` gọi:

```http
GET /User/me
Authorization: Bearer <token>
```

Render:

- username;
- first name;
- last name;
- email;
- phone;
- avatar;
- preferred currency;
- onboarding status.

### 6.2 Cập nhật profile

Form edit:

| Field | API field |
| --- | --- |
| First name | `firstName` |
| Last name | `lastName` |
| Phone | `phone` |
| Avatar URL | `avatarUrl` |

Submit:

```http
PATCH /User/me
Authorization: Bearer <token>
```

Sau response:

- update UI profile header/avatar;
- show toast thành công;
- không cần logout/login lại.

---

## 7. Financial account flow

### 7.1 Màn danh sách nguồn tiền

UI `/accounts` gọi:

```http
GET /FinancialAccount
Authorization: Bearer <token>
```

Render mỗi item:

- name;
- account type;
- connection mode;
- provider/masked account number nếu linked;
- current balance;
- sync status;
- default badge;
- active/inactive badge.

CTA:

- `Thêm nguồn tiền thủ công`.
- `Liên kết ngân hàng qua Casso`.
- `Sửa`.
- `Xóa/Ngừng theo dõi`.
- `Sync Casso` nếu account là `LinkedApi`.

### 7.2 Tạo nguồn tiền thủ công

Modal/form:

| Field | UI |
| --- | --- |
| `name` | text |
| `accountType` | select `Cash`, `Bank`, `EWallet`, `Other` |
| `currentBalance` | money input |
| `currency` | default `VND` |
| `isDefault` | checkbox |

Submit:

```http
POST /FinancialAccount/Manual
Authorization: Bearer <token>
```

Sau response:

- đóng modal;
- refresh `GET /FinancialAccount`;
- nếu đang ở dashboard thì refresh dashboard.

### 7.3 Liên kết ngân hàng/Casso

Modal/form:

| Field | UI |
| --- | --- |
| `bankName` | text |
| `bankCode` | optional text |
| `accountNumber` | text |
| `accountHolderName` | optional text |
| `isDefault` | checkbox |

Submit:

```http
POST /FinancialAccount/LinkApi
Authorization: Bearer <token>
```

Sau response:

- hiển thị account với `providerName`, `maskedAccountNumber`, `syncStatus`;
- refresh list.

UX:

- Không cho nhập/sửa balance thủ công đối với linked account.
- Gắn nút `Sync Casso` để kéo giao dịch.

### 7.4 Sửa nguồn tiền

Form:

| Field | Ghi chú |
| --- | --- |
| `name` | optional |
| `currentBalance` | chỉ cho manual account |
| `isDefault` | optional |

Submit:

```http
PATCH /FinancialAccount/{id}
Authorization: Bearer <token>
```

Sau response:

- update row/card;
- refresh dashboard nếu balance đổi.

### 7.5 Xóa/ngừng theo dõi nguồn tiền

UX:

1. User click `Xóa`.
2. FE mở confirm dialog: `Nguồn tiền sẽ được ngừng theo dõi, dữ liệu cũ không bị xóa vĩnh viễn.`
3. User confirm.

API:

```http
DELETE /FinancialAccount/{id}
Authorization: Bearer <token>
```

Sau response:

- refresh list;
- show toast.

---

## 8. Category flow

### 8.1 Màn danh mục

UI `/categories` gọi:

```http
GET /api/v1/categories
Authorization: Bearer <token>
```

Render 2 section:

| Section | Quyền FE |
| --- | --- |
| Default categories | chỉ xem |
| Custom categories | tạo/sửa/xóa |

### 8.2 Tạo custom category

Form:

| Field | UI |
| --- | --- |
| `name` | text |
| `icon` | icon picker/text |
| `color` | color picker |

Submit:

```http
POST /api/v1/categories
Authorization: Bearer <token>
```

Sau response:

- thêm item vào custom list hoặc refresh list.

### 8.3 Sửa custom category

```http
PATCH /api/v1/categories/{id}
Authorization: Bearer <token>
```

Sau response:

- update item trong list;
- transaction cũ dùng category này vẫn hiển thị tên mới.

### 8.4 Xóa custom category

```http
DELETE /api/v1/categories/{id}
Authorization: Bearer <token>
```

UX:

- Confirm trước khi xóa.
- Sau xóa refresh category list.

---

## 9. Jar flow

### 9.1 Màn danh sách hũ

UI `/jars` gọi:

```http
GET /Jar
Authorization: Bearer <token>
```

Render:

- method type;
- total jar balance;
- unallocated balance;
- list jars: name, balance, color, icon, status.

CTA:

- `Tạo hũ`;
- `Sửa hũ`;
- `Lưu trữ hũ`.

### 9.2 Tạo hũ

Form:

| Field | UI |
| --- | --- |
| `name` | text |
| `color` | color picker |
| `icon` | icon picker/text |

Submit:

```http
POST /Jar
Authorization: Bearer <token>
```

Sau response:

- refresh `GET /Jar`;
- nếu goal/transaction form đang mở, refresh jar dropdown.

### 9.3 Sửa hũ

```http
PATCH /Jar/{id}
Authorization: Bearer <token>
```

Body:

```json
{
  "name": "string",
  "color": "string",
  "icon": "string"
}
```

### 9.4 Lưu trữ hũ

```http
DELETE /Jar/{id}
Authorization: Bearer <token>
```

UX:

- Label nên là `Lưu trữ` thay vì `Xóa vĩnh viễn`, vì backend chuyển status `Archived`.
- Không có API chỉnh balance hũ trực tiếp. Balance thay đổi qua transaction.

---

## 10. Transaction flow

### 10.1 Màn danh sách giao dịch

Khi vào `/transactions`, FE nên gọi song song:

```http
GET /Transactions?pageIndex=1&pageSize=20
GET /FinancialAccount
GET /Jar
GET /api/v1/categories
```

Lý do:

- transaction list cần filter;
- form tạo/sửa transaction cần dropdown account/jar/category.

Filter UI:

| Filter | Query |
| --- | --- |
| Page | `pageIndex`, `pageSize` |
| Nguồn tiền | `financialAccountId` |
| Loại | `type` |
| Hũ | `jarId` |
| Category | `categoryId` |
| Khoảng ngày | `fromDate`, `toDate` |
| Search note/raw description | `keyword` |
| Sort | `sortBy`, `sortDir` |

### 10.2 Tạo giao dịch thu nhập

UI `/transactions/new` hoặc modal:

| Field | UI |
| --- | --- |
| Type | select `Income` |
| Amount | money input, số dương |
| Financial account | dropdown manual account |
| Category | optional dropdown |
| Note | optional textarea |
| Date | date/time picker |

Submit:

```http
POST /Transactions
Authorization: Bearer <token>
```

Body gợi ý:

```json
{
  "financialAccountId": "guid",
  "type": "Income",
  "transactionsAmount": 100000,
  "categoryId": "guid",
  "fromJarId": null,
  "toJarId": null,
  "note": "string",
  "date": "datetimeOffset"
}
```

Sau response:

- đóng modal;
- refresh transaction list;
- refresh account list/dashboard.

### 10.3 Tạo giao dịch chi tiêu từ hũ

Theo code hiện tại, flow expense rõ nhất là chi từ hũ.

UI:

| Field | UI |
| --- | --- |
| Type | select `Expense` |
| Amount | money input, số dương |
| From jar | dropdown jar |
| Category | dropdown |
| Note | optional textarea |
| Date | date/time picker |

Submit:

```http
POST /Transactions
Authorization: Bearer <token>
```

Body gợi ý:

```json
{
  "financialAccountId": null,
  "type": "Expense",
  "transactionsAmount": 50000,
  "categoryId": "guid",
  "fromJarId": "guid",
  "toJarId": null,
  "note": "string",
  "date": "datetimeOffset"
}
```

Sau response:

- refresh transaction list;
- refresh jars;
- refresh limits nếu expense liên quan limit;
- refresh notifications badge vì backend có thể tạo `SpendingAlert`.

UX:

- Nếu backend trả lỗi không đủ tiền, show lỗi ngay dưới amount/jar.
- Không gửi amount âm.

### 10.4 Transfer/internal jar movement

Code service hiện có nhánh `Transfer`, nhưng public contract MVP không khuyến nghị dựng UI transfer chính. FE chỉ nên dựng màn transfer nếu team chốt dùng capability này.

Nếu chưa chốt, không đưa `Transfer` vào dropdown type chính; chỉ hiển thị `Income` và `Expense`.

### 10.5 Sửa giao dịch

UI:

- mở edit modal từ row transaction.
- chỉ cho sửa amount, category, note.
- không cho đổi type/account/jar/date theo API hiện tại.

API:

```http
PATCH /Transactions/{id}
Authorization: Bearer <token>
```

Body:

```json
{
  "transactionsAmount": 50000,
  "categoryId": "guid",
  "note": "string"
}
```

Sau response:

- refresh row/list;
- refresh dashboard/jars/accounts nếu amount đổi.

### 10.6 Xóa giao dịch

UX:

1. User click `Xóa`.
2. Confirm dialog.
3. Gọi API.

```http
DELETE /Transactions/{id}
Authorization: Bearer <token>
```

Sau response:

- remove row hoặc refresh list;
- refresh dashboard/jars/accounts.

Ghi chú:

- Imported/linked transaction có thể không được sửa/xóa manual.

---

## 11. Casso sync flow

### 11.1 User bấm sync linked account

Màn dùng: `/accounts` hoặc account detail.

Điều kiện hiển thị nút:

- account `connectionMode = LinkedApi`;
- account active.

API:

```http
GET /Transactions/Casso?financialAccountId={id}&page=1&pageSize=50&sort=ASC
Authorization: Bearer <token>
```

Optional query:

- `fromDate`;
- `toDate`.

Sau response:

- show toast: `Đã nhận X, tạo Y, bỏ qua Z`.
- refresh transactions;
- refresh account balance;
- refresh dashboard.

### 11.2 Webhook Casso

Endpoint:

```http
POST /Transactions/Casso
```

FE không gọi API này. Đây là endpoint để Casso/server ngoài gọi vào backend.

---

## 12. OCR/import flow

### 12.1 Upload ảnh hóa đơn

UI `/imports/ocr`:

| Field | UI |
| --- | --- |
| `file` | file picker |
| `layout` | optional select/input |
| `runOcr` | toggle |

Submit:

```http
POST /api/v1/imports/image
Authorization: Bearer <token>
Content-Type: multipart/form-data
```

Form data:

- `file`;
- `layout`;
- `runOcr`.

Sau response:

1. Nếu `ocrResult.isSuccess = true`:
   - show extracted text/raw OCR section;
   - cho user copy hoặc bấm `Tạo giao dịch từ kết quả OCR`.
2. Nếu OCR lỗi nhưng upload thành công:
   - show file uploaded;
   - show OCR error;
   - cho user nhập transaction thủ công.

Giới hạn hiện tại:

- Chưa có API preview/confirm import statement.
- Chưa có API biến OCR result thành transaction tự động.
- FE muốn ghi sổ vẫn phải gọi `POST /Transactions` sau khi user xác nhận thông tin.

---

## 13. Limit flow

### 13.1 Màn hạn mức

Khi vào `/limits`, FE gọi:

```http
GET /api/v1/limits
GET /Jar
GET /api/v1/categories
```

Render mỗi limit:

- target type;
- target name;
- limit amount;
- period;
- alert percentage;
- current spent;
- current percentage;
- status.

### 13.2 Tạo hạn mức

UI form:

| Field | UI |
| --- | --- |
| Target type | segmented `Jar` / `Category` |
| Target | dropdown jars/categories |
| Limit amount | money input |
| Period | select `Daily`, `Monthly` |
| Alert at percentage | number/slider 1-100 |

Submit:

```http
POST /api/v1/limits
Authorization: Bearer <token>
```

Body:

```json
{
  "targetType": "Jar",
  "targetId": "guid",
  "limitAmount": 1000000,
  "period": "Monthly",
  "alertAtPercentage": 80
}
```

Sau response:

- refresh limit list;
- show toast.

### 13.3 Sửa hạn mức

API chỉ sửa amount và alert percentage:

```http
PATCH /api/v1/limits/{id}
Authorization: Bearer <token>
```

Body:

```json
{
  "limitAmount": 1200000,
  "alertAtPercentage": 80
}
```

### 13.4 Xóa hạn mức

```http
DELETE /api/v1/limits/{id}
Authorization: Bearer <token>
```

Sau response:

- refresh list.

---

## 14. Goal flow

### 14.1 Màn mục tiêu

Khi vào `/goals`, FE gọi:

```http
GET /api/v1/goals
GET /Jar
```

Render:

- title;
- target amount;
- saved amount;
- progress percentage;
- due date;
- status;
- suggested monthly contribution.

### 14.2 Xem chi tiết goal

Khi user click một goal:

```http
GET /api/v1/goals/{id}
Authorization: Bearer <token>
```

Render thêm:

- days remaining;
- linked jar;
- suggested monthly contribution.

### 14.3 Tạo goal

Form:

| Field | UI |
| --- | --- |
| `title` | text |
| `targetAmount` | money input |
| `dueDate` | date picker |
| `linkedJarId` | optional jar dropdown |
| `note` | optional textarea |

Submit:

```http
POST /api/v1/goals
Authorization: Bearer <token>
```

Sau response:

- chuyển về `/goals` hoặc mở detail;
- refresh dashboard goal progress.

### 14.4 Sửa goal

```http
PATCH /api/v1/goals/{id}
Authorization: Bearer <token>
```

### 14.5 Xóa/hủy goal

```http
DELETE /api/v1/goals/{id}
Authorization: Bearer <token>
```

UX:

- Label nên là `Hủy mục tiêu` vì backend chuyển status `Cancelled`.

---

## 15. Reminder flow

### 15.1 Màn reminders

Khi vào `/reminders`, FE gọi:

```http
GET /api/v1/reminders
GET /api/v1/categories
```

Render:

- title;
- amount;
- frequency;
- next due date;
- status.

### 15.2 Tạo reminder

Form:

| Field | UI |
| --- | --- |
| `title` | text |
| `amount` | money input |
| `frequency` | select `Daily`, `Weekly`, `Monthly`, `Quarterly`, `Yearly` |
| `dayOfMonth` | optional number 1-31 |
| `startDate` | date picker |
| `categoryId` | optional category dropdown |
| `notifyDaysBefore` | number |
| `note` | optional textarea |

Submit:

```http
POST /api/v1/reminders
Authorization: Bearer <token>
```

Sau response:

- show card mới với `nextDueDate`;
- refresh list.

### 15.3 Sửa reminder/status

```http
PATCH /api/v1/reminders/{id}
Authorization: Bearer <token>
```

Body có thể gồm:

- title;
- amount;
- frequency;
- dayOfMonth;
- status `Active`, `Paused`, `Completed`, `Cancelled`;
- notifyDaysBefore;
- note.

### 15.4 Xóa reminder

```http
DELETE /api/v1/reminders/{id}
Authorization: Bearer <token>
```

UX:

- Label nên là `Hủy nhắc nhở`, vì backend chuyển status `Cancelled`.

---

## 16. Notification flow

### 16.1 Notification badge

Ở layout sau login, FE có thể gọi:

```http
GET /api/v1/notifications?pageIndex=1&pageSize=5&status=unread
Authorization: Bearer <token>
```

Dùng `unreadCount` để hiển thị badge.

### 16.2 Inbox notification

UI `/notifications`:

Filter:

| UI | Query |
| --- | --- |
| Type | `type` |
| Read/unread | `status` |
| Pagination | `pageIndex`, `pageSize` |

API:

```http
GET /api/v1/notifications?type=SpendingAlert&status=unread&pageIndex=1&pageSize=20
Authorization: Bearer <token>
```

### 16.3 Mark read/unread

Mark selected:

```http
PATCH /api/v1/notifications/status
Authorization: Bearer <token>
```

Body:

```json
{
  "ids": ["guid"],
  "isRead": true,
  "markAll": false
}
```

Mark all:

```json
{
  "ids": null,
  "isRead": true,
  "markAll": true
}
```

Sau response:

- update list;
- update unread badge từ `unreadCount`.

---

## 17. AI chat flow

UI `/ai-chat` hoặc sidebar chat:

Input:

- message textarea;
- send button;
- optional suggestions chips từ response trước.

Submit:

```http
POST /api/v1/ai/chat
Authorization: Bearer <token>
```

Body:

```json
{
  "message": "string",
  "recentMessages": [
    {
      "sender": "user",
      "content": "string"
    }
  ]
}
```

Sau response:

- append answer vào chat;
- render `suggestions` thành quick action chips;
- hiển thị nhỏ `source` là `AI` hoặc `RuleBased`.

UX:

- Nếu API lỗi, show fallback UI: `Hiện chưa thể lấy tư vấn, vui lòng thử lại`.
- Không hiển thị setting/API key/provider secret ở UI user.

---

## 18. Admin flow

### 18.1 Admin login

Admin dùng:

```http
POST /api/v1/auth/login
```

Sau response:

1. FE lưu token.
2. Decode JWT role.
3. Nếu role `Admin`: chuyển `/admin/dashboard`.
4. Nếu không phải admin mà vào admin route: redirect `/dashboard` hoặc show 403 page.

### 18.2 Admin dashboard

UI `/admin/dashboard` gọi:

```http
GET /api/v1/admin/dashboard
Authorization: Bearer <adminToken>
```

Render:

- summary cards;
- recent users;
- recent transactions.

### 18.3 Admin users

List:

```http
GET /api/v1/admin/users?pageIndex=1&pageSize=20&status=Active&keyword=abc
Authorization: Bearer <adminToken>
```

Detail:

```http
GET /api/v1/admin/users/{id}
Authorization: Bearer <adminToken>
```

Update status:

```http
PATCH /api/v1/admin/users/{id}/status
Authorization: Bearer <adminToken>
```

Body:

```json
{
  "status": "Banned",
  "statusReason": "string"
}
```

Change role:

```http
PATCH /api/v1/change-role/{accountId}?role=Admin
Authorization: Bearer <adminToken>
```

UX:

- Status action nên là explicit: `Ban user` hoặc `Unban user`.
- Confirm trước khi ban hoặc đổi role.

### 18.4 Admin default categories

List:

```http
GET /api/v1/admin/categories?isActive=true
Authorization: Bearer <adminToken>
```

Create:

```http
POST /api/v1/admin/categories
Authorization: Bearer <adminToken>
```

Body:

```json
{
  "name": "string",
  "icon": "string",
  "color": "string",
  "order": 1
}
```

Update:

```http
PATCH /api/v1/admin/categories/{id}
Authorization: Bearer <adminToken>
```

Delete/deactivate:

```http
DELETE /api/v1/admin/categories/{id}
Authorization: Bearer <adminToken>
```

### 18.5 Admin broadcasts

List:

```http
GET /api/v1/admin/broadcasts?pageIndex=1&pageSize=20&status=Queued
Authorization: Bearer <adminToken>
```

Create/send:

```http
POST /api/v1/admin/broadcasts
Authorization: Bearer <adminToken>
```

Body gửi ngay:

```json
{
  "title": "string",
  "body": "string",
  "targetAudience": "All",
  "scheduledAt": null
}
```

Body hẹn giờ:

```json
{
  "title": "string",
  "body": "string",
  "targetAudience": "All",
  "scheduledAt": "datetimeOffset"
}
```

UX:

- Nếu `scheduledAt = null`, show trạng thái `Sent`.
- Nếu có `scheduledAt`, show trạng thái `Queued`.
- Scheduled dispatch job hiện cần backend bổ sung nếu muốn tự gửi đúng giờ.

### 18.6 Admin audit logs

UI `/admin/audit-logs` gọi:

```http
GET /api/v1/admin/audit-logs?page=1&pageSize=20
Authorization: Bearer <adminToken>
```

Filter:

- adminId;
- actionType;
- entityType;
- fromDate;
- toDate.

### 18.7 Admin AI settings

Load form:

```http
GET /api/v1/admin/ai-settings
Authorization: Bearer <adminToken>
```

Render:

- model name;
- system prompt;
- temperature;
- max tokens;
- enabled switch;
- api key masked.

Update:

```http
PATCH /api/v1/admin/ai-settings
Authorization: Bearer <adminToken>
```

Body:

```json
{
  "modelName": "string",
  "systemPrompt": "string",
  "temperature": 0.7,
  "maxTokens": 1000,
  "isEnabled": true
}
```

UX:

- Không có field nhập raw API key theo DTO hiện tại.
- Không hiển thị secret thô ở UI.

---

## 19. Health/dev flow

Các API này chủ yếu cho dev/ops, không phải core user UI:

```http
GET /health
GET /health/db/local
GET /health/db/render
```

FE production không cần gọi thường xuyên. Có thể dùng ở trang admin/dev diagnostics nếu team muốn.

---

## 20. Checklist FE khi nối API

1. Luôn có loading state cho nút submit và màn list.
2. Protected API luôn gửi Bearer token.
3. Nếu nhận 401: clear token và redirect `/login`.
4. Nếu nhận 403: show forbidden page hoặc redirect theo role.
5. Sau create/update/delete, refresh list đang xem.
6. Sau transaction/account/jar mutation, refresh dashboard nếu dashboard đang cache.
7. Amount nhập từ FE gửi số dương.
8. Không dựng public UI cho API chưa có trong `docs/API V2.md`.
9. Không hiển thị route/admin menu nếu JWT role không phù hợp.
10. Không expose secret config, API key, secure token trên FE.

---

## 21. Các khoảng trống cần backend bổ sung nếu FE muốn làm đủ flow

| Nhu cầu FE | Hiện trạng | Cần backend bổ sung |
| --- | --- | --- |
| Import statement đầy đủ | Chỉ có `POST /api/v1/imports/image` | create job, status, preview, edit draft, confirm |
| Reminder tự sinh notification | Có CRUD reminder | background job tạo notification đúng kỳ |
| Scheduled broadcast gửi đúng giờ | Có create queued broadcast | job dispatch queued broadcast |
| Route đồng nhất `/api/v1` | Một số route vẫn là `/User`, `/Jar`, `/Transactions` | align controller route nếu muốn public contract sạch |
| Expense trực tiếp từ account | Transaction expense hiện rõ nhất là từ jar | chốt nghiệp vụ và xử lý balance account nếu cần |
| Category limit alert đầy đủ | Limit CRUD có category target | transaction service cần check category limit |
