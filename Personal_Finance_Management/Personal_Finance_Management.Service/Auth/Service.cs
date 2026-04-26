using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Entity;
using ValidationService = Personal_Finance_Management.Service.Validations;
using JwtService = Personal_Finance_Management.Service.JwtService;

namespace Personal_Finance_Management.Service.Auth;

public class Service : IService
{
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
        var fullName = string.IsNullOrWhiteSpace(request.FullName)
            ? username
            : request.FullName.Trim();

        var now = DateTimeOffset.UtcNow;
        var role = await EnsureUserRole(now);
        var (firstName, lastName) = SplitFullName(fullName);

        var user = new Account
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            HashPassword = BCrypt.Net.BCrypt.HashPassword(request.Password, 12),
            FirstName = firstName,
            LastName = lastName,
            RoleId = role.Id,
            CreatedAt = now
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
            new Claim("fullName", fullName),
            new Claim(ClaimTypes.Role, role.Code)
        });

        return new Response.RegisterResponse
        {
            Id = user.Id,
            Username = user.Username,
            FullName = fullName,
            Email = user.Email,
            Token = token
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
            Id = Guid.NewGuid(),
            Code = DefaultRoleCode,
            Name = "User",
            Description = "Default application user",
            CreatedAt = now
        };

        _dbContext.Roles.Add(role);
        return role;
    }

    private static (string FirstName, string LastName) SplitFullName(string fullName)
    {
        var normalizedFullName = string.Join(' ', fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var firstSpaceIndex = normalizedFullName.IndexOf(' ');

        if (firstSpaceIndex < 0)
        {
            return (normalizedFullName, string.Empty);
        }

        return (
            normalizedFullName[..firstSpaceIndex],
            normalizedFullName[(firstSpaceIndex + 1)..]
        );
    }
}
