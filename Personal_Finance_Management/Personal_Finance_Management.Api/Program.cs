using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Api.Extensions;
using Personal_Finance_Management.Api.Jobs;
using Personal_Finance_Management.Api.Middlewares;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Service.Common.Constants;
using Serilog;
using AdminService = Personal_Finance_Management.Service.Admin;
using authService = Personal_Finance_Management.Service.Auth;
using BroadcastService = Personal_Finance_Management.Service.broadcast;
using BankConnectionService = Personal_Finance_Management.Service.BankConnection;
using BankSyncService = Personal_Finance_Management.Service.BankSync;
using SepayService = Personal_Finance_Management.Service.Sepay;
using CategoryService = Personal_Finance_Management.Service.category;
using ImportService = Personal_Finance_Management.Service.import;
using jwtService = Personal_Finance_Management.Service.JwtService;
using OcrService = Personal_Finance_Management.Service.ocr;
using OnboardingService = Personal_Finance_Management.Service.Onboarding;
using UserService = Personal_Finance_Management.Service.User;
using validationService = Personal_Finance_Management.Service.Validations;
using ReminderService = Personal_Finance_Management.Service.Reminder;
using GoalService = Personal_Finance_Management.Service.goal;
using LimitService = Personal_Finance_Management.Service.limit;
using NotificationService = Personal_Finance_Management.Service.notification;
using AIService = Personal_Finance_Management.Service.AI;

using Personal_Finance_Management.Service.Seeding;

using financialAccountService = Personal_Finance_Management.Service.FinancialAccount;
using jarsService = Personal_Finance_Management.Service.Jar;
using transactionService = Personal_Finance_Management.Service.Transaction;
using dashboardService = Personal_Finance_Management.Service.Dashboard;
var builder = WebApplication.CreateBuilder(args);
const string FrontendCorsPolicy = "FrontendCorsPolicy";

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {UserId} {SourceContext} {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        outputTemplate:
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {CorrelationId} {UserId} {SourceContext} {Message:lj}{NewLine}{Exception}"));

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Personal_Finance_Management.Service.Base.ICurrentUserAccessor,
    Personal_Finance_Management.Service.Base.CurrentUserAccessor>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddSwaggerServices();

// hien: khuc nay dung de chon connection string dung cho local hoac hosting truoc khi dang ky DbContext
var databaseConnectionString = builder.GetAppDatabaseConnectionString();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(databaseConnectionString)
        // hien: cai dat de toan bo database dung snake case, neu khong se bi loi khi truy van bang do EF mac dinh se dung PascalCase
        .UseSnakeCaseNamingConvention()
);
builder.Services.AddJwtServices(builder.Configuration);
builder.Services.AddAuthorizationPolicies();
builder.Services.Configure<SeedAccountsOptions>(
    builder.Configuration.GetSection(SeedAccountsOptions.SectionName));


builder.Services.AddScoped<authService.IService, authService.Service>();
builder.Services.AddScoped<jwtService.IService, jwtService.Service>();
builder.Services.AddScoped<validationService.IServices, validationService.ValidationServices>();
builder.Services.AddScoped<OnboardingService.IService, OnboardingService.Service>();
builder.Services.AddScoped<UserService.IService, UserService.Service>();
builder.Services.AddScoped<financialAccountService.IService, financialAccountService.Service>();
builder.Services.AddScoped<jarsService.IService, jarsService.Service>();
builder.Services.AddScoped<transactionService.IService, transactionService.Service>();
builder.Services.AddScoped<dashboardService.IService, dashboardService.Service>();
builder.Services.AddScoped<CategoryService.IService, CategoryService.Service>();
builder.Services.AddScoped<ReminderService.IService, ReminderService.Service>();
builder.Services.AddScoped<GoalService.IService, GoalService.Service>();
builder.Services.AddScoped<LimitService.IService, LimitService.Service>();
builder.Services.AddScoped<LimitService.ISpendingLimitEvaluator, LimitService.SpendingLimitEvaluator>();
builder.Services.AddScoped<NotificationService.IService, NotificationService.Service>();
builder.Services.AddScoped<BroadcastService.IService, BroadcastService.Service>();
builder.Services.AddScoped<AdminService.IService, AdminService.Service>();
builder.Services.AddScoped<AIService.IService, AIService.Service>();
builder.Services.AddScoped<BankConnectionService.IService, BankConnectionService.Service>();
builder.Services.AddScoped<BankSyncService.IService, BankSyncService.Service>();
builder.Services.AddScoped<SepayService.ISepayTokenProtector, SepayService.SepayTokenProtector>();
builder.Services.AddScoped<DatabaseSeedService>();
builder.Services.AddHostedService<BroadcastDispatchBackgroundService>();
builder.Services.AddHttpClient<SepayService.ISepayClient, SepayService.SepayClient>(client =>
{
    var timeoutSeconds = builder.Configuration.GetValue<int?>(ConfigKeys.Sepay.TimeoutSeconds)
                         ?? IntegrationDefaults.SepayTimeoutSeconds;
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
});
builder.Services.AddHttpClient<OcrService.IService, OcrService.Service>(client =>
{
    var timeoutSeconds = builder.Configuration.GetValue<int?>("Ocr:TimeoutSeconds") ?? 120;
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
});
builder.Services.AddScoped<OcrService.IReceiptParserService, OcrService.ReceiptParserService>();
builder.Services.AddScoped<ImportService.IServices, ImportService.Service>();

var app = builder.Build();

// hien: khuc nay dung de tu dong apply database migration khi bien ApplyMigrations duoc bat
app.ApplyDatabaseMigrations();
if (app.Configuration.GetValue<bool>("SeedAccounts:Enabled"))
{
    await app.SeedConfiguredAccountsAsync();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseSerilogRequestLogging();

var enableSwagger = app.Environment.IsDevelopment()
                    || app.Configuration.GetValue<bool>("EnableSwagger");


if (enableSwagger)
{
    app.UseSwaggerAPI();
}

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

