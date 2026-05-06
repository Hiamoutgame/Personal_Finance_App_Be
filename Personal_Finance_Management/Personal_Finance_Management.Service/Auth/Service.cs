using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Entity;
using Personal_Finance_Management.Repository.Enum;
using ValidationService = Personal_Finance_Management.Service.Validations;
using JwtService = Personal_Finance_Management.Service.JwtService;

namespace Personal_Finance_Management.Service.Auth;

public class Service : IService
{
    private static readonly Guid DefaultRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string DefaultRoleCode = "User";

    private readonly AppDbContext _dbContext;
    private readonly JwtService.IService _jwtService;
    private readonly ValidationService.IServices _validationServices;

    public Service(
        AppDbContext dbContext,
        JwtService.IService jwtService,
        ValidationService.IServices validationServices)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _validationServices = validationServices;
    }

    public async Task<Response.RegisterResponse> Register(Request.RegisterRequest request)
    {
        await _validationServices.ValidateRegisterRequest(request);

        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();

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
            new Claim("id", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("username", user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName),
            new Claim(ClaimTypes.Role, role.Code)
        });

        return new Response.RegisterResponse
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
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
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Accounts
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new Exception("Invalid email or password.");
        }

        var token = _jwtService.GenerateAccessToken(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("id", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("username", user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName),
            new Claim(ClaimTypes.Role, user.Role.Code)
        });

        return await Task.FromResult(new Response.LoginResponse
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            AccessToken = token
        });
    }
}
