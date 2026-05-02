# Personal Finance Manager - Core API Contracts (MVP)

Tài liệu này là bản API contract cho frontend theo hướng client-first, bám theo core scope trong 1 tháng.

Nguồn đồng bộ:

- docs/overview.md
- docs/user story.md
- docs/finjar_schema.sql

## 1. Thay đổi quan trọng

1. Hệ thống đã chuyển sang access token only.
2. Bỏ refresh token khỏi contract API core:

- Không còn endpoint `POST /api/v1/auth/refresh`.
- Không trả `refreshToken` trong login response.
- Logout là hành vi kết thúc phiên ở FE (xóa token local) và ghi nhật ký ở BE nếu cần.

3. Response được tối giản hóa: loại các field backend-internal không cần cho UI.

## 2. Nguyên tắc thiết kế

### 2.1. Client-first

- Request chỉ chứa dữ liệu user thực sự nhập/chọn.
- Response chỉ giữ field FE cần để render, sort, filter, hoặc cập nhật state.

### 2.2. Thin write, rich read

- Write APIs (POST/PATCH/DELETE): response gọn, ưu tiên `id`, một số field cần cập nhật UI và `message`.
- Read APIs (GET): trả đầy đủ để render màn hình.

### 2.3. Không đưa internal processing ra ngoài

- Không trả các field mang tính nội bộ như `updatedAt`, `createdAt` ở các endpoint ghi dữ liệu, nếu FE không sử dụng.
- Không trả dữ liệu phục vụ chỉ cho logging, tracing, transaction internals.

## 3. Convention chung

### 3.1. Base URL

`/api/v1`

### 3.2. Auth

- Public: không cần token.
- Bearer: user access token.
- Admin: access token role `Admin`.

### 3.3. Money và thời gian

- Amount/balance/limit/target sử dụng decimal.
- Time trả về theo ISO8601.

### 3.4. Pagination

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

## 4. Khi nào giữ field thời gian trong response

Không phải tất cả field thời gian đều vô ích. Các field dưới đây được giữ vì có giá trị UI rõ ràng:

- `transactions.date`: FE cần hiển thị timeline, sắp xếp, filter theo ngày.
- `notifications.occurredAt`: FE cần hiển thị "vừa xong", "2 giờ trước".
- `goals.dueDate`, `goals.daysRemaining`: FE cần hiển thị mức độ gấp của mục tiêu.
- `reminders.nextDueDate`: FE cần hiển thị lịch nhắc sắp tới.
- `admin.users.lastLoginAt`: FE admin cần hỗ trợ moderation/hỗ trợ tài khoản.
- `admin.audit-logs.createdAt`: trường bắt buộc cho màn audit timeline.

Những field như `createdAt` trong response tạo category/jar/limit thường không cần cho FE ngay lập tức nên đã loại bỏ.

## 5. API Contracts theo nhóm flow

## P0 - Authentication & Admin Foundation

### `POST /api/v1/auth/register`

- Auth: Public

Request

```json
{
  "username": "string",
  "email": "string",
  "password": "string",
  "firstName": "string",
  "lastName": "string"
}
```

Response `201 Created`

```json
{
  "accessToken": "string",
  "tokenType": "Bearer",
  "expiresIn": 900,
  "user": {
    "id": "guid",
    "username": "string",
    "firstName": "string",
    "lastName": "string",
    "email": "string",
    "phone": "string | null",
    "role": "User | Admin",
    "isOnboardingCompleted": false
  }
}
```

Notes

- `409 Conflict` nếu username hoặc email đã tồn tại.
- Password hash dùng BCrypt.
- Register thành công trả token ngay để FE auto-login vào app

### `POST /api/v1/auth/login`

- Auth: Public

Request

```json
{
  "username": "string",
  "password": "string"
}
```

Response `200 OK`

```json
{
  "accessToken": "string",
  "tokenType": "Bearer",
  "expiresIn": 900,
  "user": {
    "id": "guid",
    "username": "string",
    "firstName": "string",
    "lastName": "string",
    "email": "string",
    "phone": "string | null",
    "role": "User | Admin",
    "isOnboardingCompleted": false
  }
}
```

Notes

- `401 Unauthorized` nếu sai credential.
- `403 Forbidden` nếu account bị banned.

### `POST /api/v1/auth/logout`

- Auth: Bearer

Request

```json
{}
```

Response `200 OK`

```json
{
  "message": "Logged out successfully"
}
```

### `POST /api/v1/admin/auth/login`

- Auth: Public

Request

```json
{
  "username": "string",
  "password": "string"
}
```

Response `200 OK`

```json
{
  "accessToken": "string",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "admin": {
    "id": "guid",
    "username": "string",
    "role": "Admin"
  }
}
```

**Notes**

- tự động ghi audit log
- `403 Forbidden` nếu không phải role admin

## P1 — Onboarding, Profile, Financial Accounts, Category

### `GET /api/v1/user/me`

- Auth: Bearer

Response `200 OK`

```json
{
  "id": "guid",
  "username": "string",
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "phone": "string | null",
  "avatarUrl": "string | null",
  "preferredCurrency": "VND",
  "isOnboardingCompleted": true
}
```

### `PATCH /api/v1/user/me`

- Auth: Bearer

Request

```json
{
  "firstName": "string",
  "lastName": "string",
  "phone": "string | null",
  "avatarUrl": "string | null"
}
```

Response `200 OK`

```json
{
  "id": "guid",
  "firstName": "string",
  "lastName": "string",
  "phone": "string | null",
  "avatarUrl": "string | null"
}
```

### `GET /api/v1/user/me/setup`

- Auth: Bearer

Response `200 OK`

```json
{
  "isOnboardingCompleted": true,
  "monthlyIncome": 15000000,
  "budgetMethod": "SixJars",
  "financialAccountCount": 2,
  "defaultFinancialAccountId": "guid | null",
  "jarCount": 6,
  "financialAccountCount": 1,
  "limitCount": 2,
  "activeGoalCount": 1
}
```

### `GET /api/v1/financial-accounts`

- **Auth**: Bearer
- **Mục đích**: lấy danh sách nguồn tiền user đang theo dõi, bao gồm tiền mặt thủ công và tài khoản liên kết nếu có

**Response `200 OK`**

```json
{
  "data": [
    {
      "id": "guid",
      "name": "Tiền mặt",
      "accountType": "Cash",
      "connectionMode": "Manual",
      "providerName": null,
      "maskedAccountNumber": null,
      "currency": "VND",
      "currentBalance": 3000000,
      "availableBalance": null,
      "balanceAsOf": "ISO8601 | null",
      "syncStatus": "NeverSynced",
      "isDefault": true,
      "isActive": true
    }
  ]
}
```

### `POST /api/v1/financial-accounts`

- **Auth**: Bearer
- **Mục đích**: tạo nguồn tiền thủ công, ví dụ tiền mặt hoặc một tài khoản user muốn tự theo dõi

**Request**

```json
{
  "name": "Tiền mặt",
  "accountType": "Cash",
  "currentBalance": 3000000,
  "currency": "VND",
  "isDefault": true
}
```

**Response `201 Created`**

```json
{
  "id": "guid",
  "name": "Tiền mặt",
  "accountType": "Cash",
  "connectionMode": "Manual",
  "currentBalance": 3000000,
  "currency": "VND",
  "isDefault": true,
  "isActive": true
}
```

**Notes**

- API này chỉ tạo record `financial_accounts` ở `connectionMode = Manual`.
- Record `LinkedApi` được tạo bởi flow bank-link/provider ở phase sau.
- `financial_accounts` là nguồn tiền để theo dõi, không phải ví do FinJar phát hành.

### `PATCH /api/v1/financial-accounts/{id}`

- **Auth**: Bearer

**Request**

```json
{
  "name": "Tiền mặt chính",
  "currentBalance": 3500000,
  "isDefault": true
}
```

**Response `200 OK`**

```json
{
  "id": "guid",
  "name": "Tiền mặt chính",
  "currentBalance": 3500000,
  "isDefault": true,
  "updatedAt": "ISO8601"
}
```

**Notes**

- Chỉ cho user cập nhật nguồn tiền thuộc sở hữu của chính họ.
- Với `connectionMode = LinkedApi`, backend có thể giới hạn field được sửa thủ công để tránh lệch dữ liệu sync.

### `DELETE /api/v1/financial-accounts/{id}`

- **Auth**: Bearer
- **Mục đích**: ngưng theo dõi một nguồn tiền

**Response `200 OK`**

```json
{
  "message": "Financial account deactivated"
}
```

**Notes**

- Nên map thành `is_active = false`, không hard delete nếu đã có transaction/import liên quan.
- `409 Conflict` nếu đây là nguồn tiền duy nhất hoặc đang bị ràng buộc bởi nghiệp vụ chưa xử lý được.

### `POST /api/v1/users/onboarding`

- Auth: Bearer

Request

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

Response `200 OK`

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
  "defaultFinancialAccount": {
    "name": "Tiền mặt",
    "accountType": "Cash"
  }
}
```

### `GET /api/v1/financial-accounts`

- Auth: Bearer

Response `200 OK`

```json
{
  "data": [
    {
      "id": "guid",
      "name": "Tiền mặt",
      "accountType": "Cash",
      "connectionMode": "Manual",
      "currentBalance": 5000000,
      "isDefault": true,
      "isActive": true
    }
  ]
}
```

### `POST /api/v1/financial-accounts`

- Auth: Bearer

Request

```json
{
  "name": "Tiền mặt",
  "accountType": "Cash",
  "initialBalance": 5000000,
  "isDefault": true
}
```

Response `201 Created`

```json
{
  "id": "guid",
  "name": "Tiền mặt",
  "accountType": "Cash",
  "connectionMode": "Manual",
  "currentBalance": 5000000,
  "isDefault": true,
  "isActive": true
}
```

### `PATCH /api/v1/financial-accounts/{id}`

- Auth: Bearer

Request

```json
{
  "name": "Tiền mặt gia đình",
  "isDefault": true,
  "isActive": true
}
```

Response `200 OK`

```json
{
  "id": "guid",
  "name": "Tiền mặt gia đình",
  "isDefault": true,
  "isActive": true
}
```

### `PATCH /api/v1/financial-accounts/{id}/balance`

- Auth: Bearer

Request

```json
{
  "newBalance": 6500000,
  "note": "Điều chỉnh số dư đầu kỳ"
}
```

Response `200 OK`

```json
{
  "id": "guid",
  "currentBalance": 6500000
}
```

### `DELETE /api/v1/financial-accounts/{id}`

- Auth: Bearer

Response `200 OK`

```json
{
  "message": "Financial account deleted"
}
```

### `GET /api/v1/categories`

- Auth: Bearer

Response `200 OK`

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

- Auth: Bearer

Request

```json
{
  "name": "Thú cưng",
  "icon": "pet",
  "color": "#A8DADC"
}
```

Response `201 Created`

```json
{
  "id": "guid",
  "name": "Thú cưng",
  "icon": "pet",
  "color": "#A8DADC"
}
```

### `PATCH /api/v1/categories/{id}`

- Auth: Bearer

Request

```json
{
  "name": "Thú cưng",
  "icon": "pet",
  "color": "#A8DADC"
}
```

Response `200 OK`

```json
{
  "id": "guid",
  "name": "Thú cưng",
  "icon": "pet",
  "color": "#A8DADC"
}
```

### `DELETE /api/v1/categories/{id}`

- Auth: Bearer

Response `200 OK`

```json
{
  "message": "Category deleted"
}
```

## P2 - Jars, Transactions, Import Statement

### `GET /api/v1/jars`

- Auth: Bearer

Response `200 OK`

```json
{
  "methodType": "SixJars",
  "totalJarBalance": 12000000,
  "unallocatedBalance": 3000000,
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
- **Mục đích**: tạo bộ hũ ban đầu sau onboarding, chưa bắt buộc phân bổ số dư ngay

Request

```json
{
  "methodType": "SixJars",
  "initialBalance": 15000000,
  "sourceFinancialAccountId": "guid | null",
  "customJars": []
}
```

Response `201 Created`

```json
{
  "methodType": "SixJars",
  "data": [
    {
      "id": "guid",
      "name": "Necessities",
      "percentage": 55,
      "balance": 0
    }
  ]
}
```

**Notes**

- `409 Conflict` nếu user đã setup jars trước đó
- nếu `methodType = Custom` thì tổng `percentage` của `customJars` phải bằng `100`
- số dư gốc nằm ở `financial_accounts`; nếu user muốn phân bổ tiền vào hũ sau setup thì gọi `POST /api/v1/jars/allocate`

### `POST /api/v1/jars`

- Auth: Bearer

Request

```json
{
  "name": "Du lịch",
  "percentage": 10,
  "color": "#2A9D8F",
  "icon": "plane"
}
```

Response `201 Created`

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

- Auth: Bearer

Request

```json
{
  "name": "Du lịch hè",
  "percentage": 12,
  "color": "#2A9D8F",
  "icon": "plane"
}
```

Response `200 OK`

```json
{
  "id": "guid",
  "name": "Du lịch hè",
  "percentage": 12,
  "color": "#2A9D8F",
  "icon": "plane",
  "status": "Active"
}
```

### `DELETE /api/v1/jars/{id}`

- Auth: Bearer

Response `200 OK`

```json
{
  "message": "Jar deleted"
}
```

### `POST /api/v1/jars/allocate`

- **Auth**: Bearer
- **Mục đích**: phân bổ ngân sách từ một nguồn tiền đang theo dõi vào các hũ theo tỷ lệ hiện tại

Request

```json
{
  "sourceFinancialAccountId": "guid | null",
  "amount": 15000000,
  "sourceFinancialAccountId": "guid | null",
  "note": "Lương tháng 4"
}
```

Response `200 OK`

```json
{
  "allocationId": "guid",
  "sourceFinancialAccountId": "guid | null",
  "totalAllocated": 15000000,
  "affectedJarCount": 6
}
```

**Notes**

- `sourceFinancialAccountId` map vào `jar_allocations.source_financial_account_id`.
- Field này chỉ giúp biết khoản phân bổ được quy chiếu từ nguồn tiền nào; FinJar không rút/chuyển tiền thật khỏi ngân hàng.
- Nếu không gửi `sourceFinancialAccountId`, backend có thể dùng nguồn mặc định hoặc chỉ ghi nhận allocation không gắn nguồn cụ thể tùy rule nội bộ.

### `POST /api/v1/jars/transfer`

- **Auth**: Bearer
- **Mục đích**: chuyển ngân sách nội bộ giữa các hũ

Request

```json
{
  "fromJarId": "guid",
  "toJarId": "guid",
  "amount": 500000,
  "note": "Bù ngân sách ăn uống"
}
```

Response `200 OK`

```json
{
  "transferId": "guid",
  "amount": 500000,
  "fromJarId": "guid",
  "toJarId": "guid"
}
```

### `GET /api/v1/jars/transfers`

- Auth: Bearer

Query Params

- `page=1`
- `pageSize=20`
- `fromDate=2026-04-01`
- `toDate=2026-04-30`

Response `200 OK`

```json
{
  "data": [
    {
      "id": "guid",
      "amount": 500000,
      "fromJarName": "Savings",
      "toJarName": "Necessities",
      "note": "Bù ngân sách ăn uống",
      "date": "ISO8601"
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

- Auth: Bearer

Query Params

- `page=1`
- `pageSize=20`
- `financialAccountId=guid`
- `type=Income|Expense`
- `jarId=guid`
- `categoryId=guid`
- `fromDate=2026-04-01`
- `toDate=2026-04-30`
- `keyword=cà phê`
- `sortBy=date|amount`
- `sortDir=desc`

Response `200 OK`

```json
{
  "data": [
    {
      "id": "guid",
      "type": "Expense",
      "amount": 55000,
      "note": "Cà phê sáng",
      "date": "ISO8601",
      "financialAccount": {
        "id": "guid",
        "name": "Tiền mặt"
      },
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

- Auth: Bearer

Request

```json
{
  "financialAccountId": "guid",
  "type": "Expense",
  "amount": 55000,
  "categoryId": "guid",
  "jarId": "guid",
  "note": "Cà phê sáng",
  "date": "ISO8601"
}
```

Response `201 Created`

```json
{
  "id": "guid",
  "financialAccountId": "guid",
  "type": "Expense",
  "amount": 55000,
  "date": "ISO8601"
}
```

### `PATCH /api/v1/transactions/{id}`

- Auth: Bearer

Request

```json
{
  "financialAccountId": "guid",
  "amount": 60000,
  "categoryId": "guid",
  "jarId": "guid",
  "note": "Cà phê sáng + bánh",
  "date": "ISO8601"
}
```

Response `200 OK`

```json
{
  "id": "guid",
  "type": "Expense",
  "amount": 60000,
  "date": "ISO8601"
}
```

**Notes**

- Nếu đổi `financialAccountId`, backend phải đảo tác động số dư ở nguồn cũ và áp dụng lại ở nguồn mới trong cùng transaction.

### `DELETE /api/v1/transactions/{id}`

- Auth: Bearer

Response `200 OK`

```json
{
  "message": "Transaction deleted"
}
```

### `POST /api/v1/imports`

- Auth: Bearer
- Content-Type: `multipart/form-data`

Fields

- `file`: CSV/XLS/XLSX/PDF, max 10MB
- `financialAccountId`: required
- `bankCode`: optional

Response `202 Accepted`

```json
{
  "id": "guid",
  "financialAccountId": "guid",
  "status": "Pending",
  "fileName": "sao_ke_04_2026.xlsx"
}
```

**Notes**

- `financialAccountId` map vào `import_jobs.financial_account_id`.
- Các transaction được tạo sau khi confirm import sẽ kế thừa `financialAccountId` này.
- `bankCode` chỉ hỗ trợ parser; khóa nghiệp vụ chính vẫn là `financialAccountId`.

### `GET /api/v1/imports/{id}`

- Auth: Bearer

Response `200 OK`

```json
{
  "id": "guid",
  "financialAccountId": "guid",
  "status": "Processing",
  "progress": 80,
  "parsedCount": 41,
  "failedCount": 2,
  "errorMessage": null
}
```

### `GET /api/v1/imports/{id}/preview`

- Auth: Bearer

Response `200 OK`

```json
{
  "id": "guid",
  "financialAccountId": "guid",
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

- Auth: Bearer

Request

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

Response `200 OK`

```json
{
  "importedCount": 39,
  "skippedCount": 2,
  "failedCount": 0
}
```

## P3 - Personal Dashboard

### `GET /api/v1/dashboard`

- Auth: Bearer

Query Params

- `period=current_month|last_month|custom`
- `fromDate=2026-04-01`
- `toDate=2026-04-30`

Response `200 OK`

```json
{
  "balanceSummary": {
    "totalBalance": 15000000,
    "allocatedBalance": 12000000,
    "unallocatedBalance": 3000000,
    "totalIncome": 20000000,
    "totalExpense": 5000000,
    "netChange": 15000000
  },
  "financialAccounts": [
    {
      "id": "guid",
      "name": "Tiền mặt",
      "currentBalance": 3000000,
      "isDefault": true
    }
  ],
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

## P4 - Limits, Notifications, Goals, Reminders

### `GET /api/v1/limits`

- Auth: Bearer

Response `200 OK`

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

- Auth: Bearer

Request

```json
{
  "targetType": "Category",
  "targetId": "guid",
  "limitAmount": 2000000,
  "period": "Monthly",
  "alertAtPercentage": 80
}
```

Response `201 Created`

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

- Auth: Bearer

Request

```json
{
  "limitAmount": 2500000,
  "alertAtPercentage": 85
}
```

Response `200 OK`

```json
{
  "id": "guid",
  "limitAmount": 2500000,
  "alertAtPercentage": 85
}
```

### `DELETE /api/v1/limits/{id}`

- Auth: Bearer

Response `200 OK`

```json
{
  "message": "Limit deleted"
}
```

### `GET /api/v1/notifications`

- Auth: Bearer

Query Params

- `type=SpendingAlert|GoalUpdate|Reminder|System|Broadcast`
- `status=read|unread`
- `page=1`
- `pageSize=20`

Response `200 OK`

```json
{
  "data": [
    {
      "id": "guid",
      "type": "SpendingAlert",
      "title": "Gần đạt hạn mức Ăn uống",
      "body": "Bạn đã chi 80% ngân sách Ăn uống tháng này",
      "isRead": false,
      "occurredAt": "ISO8601"
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

- Auth: Bearer

Request

```json
{
  "ids": ["guid"],
  "isRead": true,
  "markAll": false
}
```

Response `200 OK`

```json
{
  "updatedCount": 1,
  "unreadCount": 2
}
```

### `GET /api/v1/goals`

- Auth: Bearer

Response `200 OK`

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

- Auth: Bearer

Response `200 OK`

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
      "date": "ISO8601"
    }
  ]
}
```

### `POST /api/v1/goals`

- Auth: Bearer

Request

```json
{
  "title": "Mua laptop gaming",
  "targetAmount": 25000000,
  "dueDate": "2026-06-30",
  "linkedJarId": "guid | null",
  "note": "Mua trong hè"
}
```

Response `201 Created`

```json
{
  "id": "guid",
  "title": "Mua laptop gaming",
  "targetAmount": 25000000,
  "savedAmount": 0,
  "progressPercentage": 0,
  "status": "Active",
  "dueDate": "2026-06-30"
}
```

### `PATCH /api/v1/goals/{id}`

- Auth: Bearer

Request

```json
{
  "title": "Mua laptop học tập",
  "targetAmount": 22000000,
  "dueDate": "2026-07-15",
  "linkedJarId": "guid | null",
  "note": "string"
}
```

Response `200 OK`

```json
{
  "id": "guid",
  "title": "Mua laptop học tập",
  "targetAmount": 22000000,
  "dueDate": "2026-07-15",
  "status": "Active"
}
```

### `DELETE /api/v1/goals/{id}`

- Auth: Bearer

Response `200 OK`

```json
{
  "message": "Goal deleted"
}
```

### `POST /api/v1/goals/{id}/contributions`

- Auth: Bearer

Request

```json
{
  "amount": 1000000,
  "sourceJarId": "guid | null",
  "sourceFinancialAccountId": "guid | null",
  "note": "Tiết kiệm tuần này"
}
```

Response `200 OK`

```json
{
  "contributionId": "guid",
  "goalId": "guid",
  "newSavedAmount": 12250000,
  "newProgressPercentage": 49.0,
  "remainingAmount": 12750000,
  "isCompleted": false
}
```

**Notes**

- Chỉ gửi một trong hai field: `sourceJarId` hoặc `sourceFinancialAccountId`.
- `sourceFinancialAccountId` map vào `goal_contributions.source_financial_account_id`.
- Đây là ghi nhận/phân loại nguồn đóng góp trong app, không phải lệnh chuyển tiền thật vào goal.

### `GET /api/v1/reminders`

- Auth: Bearer

Response `200 OK`

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

- Auth: Bearer

Request

```json
{
  "title": "Tiền điện nước",
  "amount": 500000,
  "frequency": "Monthly",
  "dayOfMonth": 25,
  "startDate": "2026-04-25",
  "categoryId": "guid | null",
  "notifyDaysBefore": 1,
  "note": "string"
}
```

Response `201 Created`

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

- Auth: Bearer

Request

```json
{
  "title": "Tiền điện + nước",
  "amount": 600000,
  "frequency": "Monthly",
  "dayOfMonth": 26,
  "status": "Active",
  "notifyDaysBefore": 1,
  "note": "string"
}
```

Response `200 OK`

```json
{
  "id": "guid",
  "title": "Tiền điện + nước",
  "frequency": "Monthly",
  "nextDueDate": "ISO8601",
  "status": "Active"
}
```

### `DELETE /api/v1/reminders/{id}`

- Auth: Bearer

Response `200 OK`

```json
{
  "message": "Reminder deleted"
}
```

## P6 - Admin User Management & System Notifications

### `GET /api/v1/admin/users`

- Auth: Admin

Query Params

- `page=1`
- `pageSize=20`
- `keyword=nguyen`
- `status=Active|Banned`
- `sortBy=lastLogin|username`
- `sortDir=desc`

Response `200 OK`

```json
{
  "data": [
    {
      "id": "guid",
      "username": "string",
      "firstName": "string",
      "lastName": "string",
      "email": "string",
      "status": "Active",
      "isOnboardingCompleted": true,
      "jarCount": 6,
      "transactionCount": 143,
      "lastLoginAt": "ISO8601"
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

- Auth: Admin

Response `200 OK`

```json
{
  "id": "guid",
  "username": "string",
  "firstName": "string",
  "lastName": "string",
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
  "lastLoginAt": "ISO8601"
}
```

### `PATCH /api/v1/admin/users/{id}/status`

- Auth: Admin

Request

```json
{
  "status": "Banned",
  "reason": "Spam or abuse"
}
```

Response `200 OK`

```json
{
  "id": "guid",
  "status": "Banned",
  "reason": "Spam or abuse"
}
```

### `GET /api/v1/admin/categories`

- Auth: Admin

Response `200 OK`

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

- Auth: Admin

Request

```json
{
  "name": "Ăn uống",
  "icon": "food",
  "color": "#FF6B6B",
  "order": 1
}
```

Response `201 Created`

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

- Auth: Admin

Request

```json
{
  "name": "Ăn uống",
  "icon": "food",
  "color": "#FF6B6B",
  "order": 1,
  "isActive": true
}
```

Response `200 OK`

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

### `DELETE /api/v1/admin/categories/{id}`

- Auth: Admin

Response `200 OK`

```json
{
  "message": "Category deleted"
}
```

### `POST /api/v1/admin/broadcasts`

- Auth: Admin

Request

```json
{
  "title": "Bảo trì hệ thống",
  "body": "Hệ thống sẽ bảo trì lúc 23:00",
  "targetAudience": "All",
  "scheduledAt": null
}
```

Response `202 Accepted`

```json
{
  "id": "guid",
  "status": "Queued",
  "targetCount": 1482,
  "scheduledAt": null
}
```

### `GET /api/v1/admin/broadcasts`

- Auth: Admin

Query Params

- `page=1`
- `pageSize=20`
- `status=Queued|Sent|Failed|Cancelled`

Response `200 OK`

```json
{
  "data": [
    {
      "id": "guid",
      "title": "Bảo trì hệ thống",
      "targetAudience": "All",
      "targetCount": 1482,
      "status": "Sent",
      "sentAt": "ISO8601"
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

- Auth: Admin

Query Params

- `timeframe` (optional, enum: `day | month | year`, default: `day`)

Request Rules

- Nếu không truyền `timeframe`, backend tự dùng `day`.
- Nếu `timeframe` không thuộc enum hợp lệ, trả `400 Bad Request`.
- Endpoint không nhận request body.

Example Request

```http
GET /api/v1/admin/dashboard?timeframe=day
Authorization: Bearer <admin_access_token>
```

Response `200 OK`

```jsonc
{
  "statCards": [
    {
      "type": "total_users",
      "label": "Tổng người dùng",
      "value": 1357,
      "deltaPercent": 12, // ((currentTotalUsers - previousTotalUsers) / previousTotalUsers) * 100
    },
    {
      "type": "engagement",
      "label": "Tương tác (DAU/MAU)",
      "dau": 892, // distinct users active trong ngày cuối kỳ (hoặc ngày được chọn)
      "mau": 1234, // distinct users active trong 30 ngày gần nhất
      "stickinessPercent": 72, // (dau / mau) * 100
    },
    {
      "type": "transactions",
      "label": "Tổng giá trị giao dịch",
      "totalTransactionValue": 100000000, // SUM(amount) với transaction status hợp lệ trong kỳ của timeframe
      "totalTransactions": 200, // COUNT(*) transaction trong kỳ của timeframe
    },
    {
      "type": "system_health",
      "label": "Sức khỏe hệ thống",
      "errorRatePercent": 0.02, // (failed_sync_or_ocr_jobs / total_sync_or_ocr_jobs) * 100
      "status": "Good",
      "bannedUsers": 12,
    },
  ],
  "transactionVolumeTrend": [
    //bảng xu hướng thể hiện tổng giá trị giao dịch + số lượng giao dịch theo param day | month | year
    { "label": "T2", "amount": 14000000, "count": 28 },
    { "label": "T3", "amount": 12000000, "count": 24 },
    { "label": "T4", "amount": 16000000, "count": 32 },
    { "label": "T5", "amount": 13000000, "count": 26 },
    { "label": "T6", "amount": 17000000, "count": 34 },
    { "label": "T7", "amount": 12000000, "count": 24 },
    { "label": "CN", "amount": 16000000, "count": 32 },
  ],
  "topSpendingCategories": [
    { "label": "Ăn uống", "value": 32000000 }, // SUM(amount) của category
    { "label": "Mua sắm", "value": 24000000 }, // tính theo công thức mô tả bên dưới
    { "label": "Di chuyển", "value": 18000000 },
    { "label": "Giải trí", "value": 14000000 },
    { "label": "Khác", "value": 12000000 }, // Tổng phần còn lại ngoài top category
  ],
  "retentionTrend": [
    {
      "periodLabel": "D0", // mốc ngày kể từ thời điểm onboarding của cohort
      "cohortA": 100, // Nhóm Q1/2026: D0 luôn = 100%
      "cohortB": 100, // Nhóm Q2/2025: D0 luôn = 100%
      "cohortC": 100, // Nhóm Q3/2025: D0 luôn = 100%
      "cohortD": 100, // Nhóm Q4/2025: D0 luôn = 100%
    },
    {
      "periodLabel": "D7",
      "cohortA": 76, // (activeUsersInCohortAtDay7 / totalUsersInCohort) * 100
      "cohortB": 72, // (activeUsersInCohortAtDay7 / totalUsersInCohort) * 100
      "cohortC": 68, // (activeUsersInCohortAtDay7 / totalUsersInCohort) * 100
      "cohortD": 64, // (activeUsersInCohortAtDay7 / totalUsersInCohort) * 100
    },
    {
      "periodLabel": "D14",
      "cohortA": 66, // (activeUsersInCohortAtDay14 / totalUsersInCohort) * 100
      "cohortB": 61, // (activeUsersInCohortAtDay14 / totalUsersInCohort) * 100
      "cohortC": 57, // (activeUsersInCohortAtDay14 / totalUsersInCohort) * 100
      "cohortD": 53, // (activeUsersInCohortAtDay14 / totalUsersInCohort) * 100
    },
    {
      "periodLabel": "D21",
      "cohortA": 59, // (activeUsersInCohortAtDay21 / totalUsersInCohort) * 100
      "cohortB": 55, // (activeUsersInCohortAtDay21 / totalUsersInCohort) * 100
      "cohortC": 51, // (activeUsersInCohortAtDay21 / totalUsersInCohort) * 100
      "cohortD": 47, // (activeUsersInCohortAtDay21 / totalUsersInCohort) * 100
    },
    {
      "periodLabel": "D30",
      "cohortA": 52, // (activeUsersInCohortAtDay30 / totalUsersInCohort) * 100
      "cohortB": 48, // (activeUsersInCohortAtDay30 / totalUsersInCohort) * 100
      "cohortC": 44, // (activeUsersInCohortAtDay30 / totalUsersInCohort) * 100
      "cohortD": 40, // (activeUsersInCohortAtDay30 / totalUsersInCohort) * 100
    },
  ],
  "recentUsers": [
    //danh sách 10 user mới nhất nhm reverse
    {
      "id": "guid",
      "fullName": "Nguyen Van Anh",
      "email": "anh@finjar.app",
      "status": "Active",
      "createdAt": "ISO8601",
    },
  ],
  "recentTransactions": [
    //danh sách 10 transaction mới nhất nhm reverse
    {
      "id": "guid",
      "type": "Expense",
      "amount": 120000,
      "note": "Ăn trưa",
      "transactionDate": "ISO8601",
    },
  ],
}
```

Ghi chú:

- API này là nguồn dữ liệu duy nhất cho trang Admin Dashboard FE hiện tại.
- `statCards` giữ dạng array, mỗi phần tử phải có `type` để FE map icon/format ổn định.
- `statCards` ở phiên bản hiện tại phải trả đúng 4 phần tử theo thứ tự:
  `total_users`, `engagement`, `transactions`, `system_health`.
- `transactionVolumeTrend` phải trả về theo đúng `timeframe`:
  - `day`: 7 điểm gần nhất, `label` = `T2..CN` (hoặc format ngày ngắn), mỗi điểm trả cả `amount` và `count` theo ngày.
  - `month`: 12 điểm gần nhất, `label` = `T1..T12` (hoặc `YYYY-MM`), mỗi điểm trả cả `amount` và `count` theo tháng.
  - `year`: 5 điểm gần nhất, `label` = `YYYY`, mỗi điểm trả cả `amount` và `count` theo năm.
- `topSpendingCategories`:
  - Công thức: `value = SUM(amount)` theo từng category.
  - Sắp xếp giảm dần theo `value`, trả top N (khuyến nghị N=4), phần còn lại gộp thành `"Khác"`.
  - FE tính tỷ trọng hiển thị theo công thức: `value / SUM(value của tất cả category) * 100`.
- `retentionTrend`:
  - `periodLabel` là số ngày kể từ mốc onboarding của cohort: `D0`, `D7`, `D14`, `D21`, `D30`.
  - `cohortA..D` là tỷ lệ % user còn active tại mốc đó, giá trị kỳ vọng trong khoảng `0..100`.
  - Công thức tại mốc `Dx`:
    `retentionPercent = (activeUsersInCohortAtDayX / totalUsersInCohortAtD0) * 100`. (này xem từ audit tìm lastLogin)
  - Định nghĩa cohort (phiên bản hiện tại):
    - `cohortA`: user hoàn tất onboarding trong Q1/2026
    - `cohortB`: user hoàn tất onboarding trong Q2/2025
    - `cohortC`: user hoàn tất onboarding trong Q3/2025
    - `cohortD`: user hoàn tất onboarding trong Q4/2025
  - Quy tắc làm tròn: làm tròn số nguyên gần nhất.
  - Quy tắc dữ liệu: nên đảm bảo xu hướng không tăng bất thường trong cùng cohort (`D0 >= D7 >= D14 >= D21 >= D30`).

### `GET /api/v1/admin/audit-logs`

- Auth: Admin

Query Params

- `adminId=guid`
- `actionType=Login|BanUser|CategoryChange|BroadcastSend`
- `entityType=User|Category|Broadcast|AiSetting`
- `fromDate=2026-04-01`
- `toDate=2026-04-30`
- `page=1`
- `pageSize=50`

Response `200 OK`

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

- Auth: Admin

Response `200 OK`

```json
{
  "modelName": "gpt-4o-mini",
  "systemPrompt": "string",
  "temperature": 0.7,
  "maxTokens": 1000,
  "isEnabled": true,
  "apiKeyMasked": "sk-...xxxx"
}
```

### `PATCH /api/v1/admin/ai-settings`

- Auth: Admin

Request

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

Response `200 OK`

```json
{
  "modelName": "gpt-4o-mini",
  "isEnabled": true
}
```

## 6. Field response đã cắt giảm so với bản cũ

Đã cắt bỏ khỏi nhiều write responses:

- `createdAt`
- `updatedAt`
- các trường "balance sau cập nhật" không bắt buộc cho màn hình hiện tại
- các metadata nội bộ không dùng để render

Nguyên tắc áp dụng:

- Nếu FE cần field để hiển thị trực tiếp, sắp xếp, lọc, tình trạng cảnh báo hoặc support quyết định user thì giữ.
- Nếu field chỉ phục vụ nội bộ backend và FE không dùng ngay trong UI flow thì bỏ.

## 7. Ghi chú thực thi cho team

### Frontend

- Ưu tiên gọi đúng endpoint theo use case màn hình.
- Không tự tính toán các số liệu nghiệp vụ mà backend đã cung cấp (limit status, goal progress, v.v.).

### Backend

- Không expose entity DB trực tiếp.
- Mapping DTO theo nguyên tắc: write gọn, read đủ thông tin cho UI.
- Xử lý atomic cho allocate/transfer/transaction/contribution.

## 8. Tổng kết

Bản này đã đồng bộ theo hướng:

- Access token only (bỏ refresh token flow trong contract core).
- Thêm nhóm API financial accounts theo core user story và SQL schema.
- Cắt giảm field response thừa, đặc biệt `createdAt/updatedAt` ở các endpoint ghi.
- Giữ lại có chủ đích các field thời gian/chỉ số mà FE cần để render màn hình hoặc filter/sort.
