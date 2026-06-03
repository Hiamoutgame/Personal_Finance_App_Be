using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Service.Base;
using Personal_Finance_Management.Service.Common.Constants;
using Personal_Finance_Management.Service.Common.Enums;
using Personal_Finance_Management.Service.Validations;

namespace Personal_Finance_Management.Service.Jar;

public class Service : IService
{
    
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }
    
    public async Task<Response.GetJarsResult> GetJar()
    {
        var userIdGuid = GetCurrentUserId();

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid );
        if (user == null)
            throw new Exception("User not found");
        
        var jarSetup = _dbContext.JarSetups.FirstOrDefault(x => x.UserId == userIdGuid);
        var jars = _dbContext.Jars.Where(x => x.UserId == userIdGuid);

        var totalJarsBalance = _dbContext.Jars.Where(x => x.UserId == userIdGuid).Sum(x => x.Balance);
        var totalAccountsBalance = _dbContext.FinancialAccounts.Where(x => x.UserId == userIdGuid).Sum(x => x.CurrentBalance);
        var selectedQuery = jars.Select(x => new Response.GetJarResponse
        {
            id = x.Id,
            name = x.Name,
            balance = x.Balance,
            color = x.Color,
            icon = x.Icon,
            status = x.Status,
        });
        var result = new Response.GetJarsResult()
        {
            methodType = jarSetup.MethodType,
            totalJarBalance = totalJarsBalance,
            unallocatedBalance = totalAccountsBalance,
            data = selectedQuery.ToList(),
        };
        return result;
    }

    public async Task<Response.CreateJarResponse> CreateJar(Request.CreateJarRequest request)
    {
        var userIdGuid = GetCurrentUserId();

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid );
        if (user == null)
            throw new Exception("User not found");

        var query = _dbContext.Jars.FirstOrDefault(x => x.UserId == userIdGuid && request.name == x.Name);
        if (query != null)
        {
            throw new Exception("Jar already exists");
        }
        var Jars = new Repository.Entity.Jar()
        {
            UserId = userIdGuid,
            Name = request.name,
            Color = request.color,
            Icon = request.icon,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        _dbContext.Jars.Add(Jars);
        await _dbContext.SaveChangesAsync();
        var result = new Response.CreateJarResponse
        {
            id = Jars.Id,
            name = Jars.Name,
            balance = Jars.Balance,
            status = Jars.Status,
        };
        return result;
    }


    public async Task<Response.UpdateJarResponse> UpdateJar(Guid id, Request.UpdateJarRequest request)
    {
        var userIdGuid = GetCurrentUserId();

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);
        if (user == null)
            throw new Exception("User not found");

        var jar = _dbContext.Jars.FirstOrDefault(x => x.Id == id && x.UserId == userIdGuid);
        if (jar == null)
        {
            throw AppValidationException.NotFound("Jar not found.", "id", ErrorCodes.JarNotFound);
        }

        jar.Name = request.name ?? jar.Name;
        jar.Color = request.color ?? jar.Color;
        jar.Icon = request.icon ?? jar.Icon;
        await _dbContext.SaveChangesAsync();

        var result = new Response.UpdateJarResponse
        {
            id = jar.Id,
            name = jar.Name,
            color = jar.Color,
            icon = jar.Icon,
            status = jar.Status,
        };
        return result;
    }

    public async Task<Response.DeleteJarResponse> DeleteJar(Guid id)
    {
        var userIdGuid = GetCurrentUserId();

        var user = await _dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);
        if (user == null)
            throw new Exception("User not found");
        var jar = _dbContext.Jars.FirstOrDefault(x => x.Id == id && x.UserId == userIdGuid);
        if (jar == null)
        {
            throw AppValidationException.NotFound("Jar not found.", "id", ErrorCodes.JarNotFound);
        }

        if (jar.Balance != 0)
        {
            throw AppValidationException.BadRequest(
                "Jar balance must be 0.",
                "balance",
                "JAR_BALANCE_MUST_BE_ZERO");
        }

        jar.Status = JarStatus.Archived;
        await _dbContext.SaveChangesAsync();
        var result = new Response.DeleteJarResponse
        {
            message = "Jar deleted"
        };
        return result;
    }

    private Guid GetCurrentUserId()
    {
        return ServiceClaimHelper.GetRequiredUserId(_httpContext);
    }
}
