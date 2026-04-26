using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using AuthRequest = Personal_Finance_Management.Service.Auth.Request;

namespace Personal_Finance_Management.Service.Validations;

public class ValidationServices : IServices
{
    private readonly AppDbContext _dbContext;

    public ValidationServices(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<T> ValidateFormRequest<T>(T request)
    {
        if (request is null)
        {
            throw AppValidationException.BadRequest("Request body is required.", "body", "REQUIRED");
        }

        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(request);

        if (!Validator.TryValidateObject(request, context, validationResults, validateAllProperties: true))
        {
            var errors = validationResults
                .SelectMany(result => result.MemberNames.DefaultIfEmpty(string.Empty),
                    (result, memberName) => new
                    {
                        field = ToCamelCase(memberName),
                        error = result.ErrorMessage ?? "Invalid value."
                    })
                .ToArray();

            throw AppValidationException.BadRequest("Invalid form data.", errors, "FORM_INVALID");
        }

        return Task.FromResult(request);
    }

    public async Task ValidateRegisterRequest(AuthRequest.RegisterRequest request)
    {
        await ValidateFormRequest(request);

        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(username))
        {
            throw AppValidationException.BadRequest("Username is required.", "username", "REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw AppValidationException.BadRequest("Email is required.", "email", "REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw AppValidationException.BadRequest("Password is required.", "password", "REQUIRED");
        }

        if (await _dbContext.Accounts.AnyAsync(a => a.Username.ToLower() == username.ToLower()))
        {
            throw AppValidationException.Conflict("Username already exists.", "username", "AUTH_CONFLICT");
        }

        if (await _dbContext.Accounts.AnyAsync(a => a.Email.ToLower() == email))
        {
            throw AppValidationException.Conflict("Email already exists.", "email", "AUTH_CONFLICT");
        }
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}
