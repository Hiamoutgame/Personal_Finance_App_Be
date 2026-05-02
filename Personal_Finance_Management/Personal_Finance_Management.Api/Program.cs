using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Api.Extensions;
using Personal_Finance_Management.Api.Middlewares;
using Personal_Finance_Management.Repository;
using authService = Personal_Finance_Management.Service.Auth;
using jwtService = Personal_Finance_Management.Service.JwtService;
using OnboardingService = Personal_Finance_Management.Service.Onboarding;
using UserService = Personal_Finance_Management.Service.User;
using validationService = Personal_Finance_Management.Service.Validations;

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
    options.UseNpgsql(
        databaseConnectionString
    )
);
builder.Services.AddJwtServices(builder.Configuration);

builder.Services.AddScoped<authService.IService, authService.Service>();
builder.Services.AddScoped<jwtService.IService, jwtService.Service>();
builder.Services.AddScoped<validationService.IServices, validationService.ValidationServices>();
builder.Services.AddScoped<OnboardingService.IService, OnboardingService.Service>();
builder.Services.AddScoped<UserService.IService, UserService.Service>();

var app = builder.Build();

// hien: khuc nay dung de tu dong apply database migration khi bien ApplyMigrations duoc bat
app.ApplyDatabaseMigrations();

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
