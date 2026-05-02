using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;

namespace Personal_Finance_Management.Api.Extensions;

public static class DatabaseMigrationExtension
{
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
