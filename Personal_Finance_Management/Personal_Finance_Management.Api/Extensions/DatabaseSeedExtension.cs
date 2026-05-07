using Personal_Finance_Management.Service.Seeding;

namespace Personal_Finance_Management.Api.Extensions;

public static class DatabaseSeedExtension
{
    public static async Task SeedConfiguredAccountsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var seedService = scope.ServiceProvider.GetRequiredService<DatabaseSeedService>();
        await seedService.SeedAsync();
    }
}
