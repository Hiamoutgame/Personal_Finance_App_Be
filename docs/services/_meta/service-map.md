# Bản đồ Dịch vụ (Service Map)

Bảng này ánh xạ các endpoint docs với controller/service/DTO/entity để sub-agent tìm đúng file trước khi sửa.

| Module | Routes | Controller | Service | DTO | Entity/schema |
| --- | --- | --- | --- | --- | --- |
| Auth | `/api/v1/auth/*` | `Api/Controllers/AuthController.cs` | `Service/Auth/IService.cs`, `Service/Auth/Service.cs` | `Service/Auth/Request.cs`, `Service/Auth/Response.cs` | `Account`, `Role` |
| User | `/api/v1/user/me*` | `Api/Controllers/UserController.cs` | `Service/User/IService.cs`, `Service/User/Service.cs` | `Service/User/Request.cs`, `Service/User/Response.cs` | `Account`, `OnboardingProfile`, `FinancialAccount`, `Jar`, `Goal`, `SpendingLimit` |
| Onboarding | `/api/v1/onboarding` | `Api/Controllers/OnboardingController.cs` | `Service/Onboarding/IService.cs`, `Service/Onboarding/Service.cs` | `Service/Onboarding/Request.cs`, `Service/Onboarding/Response.cs` | `OnboardingProfile`, `FinancialAccount`, `JarSetup`, `Jar` |
| Financial Accounts | `/api/v1/financial-accounts*` | `Api/Controllers/FinancialAccountController.cs` | `Service/FinancialAccount/*`, `Service/BankConnection/*`, `Service/BankSync/*` | Tương ứng `Request.cs`/`Response.cs` | `FinancialAccount`, `BankConnectionSession`, `Transaction` |
| Categories | `/api/v1/categories*`, `/api/v1/admin/categories*` | `CategoryController.cs`, `AdminCategoryController.cs` | `Service/category/*` | `Service/category/Request.cs`, `Response.cs` | `Category` |
| Jars | `/api/v1/jars*` | `Api/Controllers/JarController.cs` | `Service/Jar/*` | `Service/Jar/Request.cs`, `Response.cs` | `Jar`, `JarSetup`, `Transaction` |
| Transactions | `/api/v1/transactions*` | `Api/Controllers/TransactionsController.cs` | `Service/Transaction/*` | `Service/Transaction/Request.cs`, `Response.cs` | `Transaction`, `FinancialAccount`, `Jar`, `Category`, `Notification` |
| Imports/OCR | `/api/v1/imports*` | `Api/Controllers/ImportController.cs` | `Service/import/*`, `Service/ocr/*` | `Service/import/Request.cs`, `Response.cs` | `ImportJob`, `ImportTransactionDraft`, `Transaction` |
| Dashboard | `/api/v1/dashboard` | `Api/Controllers/DashboardController.cs` | `Service/Dashboard/*` | `Service/Dashboard/Request.cs`, `Response.cs` | Tích hợp từ tài khoản/hũ/giao dịch/mục tiêu/danh mục |
| AI Chat | `/api/v1/ai/chat` | `Api/Controllers/AIChatController.cs` | `Service/ai/*` | `Service/ai/Request.cs`, `Response.cs` | `AiSetting`, dữ liệu tài chính của người dùng |
| Limits | `/api/v1/limits*` | `Api/Controllers/LimitController.cs` | `Service/Limit/*` | `Service/Limit/Request.cs`, `Response.cs` | `SpendingLimit`, `Jar`, `Category`, `Transaction`, `Notification` |
| Notifications | `/api/v1/notifications*` | `Api/Controllers/NotificationController.cs` | `Service/Notification/*` | `Service/Notification/Request.cs`, `Response.cs` | `Notification` |
| Goals | `/api/v1/goals*` | `Api/Controllers/GoalController.cs` | `Service/Goal/*` | `Service/Goal/Request.cs`, `Response.cs` | `Goal`, `Jar` |
| Reminders | `/api/v1/reminders*` | `Api/Controllers/ReminderController.cs` | `Service/Reminder/*` | `Service/Reminder/Request.cs`, `Response.cs` | `Reminder`, `Category`, `Notification` |
| Admin Users | `/api/v1/admin/users*`, `/api/v1/change-role/*` | `AdminUserController.cs`, `AdminChangeRoleController.cs` | `Service/User/*`, `Service/admin/*` | `Service/User/Request.cs`, `Response.cs`, `Service/admin/*` | `Account`, `Role`, `AuditLog` |
| Admin Dashboard/Audit/AI | `/api/v1/admin/dashboard`, `/api/v1/admin/audit-logs`, `/api/v1/admin/ai-settings` | Các Admin controllers | `Service/admin/*`, `Service/ai/*` | Các DTO files tương ứng | `AuditLog`, `AiSetting`, dữ liệu tích hợp |
| Broadcasts | `/api/v1/admin/broadcasts*` | `Api/Controllers/AdminBroadcastController.cs` | `Service/broadcast/*`, background dispatcher | `Service/broadcast/Request.cs`, `Response.cs` | `Broadcast`, `Notification` |
| Health | `/health*` | `Api/Controllers/HealthController.cs` | Controller trực tiếp + kiểm tra DbContext | Không có cấu trúc cụ thể | Chỉ kiểm tra kết nối DB |

## Các tệp cắt ngang (Cross-cutting files)

- Auth policy/claims: `Api/Extensions/AuthorizationExtension.cs`, `Service/Common/Constants/AppClaimTypes.cs`, `Policies.cs`.
- Ngoại lệ (Exceptions): `Api/Middlewares/GlobalExceptionHandlerMiddleware.cs`, `Service/Validations/AppValidationException.cs`.
- Hỗ trợ người dùng hiện tại: `Service/Base/CurrentUserAccessor.cs`, `Service/Base/ServiceClaimHelper.cs`.
- Phân trang (Pagination): `Service/Base/PagedResult.cs`, mã cũ `Service/baseServices/Page.cs`.
- Các hằng số/khóa cấu hình: `Service/Common/Constants/*`, `Service/Common/Enums/*`.
