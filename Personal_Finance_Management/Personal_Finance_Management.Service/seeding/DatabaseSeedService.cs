using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Constants;
using Personal_Finance_Management.Repository.Entity;
using Personal_Finance_Management.Repository.Enum;

namespace Personal_Finance_Management.Service.Seeding;

public class DatabaseSeedService
{
    private readonly AppDbContext _dbContext;
    private readonly SeedAccountsOptions _options;

    public DatabaseSeedService(AppDbContext dbContext, IOptions<SeedAccountsOptions> options)
    {
        _dbContext = dbContext;
        _options = options.Value;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        await EnsureRoles(now, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!_options.Enabled)
        {
            return;
        }

        foreach (var accountOptions in _options.Accounts)
        {
            await EnsureAccount(accountOptions, now, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureRoles(DateTimeOffset now, CancellationToken cancellationToken)
    {
        await EnsureRole(
            AppRoles.Ids.User,
            AppRoles.Codes.User,
            "Default application user",
            now,
            cancellationToken);

        await EnsureRole(
            AppRoles.Ids.Admin,
            AppRoles.Codes.Admin,
            "Application administrator",
            now,
            cancellationToken);
    }

    private async Task EnsureRole(
        Guid id,
        string code,
        string description,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Code == code, cancellationToken);
        if (role is not null)
        {
            role.Name = code;
            role.Description = description;
            return;
        }

        _dbContext.Roles.Add(new Role
        {
            Id = id,
            Code = code,
            Name = code,
            Description = description,
            CreatedAt = now
        });
    }

    private async Task EnsureAccount(
        SeedAccountOptions accountOptions,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var username = accountOptions.Username.Trim();
        var email = accountOptions.Email.Trim().ToLowerInvariant();
        var firstName = accountOptions.FirstName.Trim();
        var lastName = accountOptions.LastName.Trim();

        if (string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(accountOptions.Password)
            || string.IsNullOrWhiteSpace(firstName)
            || string.IsNullOrWhiteSpace(lastName))
        {
            throw new InvalidOperationException("Seed account requires username, email, password, firstName, and lastName.");
        }

        if (!Enum.IsDefined(accountOptions.Role))
        {
            throw new InvalidOperationException($"Seed account role '{accountOptions.Role}' is invalid.");
        }

        var roleCode = accountOptions.Role.ToString();
        var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Code == roleCode, cancellationToken);
        if (role is null)
        {
            throw new InvalidOperationException($"Role '{roleCode}' was not seeded.");
        }

        var account = await _dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Email == email || a.Username == username, cancellationToken);

        if (account is null)
        {
            _dbContext.Accounts.Add(new Account
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(accountOptions.Password, 12),
                FirstName = firstName,
                LastName = lastName,
                RoleId = role.Id,
                Status = AccountStatus.Active.ToString(),
                PreferredCurrency = "VND",
                CreatedAt = now,
                UpdatedAt = now
            });

            return;
        }

        account.RoleId = role.Id;
        account.FirstName = firstName;
        account.LastName = lastName;
        account.Status = AccountStatus.Active.ToString();
        account.UpdatedAt = now;

        if (_options.ResetPasswords)
        {
            account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(accountOptions.Password, 12);
        }
    }
}
