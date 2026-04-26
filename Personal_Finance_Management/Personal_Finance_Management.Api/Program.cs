using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Api.Middlewares;
using Personal_Finance_Management.Repository;
using authService = Personal_Finance_Management.Service.Auth;
using jwtService = Personal_Finance_Management.Service.JwtService;
using validationService = Personal_Finance_Management.Service.Validations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
// builder.Services.AddJwtServices(builder.Configuration);
// builder.Services.AddSwaggerServices(); mốt thế vào

//add Scope
builder.Services.AddScoped<authService.IService, authService.Service>();
builder.Services.AddScoped<jwtService.IService, jwtService.Service>();
builder.Services.AddScoped<validationService.IServices, validationService.ValidationServices>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
