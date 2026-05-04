using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;

namespace Personal_Finance_Management.Api.Extensions;

public static class DatabaseMigrationExtension
{
    public static string GetAppDatabaseConnectionString(this WebApplicationBuilder builder)
    {
        // hien: khuc nay dung de xac dinh app dang chay local hay hosting dua tren environment
        var connectionName = builder.Environment.IsDevelopment()
            ? "DefaultConnection"
            : "RenderConnection";

        // hien: khuc nay dung de lay connection string dung voi moi truong hien tai
        var connectionString = builder.Configuration.GetConnectionString(connectionName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Missing database connection string '{connectionName}' for environment '{builder.Environment.EnvironmentName}'."
            );
        }
        return connectionString;
    }

    public static void ApplyDatabaseMigrations(this WebApplication app)
    {
        // hien: khuc nay dung de kiem tra co cho phep chay migration tu config hay khong
        if (!app.Configuration.GetValue<bool>("ApplyMigrations"))
        {
            return;
        }

        // hien: khuc nay dung de tao scope rieng de lay AppDbContext tu dependency injection
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // hien: khuc nay dung de apply cac EF Core migration con thieu vao database hien tai
        dbContext.Database.Migrate();
    }
}
