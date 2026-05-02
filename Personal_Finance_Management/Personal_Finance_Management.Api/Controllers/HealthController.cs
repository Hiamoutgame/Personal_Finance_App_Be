using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Repository;

namespace Personal_Finance_Management.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public HealthController(
        AppDbContext dbContext,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _environment = environment;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy"
        });
    }

    [HttpGet("db")]
    public async Task<IActionResult> GetDatabaseStatus()
    {
        return await GetCurrentDatabaseStatus();
    }

    [HttpGet("db/current")]
    public async Task<IActionResult> GetCurrentDatabaseStatus()
    {
        try
        {
            // hien: khuc nay dung de kiem tra app hien tai co ket noi duoc toi database theo connection string dang cau hinh hay khong
            var canConnect = await _dbContext.Database.CanConnectAsync();

            if (!canConnect)
            {
                return DatabaseUnavailable("Current", null);
            }

            return DatabaseAvailable("Current");
        }
        catch (Exception ex)
        {
            // hien: khuc nay dung de tra ve loi tong quat khi database khong ket noi duoc ma khong lam lo connection string
            return DatabaseUnavailable("Current", ex.Message);
        }
    }

    [HttpGet("db/local")]
    public async Task<IActionResult> GetLocalDatabaseStatus()
    {
        // hien: khuc nay dung de lay connection string danh rieng cho database local khi can test ro local db
        var connectionString = _configuration.GetConnectionString("LocalConnection")
                               ?? (_environment.IsDevelopment()
                                   ? _configuration.GetConnectionString("DefaultConnection")
                                   : null);

        return await CheckDatabaseConnection("Local", connectionString);
    }

    [HttpGet("db/render")]
    public async Task<IActionResult> GetRenderDatabaseStatus()
    {
        // hien: khuc nay dung de lay connection string danh rieng cho database Render khi can test ro hosted db
        var connectionString = _configuration.GetConnectionString("RenderConnection")
                               ?? (_environment.IsProduction()
                                   ? _configuration.GetConnectionString("DefaultConnection")
                                   : null);

        return await CheckDatabaseConnection("Render", connectionString);
    }

    private async Task<IActionResult> CheckDatabaseConnection(string target, string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // hien: khuc nay dung de bao ro rang la chua cau hinh connection string cho loai database dang check
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "Unhealthy",
                target,
                database = "NotConfigured",
                environment = _environment.EnvironmentName
            });
        }

        try
        {
            // hien: khuc nay dung de tao DbContext tam thoi voi connection string cua tung loai database can check
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            await using var dbContext = new AppDbContext(options);
            var canConnect = await dbContext.Database.CanConnectAsync();

            if (!canConnect)
            {
                return DatabaseUnavailable(target, null);
            }

            return DatabaseAvailable(target);
        }
        catch (Exception ex)
        {
            // hien: khuc nay dung de tra ve loi ket noi cua tung loai database ma khong tra ve connection string
            return DatabaseUnavailable(target, ex.Message);
        }
    }

    private OkObjectResult DatabaseAvailable(string target)
    {
        return Ok(new
        {
            status = "Healthy",
            target,
            database = "Connected",
            environment = _environment.EnvironmentName
        });
    }

    private ObjectResult DatabaseUnavailable(string target, string? error)
    {
        return StatusCode(StatusCodes.Status503ServiceUnavailable, new
        {
            status = "Unhealthy",
            target,
            database = "Disconnected",
            environment = _environment.EnvironmentName,
            error
        });
    }
}
