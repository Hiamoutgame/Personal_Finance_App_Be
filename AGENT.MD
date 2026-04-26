# AGENT CONTEXT - PERSONAL FINANCE MANAGEMENT

Tài liệu này giúp người mới vào dự án nắm nhanh bối cảnh kỹ thuật của source Personal_Finance_Management: cấu trúc folder, luồng chạy, và các design pattern (đặc biệt là DI) đang dùng.

Lưu ý: trạng thái code hiện tại đang ở mức nền tảng (scaffold), nhiều phần nghiệp vụ chính chưa triển khai đầy đủ.

## 1) Tổng Quan Kiến Trúc

Solution hiện có 3 project chính:

- Personal_Finance_Management.Api
- Personal_Finance_Management.Service
- Personal_Finance_Management.Repository

Dependency hiện tại:

- Api tham chiếu Service và Repository
- Service tham chiếu Repository
- Repository là tầng dữ liệu độc lập (Entity + DbContext + Migration)

Ý nghĩa kiến trúc:

- Api chịu trách nhiệm HTTP layer, routing, middleware, bootstrap ứng dụng.
- Service chịu trách nhiệm business logic, xử lý nghiệp vụ, tổ chức flow theo use case.
- Repository chịu trách nhiệm persistence model với Entity Framework Core và PostgreSQL.

## 2) Mô Tả Chi Tiết Các Folder

## 2.1) Gốc Solution: Personal_Finance_Management

- Personal_Finance_Management.sln
  - Điểm vào ở cấp solution, gom 3 project Api/Service/Repository.

## 2.2) Project Api: Personal_Finance_Management.Api

- Controllers
  - Nơi đặt API endpoints.
  - Hiện tại mới có WeatherForecastController (controller mẫu từ template).

- Extensions
  - Dùng để tách logic cấu hình service/pipeline khỏi Program.cs.
  - Có sẵn JwtExtension và SwaggerExtension nhưng đang để trống.

- Middlewares
  - Nơi đặt custom middleware cross-cutting (ví dụ global exception handling).
  - GlobalExceptionHandlerMiddleware đã tạo file nhưng chưa có implementation.

- Properties
  - Chứa cấu hình launch/debug (launchSettings.json).

- appsettings.json, appsettings.Development.json
  - Chứa cấu hình runtime: connection string, JWT options, logging.

- Program.cs
  - Entry point chính để bootstrap DI container và request pipeline.

## 2.3) Project Service: Personal_Finance_Management.Service

- Auth
  - Chứa contract và xử lý nghiệp vụ auth theo use case.
  - Bao gồm:
    - IService: interface cho auth use case.
    - Request: DTO input.
    - Response: DTO output.
    - Service: implementation auth service.

- JwtService
  - Chứa logic phát hành và validate JWT token.
  - Bao gồm:
    - IService: contract GenerateAccessToken, ValidateToken.
    - Service: implementation thao tác JWT.
    - JwtOption: class map cấu hình JwtOptions từ appsettings.

- Class1.cs
  - File scaffold mặc định, chưa có giá trị nghiệp vụ.

## 2.4) Project Repository: Personal_Finance_Management.Repository

- Abtraction
  - BaseEntity<TKey>: base class chung cho entities (Id, IsDeleted).
  - IAudictableEntity: hợp đồng audit field CreatedAt/UpdatedAt/ReadAt.
  - Lưu ý chính tả folder và interface hiện tại là Abtraction / IAudictableEntity.

- Entity
  - Chứa các domain entity map vào database tables.
  - Đây là trung tâm của domain model hiện tại.

- Migrations
  - Chứa migration sinh từ EF Core (Initial + ModelSnapshot).
  - Dùng để đồng bộ schema PostgreSQL theo code-first.

- AppDbContext.cs
  - Khai báo DbSet cho toàn bộ domain.
  - Cấu hình quan hệ, kiểu dữ liệu, index, default value, delete behavior trong OnModelCreating.

- Class1.cs
  - File scaffold mặc định, chưa dùng.

## 3) Domain Model Đang Có Trong Repository

Hiện tại hệ thống đã dựng khá đầy đủ khung domain theo MVP tài chính cá nhân.

## 3.1) Identity và Access

- Role
- Account
- AuditLog

## 3.2) Onboarding và Thiết Lập Ban Đầu

- OnboardingProfile
- JarSetup
- FinancialAccount
- Category

## 3.3) Quản Lý Hũ và Ngân Sách

- Jar
- JarAllocation
- JarAllocationItem
- JarTransfer

## 3.4) Giao Dịch và Import Sao Kê

- Transaction
- ImportJob
- ImportTransactionDraft

## 3.5) Mục Tiêu, Hạn Mức, Nhắc Nhở, Thông Báo

- SpendingLimit
- Goal
- GoalContribution
- Reminder
- Notification

## 3.6) Quản Trị Hệ Thống

- Broadcast
- AiSetting

## 4) Mô Tả Luồng Đi (Request Flow + Data Flow)

## 4.1) Luồng Runtime Khi Khởi Động Ứng Dụng

Theo Program.cs hiện tại:

1. Khởi tạo builder.
2. Đăng ký Controllers, EndpointsApiExplorer, Swagger.
3. Đăng ký AppDbContext với Npgsql qua connection string DefaultConnection.
4. Đăng ký DI cho JwtService.IService theo lifetime Scoped.
5. Build app.
6. Nếu môi trường Development thì bật Swagger UI.
7. Bật Authorization middleware.
8. MapControllers và chạy app.

Ý nghĩa:

- AppDbContext được tạo theo scope request.
- Mỗi request sẽ dùng một DbContext scope riêng, phù hợp mô hình web API.

## 4.2) Luồng Request Chuẩn (Mục Tiêu Kiến Trúc)

Khi triển khai nghiệp vụ thật, luồng nên đi theo thứ tự:

1. Controller nhận request DTO từ client.
2. Controller gọi service interface tương ứng.
3. Service xử lý business rule, validate nghiệp vụ.
4. Service thao tác AppDbContext để đọc/ghi dữ liệu.
5. SaveChanges trong cùng scope request (hoặc transaction khi cần atomic).
6. Service trả response DTO cho controller.
7. Controller trả HTTP response về client.

## 4.3) Luồng Dữ Liệu Tài Chính Trọng Tâm (Theo Domain)

- Luồng ghi giao dịch:
  - Transaction bắt buộc gắn với User và FinancialAccount.
  - Có thể gắn thêm Jar, Category, ImportJob (nullable).

- Luồng import sao kê:
  - ImportJob tạo phiên import.
  - ImportTransactionDraft lưu các dòng parse tạm.
  - Sau xác nhận sẽ tạo Transaction chính thức.

- Luồng hũ ngân sách:
  - JarAllocation là lần phân bổ tổng.
  - JarAllocationItem là chi tiết phân bổ theo từng Jar.
  - JarTransfer là điều chuyển ngân sách giữa 2 Jar.

- Luồng mục tiêu tài chính:
  - Goal là mục tiêu tổng.
  - GoalContribution là từng lần đóng góp từ Jar hoặc FinancialAccount.

## 4.4) Các Ràng Buộc Đáng Chú Ý Từ Migration

- Unique:
  - accounts.Username
  - accounts.Email
  - roles.Code
  - onboarding_profiles.UserId (1-1)
  - jar_setups.UserId (1-1)
  - import_transaction_drafts (ImportJobId, RowIndex)

- Index hiệu năng:
  - transactions theo (UserId, TransactionDate)
  - notifications theo (UserId, IsRead)

- Delete behavior được cấu hình theo ngữ cảnh:
  - Restrict: bảo toàn dữ liệu lịch sử/log hoặc tránh cascade không mong muốn.
  - Cascade: xóa cha kéo theo con khi quan hệ ownership rõ ràng.
  - SetNull: giữ bản ghi con nhưng bỏ liên kết khi cha bị xóa.

## 5) Design Pattern Đang Áp Dụng (Trọng Tâm DI)

## 5.1) Layered Architecture (Phân Tầng)

- Presentation layer: Api
- Application/Business layer: Service
- Data access layer: Repository

Lợi ích:

- Dễ chia việc theo team.
- Dễ thay đổi từng tầng mà ít ảnh hưởng tầng khác.
- Dễ test theo scope từng tầng.

## 5.2) Dependency Injection (DI)

DI container mặc định của ASP.NET Core đang được dùng.

Hiện có đăng ký:

- AddDbContext<AppDbContext>(...): inject DbContext theo request scope.
- AddScoped<JwtService.IService, JwtService.Service>(): inject JWT service qua interface.

Ý nghĩa DI trong dự án này:

- Controller không new trực tiếp service.
- Service không new trực tiếp dependency hạ tầng.
- Dễ mock khi viết unit test/integration test.

## 5.3) Interface-based Service Pattern

Trong Service đang dùng pattern interface + implementation:

- Auth.IService -> Auth.Service
- JwtService.IService -> JwtService.Service

Mục đích:

- Tách contract khỏi implementation.
- Hỗ trợ test/mocking và mở rộng implementation theo môi trường.

## 5.4) Options Pattern (Configuration Binding)

JwtService.Service bind cấu hình JwtOptions từ appsettings.

Ý nghĩa:

- Tách cấu hình runtime khỏi code.
- Dễ đổi theo môi trường Dev/Prod.

## 5.5) Unit of Work Tự Nhiên Qua DbContext

AppDbContext đóng vai trò điểm gom thay đổi dữ liệu trong một scope.
Khi SaveChanges, EF Core thực hiện commit theo transaction nội bộ phù hợp.

## 5.6) Extension Method Pattern (Đang Chuẩn Bị)

Đã có folder Extensions để tách logic cấu hình như JWT/Swagger khỏi Program.cs.
Hiện file extension chưa triển khai, nhưng hướng tổ chức đã đúng cho maintainability.

## 5.7) Middleware Pattern (Đang Chuẩn Bị)

Đã có chỗ cho GlobalExceptionHandlerMiddleware.
Khi hoàn thiện, middleware này sẽ tập trung hóa xử lý lỗi và chuẩn hóa error response.

## 6) Trạng Thái Hiện Tại Và Hướng Áp Dụng Khi Code Mới

## 6.1) Trạng Thái Hiện Tại

- Khung kiến trúc và schema đã khá đầy đủ.
- Nhiều phần nghiệp vụ vẫn đang scaffold.
- Controller thật cho domain chưa được tạo (hiện chủ yếu là WeatherForecast mẫu).
- Extension/Middleware chưa được wiring vào pipeline.

## 6.2) Quy Ước Nên Giữ Khi Mở Rộng

Khi thêm một module mới, nên theo checklist:

1. Tạo request/response DTO ở Service theo use case.
2. Tạo service interface + implementation cho nghiệp vụ.
3. Inject service vào controller qua constructor.
4. Chỉ thao tác AppDbContext trong service, không để controller truy cập trực tiếp.
5. Nếu có nhiều bước cập nhật số dư/hũ/giao dịch, dùng transaction để đảm bảo atomic.
6. Đồng bộ migration mỗi khi thay đổi entity mapping.
7. Chuẩn hóa lỗi qua middleware toàn cục (khi middleware hoàn thiện).

## 7) Tóm Tắt Onboarding Trong 5 Phút

- Đây là backend .NET 8 theo mô hình 3 tầng Api-Service-Repository.
- Repository đang là phần mạnh nhất: domain entity + migration + quan hệ DB đã dựng chi tiết.
- Service có khung Auth/JWT, DI cơ bản đã hoạt động.
- Api mới ở mức nền, cần triển khai controller và wiring middleware/extensions.
- Nếu follow đúng DI + phân tầng hiện tại, dự án sẽ mở rộng tính năng nhanh và sạch hơn.
