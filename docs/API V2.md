# API v2 - Tổng hợp endpoint hiện có trong controller

Tài liệu này tổng hợp theo code controller hiện tại trong `Personal_Finance_Management.Api\Controllers` và DTO trong `Personal_Finance_Management.Service`.

Quy ước trong tài liệu:

- Field được ghi theo JSON name dự kiến khi serialize từ ASP.NET Core mặc định: PascalCase trong DTO sẽ thành camelCase, field đã lower camelCase giữ nguyên.
- Không dùng dữ liệu mẫu. Các JSON block bên dưới là schema-like JSON: value là kiểu dữ liệu, ví dụ `"guid"`, `"decimal"`, `"datetimeOffset"`.
- Field optional/nullable được ghi bằng kiểu `"type | null"`.
- Array được ghi bằng một object mẫu trong mảng, ví dụ `"data": [{ ... }]`.
- `auth` mô tả quyền gọi endpoint, không phải field body gửi lên API.
- `Bearer` nghĩa là endpoint cần access token. Nếu controller dùng `[Authorize]` mà chưa chỉ rõ policy thì ghi `Bearer`.
- Chỗ chưa rõ hoặc chưa implement được đánh dấu `[TODO]` và để dòng `Cần bổ sung: ____`.

## 1. Auth, User, Onboarding

### Auth

#### `POST /api/v1/auth/register`

Request:

```json
{
  "auth": "Public",
  "body": {
    "username": "string",
    "email": "string",
    "password": "string",
    "firstName": "string",
    "lastName": "string"
  }
}
```

Response:

```json
{
  "status": 201,
  "body": {
    "id": "guid",
    "username": "string",
    "firstName": "string",
    "lastName": "string",
    "email": "string",
    "isOnboardingCompleted": "boolean",
    "accessToken": "string"
  }
}
```

#### `POST /api/v1/auth/login`

Request:

```json
{
  "auth": "Public",
  "body": {
    "email": "string",
    "password": "string"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "username": "string",
    "firstName": "string",
    "lastName": "string",
    "email": "string",
    "role": "string",
    "isOnboardingCompleted": "boolean",
    "accessToken": "string"
  }
}
```

#### `POST /api/v1/auth/logout`

Request:

```json
{
  "auth": "Public",
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "message": "string"
  }
}
```

Ghi chú đã chốt:

- Register chốt response status là `201 Created`; controller đã cập nhật theo quyết định này.
- Login chốt request là `email + password`.
- `POST /api/v1/auth/logout` hiện không có `[Authorize]` ở controller.
- Không thêm endpoint riêng `POST /api/v1/admin/auth/login`; admin dùng auth login hiện có và phân quyền bằng role/authorize.

### User Profile

#### `GET /api/v1/user/me`

Request:

```json
{
  "auth": "Bearer User",
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "userName": "string",
    "firstName": "string",
    "lastName": "string",
    "email": "string",
    "phone": "string | null",
    "avatarUrl": "string | null",
    "preferredCurrency": "string",
    "isOnboardingCompleted": "boolean"
  }
}
```

#### `PATCH /api/v1/user/me`

Request:

```json
{
  "auth": "Bearer User",
  "body": {
    "firstName": "string | null",
    "lastName": "string | null",
    "phone": "string | null",
    "avatarUrl": "string | null"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "fullName": "string",
    "phone": "string",
    "avatarUrl": "string"
  }
}
```

#### `GET /api/v1/user/me/setup`

Request:

```json
{
  "auth": "Bearer User",
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "isOnboardingCompleted": "boolean",
    "monthlyIncome": "decimal | null",
    "budgetMethod": "string",
    "defaultFinancialAccountId": "guid | null",
    "jarCount": "int",
    "financialAccountCount": "int",
    "limitCount": "int",
    "activeGoalCount": "int"
  }
}
```

### Onboarding

#### `POST /api/v1/onboarding`

Request:

```json
{
  "auth": "Bearer User",
  "body": {
    "monthlyIncome": "int",
    "occupationType": "string",
    "financialGoalTypes": ["string"],
    "budgetMethodPreference": "string",
    "ageRange": "string",
    "spendingChallenges": ["string"]
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "recommendedMethod": "string",
    "recommendedCategories": [
      {
        "name": "string",
        "icon": "string"
      }
    ],
    "recommendedJars": [
      {
        "name": "string"
      }
    ],
    "defaultFinancialAccount": {
      "name": "string",
      "accountType": "string"
    }
  }
}
```

## 2. Financial Account, Jar, Category

### Financial Account

#### `GET /api/v1/financial-accounts`

Request:

```json
{
  "auth": "Bearer",
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "data": [
      {
        "id": "guid",
        "name": "string",
        "accountType": "string",
        "connectionMode": "string",
        "providerName": "string | null",
        "maskedAccountNumber": "string | null",
        "currency": "string",
        "currentBalance": "decimal",
        "syncStatus": "string",
        "isDefault": "boolean",
        "isActive": "boolean"
      }
    ]
  }
}
```

#### `POST /api/v1/financial-accounts/Manual`

Request:

```json
{
  "auth": "Bearer",
  "body": {
    "name": "string",
    "accountType": "string",
    "currentBalance": "decimal",
    "currency": "string | null",
    "isDefault": "boolean"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "name": "string",
    "accountType": "string",
    "connectionMode": "string",
    "currentBalance": "decimal",
    "currency": "string",
    "isDefault": "boolean",
    "isActive": "boolean"
  }
}
```

#### `POST /api/v1/financial-accounts/LinkApi`

Request:

```json
{
  "auth": "Bearer",
  "body": {
    "bankName": "string",
    "bankCode": "string | null",
    "accountNumber": "string",
    "accountHolderName": "string | null",
    "isDefault": "boolean"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "name": "string",
    "accountType": "string",
    "connectionMode": "string",
    "providerName": "string",
    "maskedAccountNumber": "string",
    "currentBalance": "decimal",
    "currency": "string",
    "syncStatus": "string",
    "isDefault": "boolean",
    "isActive": "boolean"
  }
}
```

#### `PATCH /api/v1/financial-accounts/{id}`

Request:

```json
{
  "auth": "Bearer",
  "route": {
    "id": "guid"
  },
  "body": {
    "name": "string | null",
    "currentBalance": "decimal | null",
    "isDefault": "boolean | null"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "name": "string",
    "currentBalance": "decimal",
    "isDefault": "boolean",
    "updatedAt": "datetimeOffset"
  }
}
```

#### `DELETE /api/v1/financial-accounts/{id}`

Request:

```json
{
  "auth": "Bearer",
  "route": {
    "id": "guid"
  },
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "message": "string"
  }
}
```

Ghi chú:

- Route và method đang ghi theo controller hiện tại.
- Update response hiện có `updatedAt` theo DTO hiện tại.

### Jar

#### `GET /api/v1/jars`

Request:

```json
{
  "auth": "Bearer",
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "methodType": "string",
    "totalJarBalance": "decimal",
    "unallocatedBalance": "decimal",
    "data": [
      {
        "id": "guid",
        "name": "string",
        "balance": "decimal",
        "color": "string",
        "icon": "string",
        "status": "string"
      }
    ]
  }
}
```

#### `POST /api/v1/jars`

Request:

```json
{
  "auth": "Bearer",
  "body": {
    "name": "string",
    "color": "string",
    "icon": "string"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "name": "string",
    "balance": "decimal",
    "status": "string"
  }
}
```

#### `PATCH /api/v1/jars/{id}`

Request:

```json
{
  "auth": "Bearer",
  "route": {
    "id": "guid"
  },
  "body": {
    "name": "string | null",
    "color": "string | null",
    "icon": "string | null"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "name": "string",
    "color": "string",
    "icon": "string",
    "status": "string"
  }
}
```

#### `DELETE /api/v1/jars/{id}`

Request:

```json
{
  "auth": "Bearer",
  "route": {
    "id": "guid"
  },
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "message": "string"
  }
}
```

Ghi chú:

- Route đang ghi theo controller hiện tại.
- Public jar setup/allocate/transfer endpoints không có trong controller hiện tại.

### Category

#### `GET /api/v1/categories`

Request:

```json
{
  "auth": "Bearer User",
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "defaultCategories": [
      {
        "id": "guid",
        "name": "string",
        "icon": "string | null",
        "color": "string | null"
      }
    ],
    "customCategories": [
      {
        "id": "guid",
        "name": "string",
        "icon": "string | null",
        "color": "string | null"
      }
    ]
  }
}
```

#### `POST /api/v1/categories`

Request:

```json
{
  "auth": "Bearer User",
  "body": {
    "name": "string",
    "icon": "string | null",
    "color": "string | null"
  }
}
```

Response:

```json
{
  "status": 201,
  "body": {
    "id": "guid",
    "name": "string",
    "icon": "string | null",
    "color": "string | null"
  }
}
```

#### `PATCH /api/v1/categories/{id}`

Request:

```json
{
  "auth": "Bearer User",
  "route": {
    "id": "guid"
  },
  "body": {
    "name": "string | null",
    "icon": "string | null",
    "color": "string | null"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "name": "string",
    "icon": "string | null",
    "color": "string | null"
  }
}
```

#### `DELETE /api/v1/categories/{id}`

Request:

```json
{
  "auth": "Bearer User",
  "route": {
    "id": "guid"
  },
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "message": "string"
  }
}
```

## 3. Transactions, Import, Casso

### Transactions

#### `GET /api/v1/transactions`

Request:

```json
{
  "auth": "Bearer",
  "query": {
    "pageIndex": "int",
    "pageSize": "int",
    "financialAccountId": "guid | null",
    "type": "string | null",
    "jarId": "guid | null",
    "categoryId": "guid | null",
    "fromDate": "date | null",
    "toDate": "date | null",
    "keyword": "string | null",
    "sortBy": "string | null",
    "sortDir": "string | null"
  },
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "data": [
      {
        "id": "guid",
        "type": "string",
        "transactionsAmount": "decimal",
        "note": "string | null",
        "date": "datetimeOffset",
        "financialAccount": {
          "id": "guid | null",
          "name": "string | null"
        },
        "jar": {
          "id": "guid | null",
          "name": "string | null"
        },
        "category": {
          "id": "guid | null",
          "name": "string | null"
        }
      }
    ],
    "pagination": {
      "page": "int",
      "pageSize": "int",
      "totalCount": "int",
      "totalPages": "int"
    }
  }
}
```

#### `POST /api/v1/transactions`

Request:

```json
{
  "auth": "Bearer",
  "body": {
    "financialAccountId": "guid | null",
    "type": "string",
    "transactionsAmount": "decimal",
    "categoryId": "guid | null",
    "fromJarId": "guid | null",
    "toJarId": "guid | null",
    "note": "string | null",
    "date": "datetimeOffset"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "financialAccountId": "guid | null",
    "type": "string",
    "transactionsAmount": "decimal",
    "date": "datetimeOffset"
  }
}
```

#### `PATCH /api/v1/transactions/{id}`

Request:

```json
{
  "auth": "Bearer",
  "route": {
    "id": "guid"
  },
  "body": {
    "transactionsAmount": "decimal | null",
    "categoryId": "guid | null",
    "note": "string | null"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "type": "string",
    "transactionsAmount": "decimal",
    "date": "datetimeOffset"
  }
}
```

#### `DELETE /api/v1/transactions/{id}`

Request:

```json
{
  "auth": "Bearer",
  "route": {
    "id": "guid"
  },
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "message": "string"
  }
}
```

Ghi chú:

- Route đang ghi theo controller hiện tại.
- FE gửi `transactionsAmount` là số dương cho cả `Income` và `Expense`.
- Request create hiện có `fromJarId` và `toJarId` theo DTO hiện tại.
- `date` là thời điểm phát sinh giao dịch, không phải ràng buộc chỉ cho giao dịch ở hiện tại.
- User được nhập giao dịch trong quá khứ.
- User không được nhập giao dịch trong tương lai.
- Request vẫn dùng `datetimeOffset` để lưu cả ngày và giờ, nhưng service phải validate `date <= now`.

### Casso Transaction Integration

#### `GET /api/v1/transactions/Casso`

Request:

```json
{
  "auth": "Bearer",
  "query": {
    "financialAccountId": "guid",
    "fromDate": "date | null",
    "toDate": "date | null",
    "page": "int",
    "pageSize": "int",
    "sort": "string | null"
  },
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "receivedCount": "int",
    "createdCount": "int",
    "skippedCount": "int",
    "message": "string"
  }
}
```

#### `POST /api/v1/transactions/Casso`

Request:

```json
{
  "auth": "Anonymous",
  "headers": {
    "secure-token": "string | null",
    "X-Casso-Signature": "string | null"
  },
  "body": {
    "error": "int",
    "data": "json"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "receivedCount": "int",
    "createdCount": "int",
    "skippedCount": "int",
    "message": "string"
  }
}
```

Ghi chú:

- Đây là endpoint integration Casso, route đang ghi theo controller hiện tại.

### Import/OCR

#### `POST /api/v1/imports/image`

Request:

```json
{
  "auth": "Bearer User",
  "contentType": "multipart/form-data",
  "formData": {
    "file": "file",
    "layout": "string | null",
    "runOcr": "boolean"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "message": "string",
    "fileName": "string",
    "originalFileName": "string",
    "storedFilePath": "string",
    "contentType": "string | null",
    "sizeInBytes": "long",
    "ocrJsonFileName": "string | null",
    "storedOcrJsonPath": "string | null",
    "rawOcrJson": "string | null",
    "ocrResult": {
      "isSuccess": "boolean",
      "text": "string | null",
      "layout": "string | null",
      "engine": "string",
      "rawJson": "string | null",
      "statusCode": "int | null",
      "errorMessage": "string | null"
    }
  }
}
```

TODO:

- `[TODO]` Controller hiện chỉ có `POST /api/v1/imports/image`, chưa có full flow `POST /api/v1/imports`, `GET /api/v1/imports/{id}`, `GET /api/v1/imports/{id}/preview`, `POST /api/v1/imports/{id}/confirm`. Cần bổ sung: ____

## 4. Dashboard, AI

### Personal Dashboard

#### `GET /api/v1/dashboard`

Request:

```json
{
  "auth": "Bearer",
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "balanceSummary": {
      "totalBalance": "decimal",
      "allocatedBalance": "decimal",
      "unallocatedBalance": "decimal",
      "totalIncome": "decimal",
      "totalExpense": "decimal",
      "netChange": "decimal"
    },
    "financialAccounts": [
      {
        "id": "guid",
        "name": "string",
        "currentBalance": "decimal",
        "isDefault": "boolean"
      }
    ],
    "jarSummary": [
      {
        "jarId": "guid",
        "jarName": "string",
        "balance": "decimal",
        "spent": "decimal",
        "spentPercentage": "decimal"
      }
    ],
    "categoryBreakdown": [
      {
        "categoryId": "guid",
        "categoryName": "string",
        "totalAmount": "decimal",
        "percentage": "decimal"
      }
    ],
    "recentTransactions": [
      {
        "id": "guid",
        "type": "string",
        "transactionsAmount": "decimal",
        "note": "string | null",
        "date": "datetimeOffset"
      }
    ],
    "goalProgress": [
      {
        "goalId": "guid",
        "title": "string",
        "progressPercentage": "decimal",
        "daysRemaining": "decimal"
      }
    ]
  }
}
```

### AI Chat

#### `POST /api/v1/ai/chat`

Request:

```json
{
  "auth": "Bearer User",
  "body": {
    "message": "string",
    "recentMessages": [
      {
        "sender": "string",
        "content": "string"
      }
    ]
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "answer": "string",
    "suggestions": ["string"],
    "source": "string"
  }
}
```

## 5. Limits, Goals, Reminders, Notifications

### Limits

#### `GET /api/v1/limits`

Request:

```json
{
  "auth": "Bearer",
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "data": [
      {
        "id": "guid",
        "targetType": "Jar | Category",
        "targetId": "guid",
        "targetName": "string",
        "limitAmount": "decimal",
        "period": "string",
        "alertAtPercentage": "decimal",
        "currentSpent": "decimal",
        "currentPercentage": "double",
        "status": "string"
      }
    ]
  }
}
```

#### `POST /api/v1/limits`

Request:

```json
{
  "auth": "Bearer",
  "body": {
    "targetType": "Jar | Category",
    "targetId": "guid",
    "limitAmount": "decimal",
    "period": "string",
    "alertAtPercentage": "decimal"
  }
}
```

Response:

```json
{
  "status": 201,
  "body": {
    "id": "guid",
    "targetType": "Jar | Category",
    "targetId": "guid",
    "limitAmount": "decimal",
    "period": "string",
    "alertAtPercentage": "decimal"
  }
}
```

#### `PATCH /api/v1/limits/{id}`

Request:

```json
{
  "auth": "Bearer",
  "route": {
    "id": "guid"
  },
  "body": {
    "limitAmount": "decimal | null",
    "alertAtPercentage": "decimal | null"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "limitAmount": "decimal",
    "alertAtPercentage": "decimal"
  }
}
```

#### `DELETE /api/v1/limits/{id}`

Request:

```json
{
  "auth": "Bearer",
  "route": {
    "id": "guid"
  },
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "message": "string"
  }
}
```

### Goals

#### `GET /api/v1/goals`

Request:

```json
{
  "auth": "Bearer",
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "data": [
      {
        "id": "guid",
        "title": "string",
        "targetAmount": "decimal",
        "savedAmount": "decimal",
        "progressPercentage": "double",
        "dueDate": "datetime",
        "status": "string",
        "suggestedMonthlyContribution": "decimal"
      }
    ]
  }
}
```

#### `GET /api/v1/goals/{id}`

Request:

```json
{
  "auth": "Bearer",
  "route": {
    "id": "guid"
  },
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "title": "string",
    "targetAmount": "decimal",
    "savedAmount": "decimal",
    "progressPercentage": "double",
    "dueDate": "datetime",
    "daysRemaining": "int",
    "status": "string",
    "suggestedMonthlyContribution": "decimal",
    "linkedJarId": "guid | null"
  }
}
```

#### `POST /api/v1/goals`

Request:

```json
{
  "auth": "Bearer",
  "body": {
    "title": "string",
    "targetAmount": "decimal",
    "dueDate": "datetime",
    "linkedJarId": "guid | null",
    "note": "string | null"
  }
}
```

Response:

```json
{
  "status": 201,
  "body": {
    "id": "guid",
    "title": "string",
    "targetAmount": "decimal",
    "savedAmount": "decimal",
    "progressPercentage": "double",
    "status": "string",
    "dueDate": "datetime"
  }
}
```

#### `PATCH /api/v1/goals/{id}`

Request:

```json
{
  "auth": "Bearer",
  "route": {
    "id": "guid"
  },
  "body": {
    "title": "string | null",
    "targetAmount": "decimal | null",
    "dueDate": "datetime | null",
    "linkedJarId": "guid | null",
    "note": "string | null"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "title": "string",
    "targetAmount": "decimal",
    "dueDate": "datetime",
    "status": "string"
  }
}
```

#### `DELETE /api/v1/goals/{id}`

Request:

```json
{
  "auth": "Bearer",
  "route": {
    "id": "guid"
  },
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "message": "string"
  }
}
```

Ghi chú đã chốt:

- Không dùng endpoint `POST /api/v1/goals/{id}/contributions`; flow đóng góp mục tiêu đã gộp qua transaction.

### Reminders

#### `GET /api/v1/reminders`

Request:

```json
{
  "auth": "Bearer",
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "data": [
      {
        "id": "guid",
        "title": "string",
        "amount": "decimal",
        "frequency": "string",
        "nextDueDate": "datetimeOffset",
        "status": "string"
      }
    ]
  }
}
```

#### `POST /api/v1/reminders`

Request:

```json
{
  "auth": "Bearer",
  "body": {
    "title": "string",
    "amount": "decimal",
    "frequency": "string",
    "dayOfMonth": "short | null",
    "startDate": "datetimeOffset",
    "categoryId": "guid | null",
    "notifyDaysBefore": "short | null",
    "note": "string | null"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "title": "string",
    "amount": "decimal",
    "frequency": "string",
    "nextDueDate": "datetimeOffset",
    "status": "string"
  }
}
```

#### `PATCH /api/v1/reminders/{id}`

Request:

```json
{
  "auth": "Bearer",
  "route": {
    "id": "guid"
  },
  "body": {
    "title": "string | null",
    "amount": "decimal | null",
    "frequency": "string | null",
    "dayOfMonth": "int | null",
    "status": "Active | Paused | Completed | Cancelled | null",
    "notifyDaysBefore": "int | null",
    "note": "string | null"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "title": "string",
    "frequency": "string",
    "nextDueDate": "datetimeOffset",
    "status": "string"
  }
}
```

#### `DELETE /api/v1/reminders/{id}`

Request:

```json
{
  "auth": "Bearer",
  "route": {
    "id": "guid"
  },
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "message": "string"
  }
}
```

Ghi chú đã chốt:

- Reminder status hợp lệ là `Active`, `Paused`, `Completed`, `Cancelled`.
- `DELETE /api/v1/reminders/{id}` chuyển status sang `Cancelled`.

### Notifications

#### `GET /api/v1/notifications`

Request:

```json
{
  "auth": "Bearer",
  "query": {
    "type": "string | null",
    "status": "string | null",
    "pageSize": "int",
    "pageIndex": "int"
  },
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "items": [
      {
        "id": "guid",
        "type": "string",
        "title": "string",
        "body": "string",
        "isRead": "boolean",
        "occurredAt": "datetimeOffset"
      }
    ],
    "totalItems": "int",
    "pageSize": "int",
    "pageIndex": "int",
    "unreadCount": "int"
  }
}
```

#### `PATCH /api/v1/notifications/status`

Request:

```json
{
  "auth": "Bearer",
  "body": {
    "ids": ["guid"],
    "isRead": "boolean",
    "markAll": "boolean"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "updatedCount": "int",
    "unreadCount": "int"
  }
}
```

## 6. Admin APIs

### Admin User Management

#### `GET /api/v1/admin/users`

Request:

```json
{
  "auth": "Bearer Admin",
  "query": {
    "pageIndex": "int",
    "pageSize": "int",
    "status": "Active | Banned | null",
    "keyword": "string | null"
  },
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "data": [
      {
        "id": "guid",
        "userName": "string",
        "firstName": "string",
        "lastName": "string",
        "email": "string",
        "phone": "string | null",
        "avatarUrl": "string | null",
        "preferredCurrency": "string",
        "isOnboardingCompleted": "boolean",
        "status": "string",
        "statusReason": "string | null",
        "createdAt": "datetimeOffset",
        "lastLoginAt": "datetimeOffset | null"
      }
    ],
    "pagination": {
      "page": "int",
      "pageSize": "int",
      "totalCount": "int",
      "totalPages": "int"
    }
  }
}
```

#### `GET /api/v1/admin/users/{id}`

Request:

```json
{
  "auth": "Bearer Admin",
  "route": {
    "id": "guid"
  },
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "userName": "string",
    "firstName": "string",
    "lastName": "string",
    "email": "string",
    "phone": "string | null",
    "avatarUrl": "string | null",
    "preferredCurrency": "string",
    "isOnboardingCompleted": "boolean",
    "status": "string",
    "statusReason": "string | null",
    "createdAt": "datetimeOffset",
    "lastLoginAt": "datetimeOffset | null"
  }
}
```

#### `PATCH /api/v1/admin/users/{id}/status`

Request:

```json
{
  "auth": "Bearer Admin",
  "route": {
    "id": "guid"
  },
  "body": {
    "status": "Active | Banned",
    "statusReason": "string | null"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "userName": "string",
    "firstName": "string",
    "lastName": "string",
    "email": "string",
    "phone": "string | null",
    "avatarUrl": "string | null",
    "preferredCurrency": "string",
    "isOnboardingCompleted": "boolean",
    "status": "string",
    "statusReason": "string | null",
    "createdAt": "datetimeOffset",
    "lastLoginAt": "datetimeOffset | null"
  }
}
```

#### `PATCH /api/v1/change-role/{accountId}`

Request:

```json
{
  "auth": "Bearer Admin",
  "route": {
    "accountId": "guid"
  },
  "queryOrBody": {
    "role": "AccountRole"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": "string"
}
```

Ghi chú:

- `GET /api/v1/admin/users` chỉ trả account role `User`.
- Change role route đang ghi đúng theo controller hiện tại là `/api/v1/change-role/{accountId}`; hiện giữ theo controller.
- `PATCH /api/v1/admin/users/{id}/status` không toggle ngầm; admin gửi status đích rõ ràng.

### Admin Categories

#### `GET /api/v1/admin/categories`

Request:

```json
{
  "auth": "Bearer Admin",
  "query": {
    "isActive": "boolean | null"
  },
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "data": [
      {
        "id": "guid",
        "name": "string",
        "icon": "string | null",
        "color": "string | null",
        "order": "int",
        "isActive": "boolean"
      }
    ]
  }
}
```

#### `POST /api/v1/admin/categories`

Request:

```json
{
  "auth": "Bearer Admin",
  "body": {
    "name": "string",
    "icon": "string | null",
    "color": "string | null",
    "order": "int"
  }
}
```

Response:

```json
{
  "status": 201,
  "body": {
    "id": "guid",
    "name": "string",
    "icon": "string | null",
    "color": "string | null",
    "order": "int",
    "isActive": "boolean"
  }
}
```

#### `PATCH /api/v1/admin/categories/{id}`

Request:

```json
{
  "auth": "Bearer Admin",
  "route": {
    "id": "guid"
  },
  "body": {
    "name": "string | null",
    "icon": "string | null",
    "color": "string | null",
    "order": "int | null",
    "isActive": "boolean | null"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "name": "string",
    "icon": "string | null",
    "color": "string | null",
    "order": "int",
    "isActive": "boolean"
  }
}
```

#### `DELETE /api/v1/admin/categories/{id}`

Request:

```json
{
  "auth": "Bearer Admin",
  "route": {
    "id": "guid"
  },
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "message": "string"
  }
}
```

### Admin Broadcasts

#### `POST /api/v1/admin/broadcasts`

Request:

```json
{
  "auth": "Bearer Admin",
  "body": {
    "title": "string",
    "body": "string",
    "targetAudience": "string",
    "scheduledAt": "datetimeOffset | null"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "id": "guid",
    "title": "string",
    "body": "string",
    "targetAudience": "string",
    "status": "string",
    "scheduledAt": "datetimeOffset | null",
    "sentAt": "datetimeOffset | null",
    "targetCount": "int",
    "deliveredCount": "int"
  }
}
```

#### `GET /api/v1/admin/broadcasts`

Request:

```json
{
  "auth": "Bearer Admin",
  "query": {
    "pageIndex": "int",
    "pageSize": "int",
    "status": "string"
  },
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "items": [
      {
        "id": "guid",
        "title": "string",
        "body": "string",
        "targetAudience": "string",
        "status": "string",
        "scheduledAt": "datetimeOffset | null",
        "sentAt": "datetimeOffset | null",
        "targetCount": "int",
        "deliveredCount": "int"
      }
    ],
    "pagination": {
      "pageIndex": "int",
      "pageSize": "int",
      "totalCount": "int",
      "totalPages": "int"
    }
  }
}
```

### Admin Dashboard, Audit Log, AI Settings

#### `GET /api/v1/admin/dashboard`

Request:

```json
{
  "auth": "Bearer Admin",
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "summary": {
      "totalUsers": "int",
      "newUsersThisMonth": "int",
      "activeUsersLast30Days": "int",
      "bannedUsers": "int",
      "totalTransactions": "int",
      "transactionsThisMonth": "int",
      "totalJars": "int",
      "activeGoals": "int",
      "pendingImportJobs": "int"
    },
    "recentUsers": [
      {
        "id": "guid",
        "username": "string",
        "firstName": "string",
        "lastName": "string",
        "email": "string",
        "status": "string",
        "isOnboardingCompleted": "boolean",
        "lastLoginAt": "datetimeOffset | null"
      }
    ],
    "recentTransactions": [
      {
        "id": "guid",
        "type": "string",
        "transactionsAmount": "decimal",
        "note": "string | null",
        "transactionDate": "datetimeOffset",
        "user": {
          "id": "guid",
          "username": "string",
          "firstName": "string",
          "lastName": "string"
        },
        "financialAccount": {
          "id": "guid",
          "name": "string",
          "accountType": "string"
        },
        "category": {
          "id": "guid",
          "name": "string"
        }
      }
    ]
  }
}
```

#### `GET /api/v1/admin/audit-logs`

Request:

```json
{
  "auth": "Bearer Admin",
  "query": {
    "adminId": "guid | null",
    "actionType": "string | null",
    "entityType": "string | null",
    "fromDate": "datetimeOffset | null",
    "toDate": "datetimeOffset | null",
    "page": "int",
    "pageSize": "int"
  },
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "items": [
      {
        "id": "guid",
        "adminUsername": "string",
        "actionType": "string",
        "entityType": "string",
        "description": "string",
        "createdAt": "datetimeOffset"
      }
    ],
    "pagination": {
      "pageIndex": "int",
      "pageSize": "int",
      "totalCount": "int",
      "totalPages": "int"
    }
  }
}
```

#### `GET /api/v1/admin/ai-settings`

Request:

```json
{
  "auth": "Bearer Admin",
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "modelName": "string",
    "systemPrompt": "string",
    "temperature": "decimal",
    "maxTokens": "int",
    "isEnabled": "boolean",
    "apiKeyMasked": "string | null"
  }
}
```

#### `PATCH /api/v1/admin/ai-settings`

Request:

```json
{
  "auth": "Bearer Admin",
  "body": {
    "modelName": "string | null",
    "systemPrompt": "string | null",
    "temperature": "decimal | null",
    "maxTokens": "int | null",
    "isEnabled": "boolean | null"
  }
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "modelName": "string",
    "isEnabled": "boolean"
  }
}
```

Security note:

- Admin AI settings response chỉ có `apiKeyMasked`, không có raw API key trong DTO response hiện tại.

## 7. Health, Legacy/Test Endpoints

### Health

#### `GET /health`

Request:

```json
{
  "auth": "Public",
  "body": null
}
```

Response:

```json
{
  "status": 200,
  "body": {
    "status": "string"
  }
}
```

#### `GET /health/db/local`

Request:

```json
{
  "auth": "Public",
  "body": null
}
```

Success response:

```json
{
  "status": 200,
  "body": {
    "status": "string",
    "target": "string",
    "database": "string",
    "environment": "string"
  }
}
```

Failure response:

```json
{
  "status": 500,
  "body": {
    "status": "string",
    "target": "string",
    "database": "string",
    "environment": "string",
    "error": "string | null"
  }
}
```

#### `GET /health/db/render`

Request:

```json
{
  "auth": "Public",
  "body": null
}
```

Success response:

```json
{
  "status": 200,
  "body": {
    "status": "string",
    "target": "string",
    "database": "string",
    "environment": "string"
  }
}
```

Failure response:

```json
{
  "status": 500,
  "body": {
    "status": "string",
    "target": "string",
    "database": "string",
    "environment": "string",
    "error": "string | null"
  }
}
```

## 8. Lưu ý cuối - TODO để fill

Các mục dưới đây là chỗ đang thiếu hoặc cần sửa code để khớp quyết định đã chốt.

1. Auth:
   - Register đã chốt `201 Created`.
   - Login đã chốt `email + password`.
   - Không thêm admin auth login riêng.
2. Import:
   - Controller chỉ có OCR image upload.
   - Full statement import/status/preview/confirm chưa có.
   - Scope cần fill: ____
3. Secrets/config:
   - `appsettings*.json` trong repo đang có secret thật-looking. Không đưa vào API docs. Nên rotate và chuyển sang env/secret manager.
