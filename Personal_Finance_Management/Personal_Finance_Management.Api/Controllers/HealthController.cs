using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Repository;

namespace Personal_Finance_Management.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public HealthController(AppDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
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
        try
        {
            // hien: khuc nay dung de kiem tra app hien tai co ket noi duoc toi database theo connection string dang cau hinh hay khong
            var canConnect = await _dbContext.Database.CanConnectAsync();

            if (!canConnect)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    status = "Unhealthy",
                    database = "Disconnected",
                    environment = _environment.EnvironmentName
                });
            }

            return Ok(new
            {
                status = "Healthy",
                database = "Connected",
                environment = _environment.EnvironmentName
            });
        }
        catch (Exception ex)
        {
            // hien: khuc nay dung de tra ve loi tong quat khi database khong ket noi duoc ma khong lam lo connection string
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "Unhealthy",
                database = "Disconnected",
                environment = _environment.EnvironmentName,
                error = ex.Message
            });
        }
    }
}
