# Personal Finance Manager — Core API Contracts

Tài liệu này mô tả **API contract chính cho client/frontend** của hệ thống **Personal Finance Manager**.  
Nội dung được viết lại dựa trên:

- [overview.md](/d:/Coding/Project/prj_pied/Personal_Finance_App_Be/docs/overview.md)
- [user story.md](/d:/Coding/Project/prj_pied/Personal_Finance_App_Be/docs/user%20story.md)
- [backend_internal_model.md](/d:/Coding/Project/prj_pied/Personal_Finance_App_Be/docs/backend_internal_model.md)

Mục tiêu của bản này:

1. API phải dễ dùng cho **frontend và user flow**, không lộ quá nhiều chi tiết xử lý nội bộ của backend.
2. API phải bám vào **core scope 1 tháng** và không bị thiếu các endpoint cốt lõi trong `user story.md`.

Tài liệu này ưu tiên:

- request ngắn, rõ ý định nghiệp vụ;
- response vừa đủ để FE render hoặc cập nhật state;
- không nhồi các field nội bộ mà FE không cần biết;
- tập trung vào `core MVP`.

## 1. Nguyên tắc thiết kế bản API này

### 1.1. Viết theo góc nhìn client

- Request chỉ chứa dữ liệu user thật sự nhập hoặc chọn.
- Response chỉ trả dữ liệu FE thật sự cần để hiển thị hoặc cập nhật UI.
- Backend tự xử lý validation nghiệp vụ, transaction, audit log, side effects và tính toán nội bộ.

### 1.2. API không bám theo database schema

- FE không cần biết entity nội bộ, aggregate hay bảng DB.
- FE không gửi các field như `newBalance`, `progressPercentage`, `status` nếu backend tự tính được.
- Backend có thể map từ DTO ngoài vào internal model riêng.

### 1.3. Thin write, rich read

- API ghi dữ liệu: request ngắn, response gọn.
- API đọc dữ liệu: response giàu hơn để render màn hình.

### 1.4. Chỉ giữ core API cho MVP

Bản này tập trung vào các nhóm core:

- Auth và profile
- Onboarding
- Categories
- Jars
- Transactions
- Dashboard
- Limits và notifications
- Goals
- Reminders
- Import statement
- Admin core operations

Các API `scale` hoặc `optional` sẽ không được mô tả chi tiết trong bản core này.

## 2. Core Coverage Check

Bảng dưới đây dùng để kiểm tra xem `apis.md` cũ có thiếu core API nào không, và bản mới đã bổ sung gì.

| Nhóm core story | Cần có theo user story | Tình trạng bản cũ | Trạng thái bản mới |
| --- | --- | --- | --- |
| Auth user | Register, login, refresh, logout | Có | Giữ lại, rút gọn payload |
| Auth admin | Admin login | Có | Giữ lại, rút gọn payload |
| Profile + financial setup | Xem/cập nhật profile, onboarding summary, budgeting settings | Chưa rõ phần financial setup summary | Bổ sung `GET /users/me/setup` |
| Onboarding | Submit onboarding, nhận jar/category recommendation | Có | Giữ lại, request/response gọn hơn |
| Category user | Get default/custom, CRUD custom category | Có | Giữ lại |
| Jar CRUD | Create, edit, delete, view jars | Thiếu create/edit/delete jar riêng | Bổ sung `POST/PATCH/DELETE /jars` |
| Jar allocation/transfer | Allocate, transfer, transfer history | Thiếu transfer history | Bổ sung `GET /jars/transfers` |
| Transaction core | CRUD transactions, list/filter | Có | Giữ lại, làm gọn response |
| Import statement | Upload, status, preview, confirm | Có | Giữ lại, đổi route rõ hơn cho FE |
| Dashboard | Balance summary, recent tx, jar/category summary, goal snapshot | Có | Giữ lại |
| Spending limits | CRUD limits | Có nhưng update/delete chưa rõ contract | Viết rõ đầy đủ |
| Notifications | Inbox, filter, mark read/unread | Chỉ rõ phần mark read, chưa rõ unread | Bổ sung `PATCH /notifications/status` |
| Goals | CRUD goals, progress, contribution | Thiếu update/delete/detail goal | Bổ sung `GET /goals/{id}`, `PATCH`, `DELETE` |
| Reminders | CRUD recurring reminders | Chỉ có create | Bổ sung `GET/PATCH/DELETE /reminders` |
| Admin users | List, search, detail, ban/unban | Có | Giữ lại |
| Admin categories | CRUD default categories | Thiếu GET list rõ ràng | Bổ sung `GET /admin/categories` |
| Admin broadcasts | Create/send broadcast, history | Thiếu history rõ ràng | Bổ sung `GET /admin/broadcasts` |
| Admin dashboard | Operational metrics | Có | Giữ lại |
| Audit logs | View audit logs | Có | Giữ lại |
| AI settings | Get/update AI settings | Có | Giữ lại |

Kết luận coverage:

- `apis.md` cũ **chưa đủ core scope** nếu bám sát `user story.md`.
- Các nhóm thiếu rõ nhất là:
  - `Jar CRUD`
  - `Transfer history`
  - `Goal update/delete/detail`
  - `Reminder list/update/delete`
  - `Notification read/unread rõ ràng`
  - `Admin category list`
  - `Admin broadcast history`
  - `Financial setup summary cho user`

## 3. Conventions Chung

### 3.1. Base URL

`/api/v1`

### 3.2. Auth

- `Public`: không cần token
- `Bearer`: user access token
- `Admin`: access token với role `Admin` hoặc `SuperAdmin`

### 3.3. Tiền tệ và thời gian

- Mọi amount/balance/limit/target dùng `decimal`
- Không dùng `float` hoặc `double`
- Thời gian trả về theo `ISO8601`

### 3.4. Pagination mặc định

Các API list dùng format:

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

### 3.5. Error format

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

## 4. Scope quyết định cho bản API core này

### Được đưa vào tài liệu này

- Tất cả `core user stories`
- Tất cả `core admin stories`
- Các flow `P0` đến `P6`

### Chưa mô tả chi tiết ở bản core này

- OCR hóa đơn bằng ảnh riêng cho receipt
- AI advice cho user
- Goal saving plan generator
- Export transaction history
- Bank link
- Shared jar
- Chatbot session/message

Lý do:

- các mục trên đang nằm ở `scale` hoặc `optional` trong `user story.md`;
- tài liệu này được chốt như bản mô tả chính cho **core 1 tháng**.

## 5. API Contracts Theo Nhóm Flow

## P0 — Authentication & Admin Foundation

### `POST /api/v1/auth/register`

- **Auth**: Public
- **Mục đích**: tạo tài khoản user

**Request**

```json
{
  "username": "string",
  "email": "string",
  "password": "string",
  "firstName": "string",
  "lastName": "string"
}
```

**Response `201 Created`**

```json
{
  "id": "guid",
  "username": "string",
  "fullName": "string",
  "email": "string",
  "createdAt": "ISO8601"
}
```

**Notes**

- `409 Conflict` nếu username hoặc email đã tồn tại
- password hash dùng `BCrypt`

### `POST /api/v1/auth/login`

- **Auth**: Public
- **Mục đích**: đăng nhập user

**Request**

```json
{
  "username": "string",
  "password": "string"
}
```

**Response `200 OK`**

```json
{
  "accessToken": "string",
  "refreshToken": "string",
  "expiresIn": 900,
  "user": {
    "id": "guid",
    "username": "string",
    "fullName": "string",
    "email": "string",
    "role": "User | Admin",
    "isOnboardingCompleted": false
  }
}
```

**Notes**

- `401 Unauthorized` nếu sai credential
- `403 Forbidden` nếu account bị banned

### `POST /api/v1/auth/refresh`

- **Auth**: Public

**Request**

```json
{
  "refreshToken": "string"
}
```

**Response `200 OK`**

```json
{
  "accessToken": "string",
  "refreshToken": "string",
  "expiresIn": 900
}
```

### `POST /api/v1/auth/logout`

- **Auth**: Bearer

**Request**

```json
{
  "refreshToken": "string"
}
```

**Response `200 OK`**

```json
{
  "message": "Logged out successfully"
}
```

### `POST /api/v1/admin/auth/login`

- **Auth**: Public
- **Mục đích**: đăng nhập admin portal

**Request**

```json
{
  "username": "string",
  "password": "string"
}
```

**Response `200 OK`**

```json
{
  "accessToken": "string",
  "expiresIn": 3600,
  "admin": {
    "id": "guid",
    "username": "string",
    "role": "Admin | SuperAdmin"
  }
}
```

**Notes**

- tự động ghi audit log
- `403 Forbidden` nếu không phải role admin

## P1 — Onboarding, Profile, Category

### `GET /api/v1/users/me`

- **Auth**: Bearer
- **Mục đích**: lấy profile cơ bản của user

**Response `200 OK`**

```json
{
  "id": "guid",
  "username": "string",
  "fullName": "string",
  "email": "string",
  "phone": "string | null",
  "avatarUrl": "string | null",
  "preferredCurrency": "VND",
  "isOnboardingCompleted": true,
  "createdAt": "ISO8601",
  "updatedAt": "ISO8601"
}
```

### `PATCH /api/v1/users/me`

- **Auth**: Bearer

**Request**

```json
{
  "fullName": "string",
  "phone": "string | null",
  "avatarUrl": "string | null"
}
```

**Response `200 OK`**

```json
{
  "id": "guid",
  "fullName": "string",
  "phone": "string | null",
  "avatarUrl": "string | null",
  "updatedAt": "ISO8601"
}
```

### `GET /api/v1/users/me/setup`

- **Auth**: Bearer
- **Mục đích**: trả summary về financial setup để FE render màn hình hồ sơ tài chính

**Response `200 OK`**

```json
{
  "isOnboardingCompleted": true,
  "monthlyIncome": 15000000,
  "budgetMethod": "SixJars",
  "jarCount": 6,
  "limitCount": 2,
  "activeGoalCount": 1
}
```

### `POST /api/v1/users/onboarding`

- **Auth**: Bearer
- **Mục đích**: hoàn tất onboarding và nhận gợi ý cấu hình ban đầu

**Request**

```json
{
  "monthlyIncome": 15000000,
  "occupationType": "Employee",
  "financialGoalTypes": ["Saving", "DebtPayoff"],
  "budgetMethodPreference": "SixJars",
  "ageRange": "25-34",
  "spendingChallenges": ["Overspending", "NoTracking"]
}
```

**Response `200 OK`**

```json
{
  "recommendedMethod": "SixJars",
  "recommendedCategories": [
    {
      "name": "Ăn uống",
      "icon": "food"
    },
    {
      "name": "Di chuyển",
      "icon": "car"
    }
  ],
  "recommendedJars": [
    {
      "name": "Necessities",
      "percentage": 55
    },
    {
      "name": "Savings",
      "percentage": 10
    }
  ],
  "message": "Onboarding completed"
}
```

### `GET /api/v1/categories`

- **Auth**: Bearer
- **Mục đích**: lấy toàn bộ default categories và custom categories của user

**Response `200 OK`**

```json
{
  "defaultCategories": [
    {
      "id": "guid",
      "name": "Ăn uống",
      "icon": "food",
      "color": "#FF6B6B"
    }
  ],
  "customCategories": [
    {
      "id": "guid",
      "name": "Thú cưng",
      "icon": "pet",
      "color": "#A8DADC"
    }
  ]
}
```

### `POST /api/v1/categories`

- **Auth**: Bearer

**Request**

```json
{
  "name": "Thú cưng",
  "icon": "pet",
  "color": "#A8DADC"
}
```

**Response `201 Created`**

```json
{
  "id": "guid",
  "name": "Thú cưng",
  "icon": "pet",
  "color": "#A8DADC"
}
```

### `PATCH /api/v1/categories/{id}`

- **Auth**: Bearer

**Request**

```json
{
  "name": "string",
  "icon": "string",
  "color": "#RRGGBB"
}
```

**Response `200 OK`**

```json
{
  "id": "guid",
  "name": "string",
  "icon": "string",
  "color": "#RRGGBB",
  "updatedAt": "ISO8601"
}
```

**Notes**

- `403 Forbidden` nếu là default category hoặc không thuộc user hiện tại

### `DELETE /api/v1/categories/{id}`

- **Auth**: Bearer

**Response `200 OK`**

```json
{
  "message": "Category deleted"
}
```

**Notes**

- `409 Conflict` nếu category đang được transaction sử dụng

## P2 — Jars, Transactions, Import Statement

### `GET /api/v1/jars`

- **Auth**: Bearer
- **Mục đích**: lấy danh sách hũ để render màn hình quản lý hũ

**Response `200 OK`**

```json
{
  "methodType": "SixJars",
  "totalBalance": 15000000,
  "data": [
    {
      "id": "guid",
      "name": "Necessities",
      "percentage": 55,
      "balance": 8250000,
      "color": "#4CAF50",
      "icon": "home",
      "status": "Active"
    }
  ]
}
```

### `POST /api/v1/jars/setup`

- **Auth**: Bearer
- **Mục đích**: tạo bộ hũ ban đầu sau onboarding

**Request**

```json
{
  "methodType": "SixJars",
  "initialBalance": 15000000,
  "customJars": []
}
```

**Response `201 Created`**

```json
{
  "methodType": "SixJars",
  "data": [
    {
      "id": "guid",
      "name": "Necessities",
      "percentage": 55,
      "balance": 8250000
    }
  ],
  "message": "Jars created successfully"
}
```

**Notes**

- `409 Conflict` nếu user đã setup jars trước đó
- nếu `methodType = Custom` thì tổng `percentage` của `customJars` phải bằng `100`

### `POST /api/v1/jars`

- **Auth**: Bearer
- **Mục đích**: tạo thêm một hũ mới sau khi đã setup

**Request**

```json
{
  "name": "Du lịch",
  "percentage": 10,
  "color": "#2A9D8F",
  "icon": "plane"
}
```

**Response `201 Created`**

```json
{
  "id": "guid",
  "name": "Du lịch",
  "percentage": 10,
  "balance": 0,
  "status": "Active"
}
```

### `PATCH /api/v1/jars/{id}`

- **Auth**: Bearer

**Request**

```json
{
  "name": "Du lịch hè",
  "percentage": 12,
  "color": "#2A9D8F",
  "icon": "plane"
}
```

**Response `200 OK`**

```json
{
  "id": "guid",
  "name": "Du lịch hè",
  "percentage": 12,
  "status": "Active",
  "updatedAt": "ISO8601"
}
```

### `DELETE /api/v1/jars/{id}`

- **Auth**: Bearer

**Response `200 OK`**

```json
{
  "message": "Jar deleted"
}
```

**Notes**

- backend có thể soft delete hoặc archive tùy rule nội bộ
- `409 Conflict` nếu không thể xóa do đang có ràng buộc nghiệp vụ

### `POST /api/v1/jars/allocate`

- **Auth**: Bearer
- **Mục đích**: phân bổ một khoản tiền vào các hũ theo tỷ lệ hiện tại

**Request**

```json
{
  "amount": 15000000,
  "note": "Lương tháng 4"
}
```

**Response `200 OK`**

```json
{
  "allocationId": "guid",
  "totalAllocated": 15000000,
  "data": [
    {
      "jarId": "guid",
      "jarName": "Necessities",
      "amount": 8250000,
      "newBalance": 8250000
    }
  ]
}
```

### `POST /api/v1/jars/transfer`

- **Auth**: Bearer
- **Mục đích**: chuyển phân bổ giữa các hũ

**Request**

```json
{
  "fromJarId": "guid",
  "toJarId": "guid",
  "amount": 500000,
  "note": "Bù ngân sách ăn uống"
}
```

**Response `200 OK`**

```json
{
  "transferId": "guid",
  "amount": 500000,
  "fromJar": {
    "id": "guid",
    "name": "Savings",
    "newBalance": 1500000
  },
  "toJar": {
    "id": "guid",
    "name": "Necessities",
    "newBalance": 8750000
  },
  "createdAt": "ISO8601"
}
```

**Notes**

- `422 Unprocessable Entity` nếu balance không đủ
- xử lý atomic ở backend

### `GET /api/v1/jars/transfers`

- **Auth**: Bearer
- **Mục đích**: xem lịch sử chuyển tiền giữa các hũ

**Query Params**

- `page=1`
- `pageSize=20`
- `fromDate=2026-04-01`
- `toDate=2026-04-30`

**Response `200 OK`**

```json
{
  "data": [
    {
      "id": "guid",
      "amount": 500000,
      "fromJarName": "Savings",
      "toJarName": "Necessities",
      "note": "Bù ngân sách ăn uống",
      "createdAt": "ISO8601"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

### `GET /api/v1/transactions`

- **Auth**: Bearer
- **Mục đích**: xem lịch sử giao dịch có filter

**Query Params**

- `page=1`
- `pageSize=20`
- `type=Income|Expense`
- `jarId=guid`
- `categoryId=guid`
- `fromDate=2026-04-01`
- `toDate=2026-04-30`
- `keyword=cà phê`
- `sortBy=date|amount`
- `sortDir=desc`

**Response `200 OK`**

```json
{
  "data": [
    {
      "id": "guid",
      "type": "Expense",
      "amount": 55000,
      "note": "Cà phê sáng",
      "date": "ISO8601",
      "jar": {
        "id": "guid",
        "name": "Necessities"
      },
      "category": {
        "id": "guid",
        "name": "Ăn uống"
      }
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 243,
    "totalPages": 13
  }
}
```

### `POST /api/v1/transactions`

- **Auth**: Bearer
- **Mục đích**: tạo giao dịch thu hoặc chi

**Request**

```json
{
  "type": "Expense",
  "amount": 55000,
  "categoryId": "guid",
  "jarId": "guid",
  "note": "Cà phê sáng",
  "date": "ISO8601"
}
```

**Response `201 Created`**

```json
{
  "id": "guid",
  "type": "Expense",
  "amount": 55000,
  "date": "ISO8601",
  "jarBalance": 8195000,
  "message": "Transaction created"
}
```

**Notes**

- `jarId` bắt buộc cho `Expense`
- `categoryId` bắt buộc cho `Expense`
- nếu không gửi `date` thì backend dùng thời điểm hiện tại
- `422 Unprocessable Entity` nếu balance của hũ không đủ

### `PATCH /api/v1/transactions/{id}`

- **Auth**: Bearer

**Request**

```json
{
  "amount": 60000,
  "categoryId": "guid",
  "jarId": "guid",
  "note": "Cà phê sáng + bánh",
  "date": "ISO8601"
}
```

**Response `200 OK`**

```json
{
  "id": "guid",
  "amount": 60000,
  "jarBalance": 8190000,
  "updatedAt": "ISO8601"
}
```

### `DELETE /api/v1/transactions/{id}`

- **Auth**: Bearer

**Response `200 OK`**

```json
{
  "message": "Transaction deleted"
}
```

### `POST /api/v1/imports`

- **Auth**: Bearer
- **Content-Type**: `multipart/form-data`
- **Mục đích**: upload file sao kê

**Fields**

- `file`: CSV/XLS/XLSX/PDF, max 10MB
- `bankCode`: optional

**Response `202 Accepted`**

```json
{
  "id": "guid",
  "status": "Pending",
  "fileName": "sao_ke_04_2026.xlsx",
  "uploadedAt": "ISO8601"
}
```

### `GET /api/v1/imports/{id}`

- **Auth**: Bearer
- **Mục đích**: kiểm tra trạng thái import job

**Response `200 OK`**

```json
{
  "id": "guid",
  "status": "Processing",
  "progress": 80,
  "parsedCount": 41,
  "failedCount": 2,
  "errorMessage": null,
  "updatedAt": "ISO8601"
}
```

### `GET /api/v1/imports/{id}/preview`

- **Auth**: Bearer
- **Mục đích**: xem trước giao dịch parse ra trước khi lưu thật

**Response `200 OK`**

```json
{
  "id": "guid",
  "summary": {
    "totalRows": 43,
    "validRows": 41,
    "invalidRows": 2
  },
  "transactions": [
    {
      "rowIndex": 1,
      "date": "ISO8601",
      "amount": 55000,
      "type": "Expense",
      "rawDescription": "HIGHLANDS COFFEE 04/04",
      "suggestedNote": "Highlands Coffee",
      "suggestedCategoryId": "guid",
      "suggestedJarId": "guid",
      "isValid": true,
      "validationError": null
    }
  ]
}
```

### `POST /api/v1/imports/{id}/confirm`

- **Auth**: Bearer
- **Mục đích**: xác nhận những dòng sẽ lưu thành transaction thật

**Request**

```json
{
  "transactions": [
    {
      "rowIndex": 1,
      "include": true,
      "categoryId": "guid",
      "jarId": "guid",
      "note": "Highlands Coffee"
    }
  ]
}
```

**Response `200 OK`**

```json
{
  "importedCount": 39,
  "skippedCount": 2,
  "failedCount": 0,
  "message": "Import completed successfully"
}
```

## P3 — Personal Dashboard

### `GET /api/v1/dashboard`

- **Auth**: Bearer
- **Mục đích**: trả dữ liệu dashboard cho màn hình tổng quan

**Query Params**

- `period=current_month|last_month|custom`
- `fromDate=2026-04-01`
- `toDate=2026-04-30`

**Response `200 OK`**

```json
{
  "balanceSummary": {
    "totalBalance": 15000000,
    "totalIncome": 20000000,
    "totalExpense": 5000000,
    "netChange": 15000000
  },
  "jarSummary": [
    {
      "jarId": "guid",
      "jarName": "Necessities",
      "balance": 8250000,
      "spent": 1500000,
      "spentPercentage": 18.2
    }
  ],
  "categoryBreakdown": [
    {
      "categoryId": "guid",
      "categoryName": "Ăn uống",
      "totalAmount": 1200000,
      "percentage": 24.0
    }
  ],
  "recentTransactions": [
    {
      "id": "guid",
      "type": "Expense",
      "amount": 55000,
      "note": "Cà phê sáng",
      "date": "ISO8601"
    }
  ],
  "goalProgress": [
    {
      "goalId": "guid",
      "title": "Mua laptop",
      "progressPercentage": 45.0,
      "daysRemaining": 62
    }
  ]
}
```

## P4 — Limits, Notifications, Goals, Reminders

### `GET /api/v1/limits`

- **Auth**: Bearer

**Response `200 OK`**

```json
{
  "data": [
    {
      "id": "guid",
      "targetType": "Category",
      "targetId": "guid",
      "targetName": "Ăn uống",
      "limitAmount": 2000000,
      "period": "Monthly",
      "alertAtPercentage": 80,
      "currentSpent": 1600000,
      "currentPercentage": 80.0,
      "status": "Warning"
    }
  ]
}
```

### `POST /api/v1/limits`

- **Auth**: Bearer

**Request**

```json
{
  "targetType": "Category",
  "targetId": "guid",
  "limitAmount": 2000000,
  "period": "Monthly",
  "alertAtPercentage": 80
}
```

**Response `201 Created`**

```json
{
  "id": "guid",
  "targetType": "Category",
  "targetId": "guid",
  "limitAmount": 2000000,
  "period": "Monthly",
  "alertAtPercentage": 80
}
```

### `PATCH /api/v1/limits/{id}`

- **Auth**: Bearer

**Request**

```json
{
  "limitAmount": 2500000,
  "alertAtPercentage": 85
}
```

**Response `200 OK`**

```json
{
  "id": "guid",
  "limitAmount": 2500000,
  "alertAtPercentage": 85,
  "updatedAt": "ISO8601"
}
```

### `DELETE /api/v1/limits/{id}`

- **Auth**: Bearer

**Response `200 OK`**

```json
{
  "message": "Limit deleted"
}
```

### `GET /api/v1/notifications`

- **Auth**: Bearer

**Query Params**

- `type=SpendingAlert|GoalUpdate|System|Broadcast`
- `status=read|unread`
- `page=1`
- `pageSize=20`

**Response `200 OK`**

```json
{
  "data": [
    {
      "id": "guid",
      "type": "SpendingAlert",
      "title": "Gần đạt hạn mức Ăn uống",
      "body": "Bạn đã chi 80% ngân sách Ăn uống tháng này",
      "isRead": false,
      "createdAt": "ISO8601"
    }
  ],
  "unreadCount": 3,
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 15,
    "totalPages": 1
  }
}
```

### `PATCH /api/v1/notifications/status`

- **Auth**: Bearer
- **Mục đích**: đánh dấu read hoặc unread

**Request**

```json
{
  "ids": ["guid"],
  "isRead": true,
  "markAll": false
}
```

**Response `200 OK`**

```json
{
  "updatedCount": 1,
  "unreadCount": 2
}
```

### `GET /api/v1/goals`

- **Auth**: Bearer

**Response `200 OK`**

```json
{
  "data": [
    {
      "id": "guid",
      "title": "Mua laptop gaming",
      "targetAmount": 25000000,
      "savedAmount": 11250000,
      "progressPercentage": 45.0,
      "dueDate": "2026-06-30",
      "status": "Active",
      "suggestedMonthlyContribution": 3450000
    }
  ]
}
```

### `GET /api/v1/goals/{id}`

- **Auth**: Bearer
- **Mục đích**: lấy chi tiết mục tiêu và tiến độ

**Response `200 OK`**

```json
{
  "id": "guid",
  "title": "Mua laptop gaming",
  "targetAmount": 25000000,
  "savedAmount": 11250000,
  "progressPercentage": 45.0,
  "dueDate": "2026-06-30",
  "daysRemaining": 71,
  "status": "Active",
  "suggestedMonthlyContribution": 3450000,
  "linkedJarId": "guid | null",
  "recentContributions": [
    {
      "id": "guid",
      "amount": 1000000,
      "createdAt": "ISO8601"
    }
  ]
}
```

### `POST /api/v1/goals`

- **Auth**: Bearer

**Request**

```json
{
  "title": "Mua laptop gaming",
  "targetAmount": 25000000,
  "dueDate": "2026-06-30",
  "linkedJarId": "guid | null",
  "note": "Mua trong hè"
}
```

**Response `201 Created`**

```json
{
  "id": "guid",
  "title": "Mua laptop gaming",
  "targetAmount": 25000000,
  "savedAmount": 0,
  "progressPercentage": 0,
  "status": "Active"
}
```

### `PATCH /api/v1/goals/{id}`

- **Auth**: Bearer

**Request**

```json
{
  "title": "Mua laptop học tập",
  "targetAmount": 22000000,
  "dueDate": "2026-07-15",
  "linkedJarId": "guid | null",
  "note": "string"
}
```

**Response `200 OK`**

```json
{
  "id": "guid",
  "title": "Mua laptop học tập",
  "targetAmount": 22000000,
  "dueDate": "2026-07-15",
  "updatedAt": "ISO8601"
}
```

### `DELETE /api/v1/goals/{id}`

- **Auth**: Bearer

**Response `200 OK`**

```json
{
  "message": "Goal deleted"
}
```

### `POST /api/v1/goals/{id}/contributions`

- **Auth**: Bearer
- **Mục đích**: đóng góp tiền vào mục tiêu

**Request**

```json
{
  "amount": 1000000,
  "sourceJarId": "guid | null",
  "note": "Tiết kiệm tuần này"
}
```

**Response `200 OK`**

```json
{
  "contributionId": "guid",
  "goalId": "guid",
  "newSavedAmount": 12250000,
  "newProgressPercentage": 49.0,
  "sourceJarBalance": 7250000,
  "isCompleted": false
}
```

### `GET /api/v1/reminders`

- **Auth**: Bearer

**Response `200 OK`**

```json
{
  "data": [
    {
      "id": "guid",
      "title": "Tiền điện nước",
      "amount": 500000,
      "frequency": "Monthly",
      "nextDueDate": "ISO8601",
      "status": "Active"
    }
  ]
}
```

### `POST /api/v1/reminders`

- **Auth**: Bearer

**Request**

```json
{
  "title": "Tiền điện nước",
  "amount": 500000,
  "frequency": "Monthly",
  "dayOfMonth": 25,
  "startDate": "2026-04-25",
  "categoryId": "guid | null",
  "note": "string"
}
```

**Response `201 Created`**

```json
{
  "id": "guid",
  "title": "Tiền điện nước",
  "frequency": "Monthly",
  "nextDueDate": "ISO8601",
  "status": "Active"
}
```

### `PATCH /api/v1/reminders/{id}`

- **Auth**: Bearer

**Request**

```json
{
  "title": "Tiền điện + nước",
  "amount": 600000,
  "frequency": "Monthly",
  "dayOfMonth": 26,
  "note": "string"
}
```

**Response `200 OK`**

```json
{
  "id": "guid",
  "title": "Tiền điện + nước",
  "nextDueDate": "ISO8601",
  "updatedAt": "ISO8601"
}
```

### `DELETE /api/v1/reminders/{id}`

- **Auth**: Bearer

**Response `200 OK`**

```json
{
  "message": "Reminder deleted"
}
```

## P6 — Admin User Management & System Notifications

### `GET /api/v1/admin/users`

- **Auth**: Admin

**Query Params**

- `page=1`
- `pageSize=20`
- `keyword=nguyen`
- `status=Active|Banned`
- `sortBy=createdAt|lastLogin`
- `sortDir=desc`

**Response `200 OK`**

```json
{
  "data": [
    {
      "id": "guid",
      "username": "string",
      "fullName": "string",
      "email": "string",
      "status": "Active",
      "isOnboardingCompleted": true,
      "jarCount": 6,
      "transactionCount": 143,
      "lastLoginAt": "ISO8601",
      "createdAt": "ISO8601"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 1482,
    "totalPages": 75
  }
}
```

### `GET /api/v1/admin/users/{id}`

- **Auth**: Admin

**Response `200 OK`**

```json
{
  "id": "guid",
  "username": "string",
  "fullName": "string",
  "email": "string",
  "status": "Active",
  "isOnboardingCompleted": true,
  "onboardingSummary": {
    "monthlyIncome": 15000000,
    "budgetMethod": "SixJars"
  },
  "jarCount": 6,
  "totalBalance": 15000000,
  "transactionCount": 143,
  "goalCount": 2,
  "lastLoginAt": "ISO8601",
  "createdAt": "ISO8601"
}
```

### `PATCH /api/v1/admin/users/{id}/status`

- **Auth**: Admin

**Request**

```json
{
  "status": "Banned",
  "reason": "Spam or abuse"
}
```

**Response `200 OK`**

```json
{
  "id": "guid",
  "status": "Banned",
  "reason": "Spam or abuse",
  "updatedAt": "ISO8601"
}
```

**Notes**

- tự động ghi `audit_logs`

### `GET /api/v1/admin/categories`

- **Auth**: Admin
- **Mục đích**: lấy danh sách default categories để admin quản lý

**Response `200 OK`**

```json
{
  "data": [
    {
      "id": "guid",
      "name": "Ăn uống",
      "icon": "food",
      "color": "#FF6B6B",
      "order": 1,
      "isActive": true
    }
  ]
}
```

### `POST /api/v1/admin/categories`

- **Auth**: Admin

**Request**

```json
{
  "name": "Ăn uống",
  "icon": "food",
  "color": "#FF6B6B",
  "order": 1
}
```

**Response `201 Created`**

```json
{
  "id": "guid",
  "name": "Ăn uống",
  "icon": "food",
  "color": "#FF6B6B",
  "order": 1,
  "isActive": true
}
```

### `PATCH /api/v1/admin/categories/{id}`

- **Auth**: Admin

**Request**

```json
{
  "name": "Ăn uống",
  "icon": "food",
  "color": "#FF6B6B",
  "order": 1,
  "isActive": true
}
```

**Response `200 OK`**

```json
{
  "id": "guid",
  "name": "Ăn uống",
  "order": 1,
  "isActive": true,
  "updatedAt": "ISO8601"
}
```

### `DELETE /api/v1/admin/categories/{id}`

- **Auth**: Admin

**Response `200 OK`**

```json
{
  "message": "Category deleted"
}
```

**Notes**

- nên là soft delete

### `POST /api/v1/admin/broadcasts`

- **Auth**: Admin
- **Mục đích**: tạo và gửi broadcast notification

**Request**

```json
{
  "title": "Bảo trì hệ thống",
  "body": "Hệ thống sẽ bảo trì lúc 23:00",
  "targetAudience": "All",
  "scheduledAt": null
}
```

**Response `202 Accepted`**

```json
{
  "id": "guid",
  "status": "Queued",
  "targetCount": 1482,
  "scheduledAt": null
}
```

### `GET /api/v1/admin/broadcasts`

- **Auth**: Admin
- **Mục đích**: xem lịch sử broadcast đã gửi hoặc đang chờ gửi

**Query Params**

- `page=1`
- `pageSize=20`
- `status=Queued|Sent`

**Response `200 OK`**

```json
{
  "data": [
    {
      "id": "guid",
      "title": "Bảo trì hệ thống",
      "targetAudience": "All",
      "targetCount": 1482,
      "status": "Sent",
      "createdAt": "ISO8601"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 5,
    "totalPages": 1
  }
}
```

### `GET /api/v1/admin/dashboard`

- **Auth**: Admin

**Query Params**

- `from=2026-04-01`
- `to=2026-04-30`

**Response `200 OK`**

```json
{
  "totalUsers": 1482,
  "newUsers": 134,
  "activeUsers": 892,
  "totalTransactions": 28400,
  "totalTransactionVolume": 4200000000,
  "bannedUsers": 12,
  "recentActivities": [
    {
      "type": "BroadcastSent",
      "label": "Gửi broadcast bảo trì hệ thống",
      "createdAt": "ISO8601"
    }
  ],
  "period": {
    "from": "ISO8601",
    "to": "ISO8601"
  }
}
```

### `GET /api/v1/admin/audit-logs`

- **Auth**: Admin

**Query Params**

- `adminId=guid`
- `actionType=Login|BanUser|CategoryChange|BroadcastSend`
- `entityType=User|Category|Broadcast|AiSetting`
- `fromDate=2026-04-01`
- `toDate=2026-04-30`
- `page=1`
- `pageSize=50`

**Response `200 OK`**

```json
{
  "data": [
    {
      "id": "guid",
      "adminUsername": "string",
      "actionType": "BanUser",
      "entityType": "User",
      "description": "Banned user: nguyen123",
      "createdAt": "ISO8601"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 50,
    "totalCount": 320,
    "totalPages": 7
  }
}
```

### `GET /api/v1/admin/ai-settings`

- **Auth**: Admin

**Response `200 OK`**

```json
{
  "modelName": "gpt-4o-mini",
  "systemPrompt": "string",
  "temperature": 0.7,
  "maxTokens": 1000,
  "isEnabled": true,
  "apiKeyMasked": "sk-...xxxx",
  "lastUpdatedAt": "ISO8601"
}
```

### `PATCH /api/v1/admin/ai-settings`

- **Auth**: Admin

**Request**

```json
{
  "modelName": "gpt-4o-mini",
  "systemPrompt": "string",
  "temperature": 0.7,
  "maxTokens": 1000,
  "apiKey": "string | null",
  "isEnabled": true
}
```

**Response `200 OK`**

```json
{
  "modelName": "gpt-4o-mini",
  "isEnabled": true,
  "lastUpdatedAt": "ISO8601"
}
```

## 6. Ghi chú triển khai cho BE và FE

### Dành cho frontend

- Ưu tiên gọi các API theo đúng use case màn hình, không tự ghép logic nghiệp vụ ở FE.
- Không tự tính `newBalance`, `progressPercentage`, `limit status`; FE chỉ hiển thị dữ liệu backend trả về.
- Với write APIs, FE chỉ cần giữ state tối thiểu và refresh lại màn hình khi cần.

### Dành cho backend

- Không trả thẳng internal model hoặc database entity ra ngoài.
- Write APIs nên map sang response DTO gọn.
- Các nghiệp vụ tiền tệ phải xử lý atomic ở backend.
- Audit log, notification trigger, limit evaluation, goal progress calculation là trách nhiệm backend.

## 7. Tổng kết

Bản `apis.md` này đã được viết lại theo hướng:

- **client-first**
- **bám core user story**
- **giảm payload thừa**
- **bổ sung các core API còn thiếu ở bản cũ**

Nếu team bám theo bản này thì FE sẽ dễ tích hợp hơn, còn BE vẫn có đủ khoảng trống để tự tổ chức logic nội bộ theo [backend_internal_model.md](/d:/Coding/Project/prj_pied/Personal_Finance_App_Be/docs/backend_internal_model.md).
