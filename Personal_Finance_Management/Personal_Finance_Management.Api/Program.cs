using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Api.Extensions;
using Personal_Finance_Management.Api.Middlewares;
using Personal_Finance_Management.Repository;
using AdminService = Personal_Finance_Management.Service.Admin;
using authService = Personal_Finance_Management.Service.Auth;
using BroadcastService = Personal_Finance_Management.Service.broadcast;
using CategoryService = Personal_Finance_Management.Service.category;
using ImportService = Personal_Finance_Management.Service.import;
using jwtService = Personal_Finance_Management.Service.JwtService;
using OcrService = Personal_Finance_Management.Service.ocr;
using OnboardingService = Personal_Finance_Management.Service.Onboarding;
using UserService = Personal_Finance_Management.Service.User;
using validationService = Personal_Finance_Management.Service.Validations;
using ReminderService =  Personal_Finance_Management.Service.Reminder;
using AIService = Personal_Finance_Management.Service.AI;
using Personal_Finance_Management.Service.Seeding;

using financialAccountService = Personal_Finance_Management.Service.FinancialAccount;
using jarsService = Personal_Finance_Management.Service.Jar;
using transactionService = Personal_Finance_Management.Service.Transaction;
using dashboardService = Personal_Finance_Management.Service.Dashboard;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
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
builder.Services.AddScoped<BroadcastService.IService, BroadcastService.Service>();
builder.Services.AddScoped<AdminService.IService, AdminService.Service>();
builder.Services.AddScoped<AIService.IService, AIService.Service>();
builder.Services.AddScoped<DatabaseSeedService>();
builder.Services.AddHttpClient<OcrService.IService, OcrService.Service>(client =>
{
    var timeoutSeconds = builder.Configuration.GetValue<int?>("Ocr:TimeoutSeconds") ?? 120;
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
});
builder.Services.AddScoped<ImportService.IServices, ImportService.Service>();

var app = builder.Build();

// hien: khuc nay dung de tu dong apply database migration khi bien ApplyMigrations duoc bat
app.ApplyDatabaseMigrations();
if (app.Configuration.GetValue<bool>("SeedAccounts:Enabled"))
{
    await app.SeedConfiguredAccountsAsync();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

var enableSwagger = app.Environment.IsDevelopment()
                    || app.Configuration.GetValue<bool>("EnableSwagger");


if (enableSwagger)
{
    app.UseSwaggerAPI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
