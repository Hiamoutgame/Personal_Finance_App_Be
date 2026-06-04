using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Constants;
using Personal_Finance_Management.Repository.Entity;
using Personal_Finance_Management.Repository.Enum;
using Personal_Finance_Management.Service.Base;
using Personal_Finance_Management.Service.Common.Constants;
using ValidationService = Personal_Finance_Management.Service.Validations;
using JwtService = Personal_Finance_Management.Service.JwtService;

namespace Personal_Finance_Management.Service.Auth;

public class Service : IService
{
    private static readonly Guid DefaultRoleId = AppRoles.Ids.User;
    private static readonly string DefaultRoleCode = AppRoles.Codes.User;

    private readonly AppDbContext _dbContext;
    private readonly JwtService.IService _jwtService;
    private readonly ValidationService.IServices _validationServices;
    private readonly IHttpContextAccessor _httpContext;

    public Service(
        AppDbContext dbContext,
        JwtService.IService jwtService,
        ValidationService.IServices validationServices,
        IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _validationServices = validationServices;
        _httpContext = httpContext;
    }

    public async Task<Response.RegisterResponse> Register(Request.RegisterRequest request)
    {
        await _validationServices.ValidateRegisterRequest(request);

        var username = ServiceTextHelper.NormalizeRequiredText(request.Username, "username", "Username is required.");
        var email = request.Email.Trim().ToLowerInvariant();
        var firstName = ServiceTextHelper.NormalizeRequiredText(request.FirstName, "firstName", "First name is required.");
        var lastName = ServiceTextHelper.NormalizeRequiredText(request.LastName, "lastName", "Last name is required.");

        var now = DateTimeOffset.UtcNow;
        var role = await EnsureUserRole(now);

        var user = new Account
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12),
            FirstName = firstName,
            LastName = lastName,
            RoleId = role.Id,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Accounts.Add(user);
        await _dbContext.SaveChangesAsync();

        var token = _jwtService.GenerateAccessToken(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(AppClaimTypes.Id, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(AppClaimTypes.Username, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(AppClaimTypes.FirstName, user.FirstName),
            new Claim(AppClaimTypes.LastName, user.LastName),
            new Claim(AppClaimTypes.IsOnboardingCompleted, user.IsOnboardingCompleted ? "true" : "false"),
            new Claim(ClaimTypes.Role, role.Code)
        });

        return new Response.RegisterResponse
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = role.Code,
            IsOnboardingCompleted = user.IsOnboardingCompleted,
            AccessToken = token
        };
    }

    private async Task<Role> EnsureUserRole(DateTimeOffset now)
    {
        var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Code == DefaultRoleCode);
        if (role is not null)
        {
            return role;
        }

        role = new Role
        {
            Id = DefaultRoleId,
            Code = DefaultRoleCode,
            Name = DefaultRoleCode,
            Description = "Default application user",
            CreatedAt = now
        };

        _dbContext.Roles.Add(role);
        return role;
    }

    public async Task<Response.LoginResponse> Login(Request.LoginRequest request)
    {
        await _validationServices.ValidateLoginRequest(request);

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Accounts
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw ValidationService.AppValidationException.Unauthorized(
                ErrorMessages.InvalidLoginCredentials,
                ErrorCodes.InvalidLoginCredentials);
        }

        var token = _jwtService.GenerateAccessToken(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(AppClaimTypes.Id, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(AppClaimTypes.Username, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(AppClaimTypes.FirstName, user.FirstName),
            new Claim(AppClaimTypes.LastName, user.LastName),
            new Claim(AppClaimTypes.IsOnboardingCompleted, user.IsOnboardingCompleted ? "true" : "false"),
            new Claim(ClaimTypes.Role, user.Role.Code)
        });
        
        var hasLimitReset = false;
        var activeLimit = _dbContext.SpendingLimits.Where(l => l.UserId == user.Id && l.IsActive == true);
        foreach (var limit in activeLimit)
        {
            if (limit.Period == "Daily")
            {
                if(DateTimeOffset.Now - limit.ResetAt >= TimeSpan.FromDays(1))
                {
                    limit.ResetAt = DateTimeOffset.UtcNow;
                    hasLimitReset = true;
                }
            }else if (limit.Period == "Monthly")
            {
                if (DateTimeOffset.Now - limit.ResetAt >= TimeSpan.FromDays(30))
                {
                    limit.ResetAt = DateTimeOffset.UtcNow;
                    hasLimitReset = true;
                }
            }
        }
        if (hasLimitReset)
        {
            await _dbContext.SaveChangesAsync();
        }
        return await Task.FromResult(new Response.LoginResponse
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.Code,
            IsOnboardingCompleted = user.IsOnboardingCompleted,
            AccessToken = token
        });
    }

    public async Task<string> Logout()
    {
        ServiceClaimHelper.GetRequiredAccountId(_httpContext, "Invalid user id.");
        return await Task.FromResult("Logout successful.");
    }
}

