# Test Commands

## Prerequisites
```bash
# 1. Build solution
dotnet build Personal_Finance_Management.sln

# 2. Install Playwright (chỉ cần 1 lần)
pwsh Personal_Finance_Management.Api.Tests/bin/Debug/net8.0/playwright.ps1 install
```

## Run all tests
```bash
dotnet test Personal_Finance_Management.sln
```

## Run by category / class

```bash
# Auth (9 tests)
dotnet test --filter "FullyQualifiedName~AuthTests"

# Dashboard (3 tests)
dotnet test --filter "FullyQualifiedName~DashboardControllerTests"

# Financial Account (7 tests)
dotnet test --filter "FullyQualifiedName~FinancialAccountControllerTests"

# Transactions (9 tests)
dotnet test --filter "FullyQualifiedName~TransactionsControllerTests"

# Goals (6 tests)
dotnet test --filter "FullyQualifiedName~GoalControllerTests"

# Jars (5 tests)
dotnet test --filter "FullyQualifiedName~JarControllerTests"

# Limits (6 tests)
dotnet test --filter "FullyQualifiedName~LimitControllerTests"

# Reminders (6 tests)
dotnet test --filter "FullyQualifiedName~ReminderControllerTests"

# Categories (7 tests)
dotnet test --filter "FullyQualifiedName~CategoryControllerTests"

# Admin Categories (6 tests)
dotnet test --filter "FullyQualifiedName~AdminCategoryControllerTests"
```

## Run by test name
```bash
# All tests containing "Login"
dotnet test --filter "Name~Login"

# All tests containing "Create"
dotnet test --filter "Name~Create"

# All tests containing "Delete"
dotnet test --filter "Name~Delete"

# All tests containing "Returns401" (unauthorized tests)
dotnet test --filter "Name~Returns401"
```

## Combined filters
```bash
# Run Auth AND Transaction tests
dotnet test --filter "FullyQualifiedName~AuthTests | FullyQualifiedName~TransactionsControllerTests"

# Run all CRUD create tests (multiple classes)
dotnet test --filter "Name~Returns201 | Name~WithValidData_Returns200"

# Exclude a specific class
dotnet test --filter "FullyQualifiedName~Api.Tests & FullyQualifiedName~AuthTests"

# Run everything EXCEPT Auth
dotnet test --filter "FullyQualifiedName~Api.Tests & FullyQualifiedName~AuthTests"
```

## Run single test method
```bash
dotnet test --filter "FullyQualifiedName~Login_WithValidCredentials_ReturnsTokenAndUserInfo"
```

## Run with verbose output
```bash
dotnet test -v n  # normal
dotnet test -v d  # detailed (including stdout from tests)
```

## Run with logger
```bash
dotnet test --logger "console;verbosity=detailed"
dotnet test --logger "trx;LogFileName=test-results.trx"
```

## Run with coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Run specific project (not whole solution)
```bash
dotnet test Personal_Finance_Management.Api.Tests/Personal_Finance_Management.Api.Tests.csproj
```

## List all tests
```bash
dotnet test --list-tests
dotnet test --list-tests --filter "FullyQualifiedName~AuthTests"
```
