using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Enum;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using AdminRequest = Personal_Finance_Management.Service.Admin.Request;
using AuthRequest = Personal_Finance_Management.Service.Auth.Request;
using ReminderRequest = Personal_Finance_Management.Service.Reminder.Request;
using UserRequest = Personal_Finance_Management.Service.User.Request;

namespace Personal_Finance_Management.Service.Validations;

public class ValidationServices : IServices
{
    private readonly AppDbContext _dbContext;
    private readonly AdminDashboardRequestValidator _adminDashboardRequestValidator = new();
    private readonly AdminAuditLogsRequestValidator _adminAuditLogsRequestValidator = new();

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

        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            throw AppValidationException.BadRequest("First name is required.", "firstName", "REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            throw AppValidationException.BadRequest("Last name is required.", "lastName", "REQUIRED");
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

    public async Task ValidateImportImageRequest(import.Request.ImportData request)
    {
        await ValidateFormRequest(request);

        if (request.File.Length == 0)
        {
            throw AppValidationException.BadRequest("Image file is required.", "file", "REQUIRED");
        }

        const long maxSizeInBytes = 10 * 1024 * 1024;
        if (request.File.Length > maxSizeInBytes)
        {
            throw AppValidationException.BadRequest("File size is too large. Maximum allowed size is 10MB.", "file", "FILE_TOO_LARGE");
        }

        var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        var supportedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".bmp", ".pdf" };
        if (!supportedExtensions.Contains(extension))
        {
            throw AppValidationException.BadRequest("Unsupported file extension. Only JPG, JPEG, PNG, BMP, and PDF are allowed.", "file", "UNSUPPORTED_FILE_EXTENSION");
        }

        var layout = request.Layout?.Trim();
        if (!string.IsNullOrWhiteSpace(layout))
        {
            var supportedLayouts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "none",
                "invoice",
                "document"
            };

            if (!supportedLayouts.Contains(layout))
            {
                throw AppValidationException.BadRequest("Unsupported OCR layout. Only none, invoice, and document are allowed.", "layout", "UNSUPPORTED_OCR_LAYOUT");
            }
        }

        if (extension == ".pdf")
        {
            return;
        }

        try
        {
            await using var formatStream = request.File.OpenReadStream();
            var format = await Image.DetectFormatAsync(formatStream);

            if (format is not (JpegFormat or PngFormat or BmpFormat))
            {
                throw AppValidationException.BadRequest("Unsupported image format. Only JPG, JPEG, PNG, and BMP are allowed.", "file", "UNSUPPORTED_IMAGE_FORMAT");
            }

            await using var identifyStream = request.File.OpenReadStream();
            var imageInfo = await Image.IdentifyAsync(identifyStream);
            if (imageInfo is null)
            {
                throw AppValidationException.BadRequest("Invalid image file.", "file", "INVALID_IMAGE");
            }

            if (imageInfo.Width > 5000 || imageInfo.Height > 5000)
            {
                throw AppValidationException.BadRequest("Image dimensions are too large. Maximum allowed size is 5000x5000 pixels.", "file", "IMAGE_DIMENSIONS_TOO_LARGE");
            }
        }
        catch (UnknownImageFormatException)
        {
            throw AppValidationException.BadRequest("Invalid image file.", "file", "INVALID_IMAGE");
        }
    }

    public async Task ValidateAdminDashboardRequest(AdminRequest.AdminDashboardRequest request)
    {
        await _adminDashboardRequestValidator.ValidateOrThrowAppException(request);
    }

    public async Task ValidateAdminAuditLogsRequest(AdminRequest.AdminAuditLogsRequest request)
    {
        await _adminAuditLogsRequestValidator.ValidateOrThrowAppException(request);
    }

    public Task ValidateCreateReminderRequest(ReminderRequest.CreateReminderRequest request)
    {
        if (request is null)
        {
            throw AppValidationException.BadRequest("Request body is required.", "body", "REQUIRED");
        }

        ValidateRequiredText(request.Title, "title", "Reminder title is required.");
        ValidateReminderAmount(request.Amount, "amount");
        ValidateEnumValue<ReminderFrequency>(request.Frequency, "frequency", "INVALID_REMINDER_FREQUENCY", required: true);
        ValidateDayOfMonth(request.DayOfMonth, "dayOfMonth");
        ValidateNotifyDaysBefore(request.NotifyDaysBefore, "notifyDaysBefore");

        return Task.CompletedTask;
    }

    public Task ValidateUpdateReminderRequest(ReminderRequest.UpdateReminderRequest request)
    {
        if (request is null)
        {
            throw AppValidationException.BadRequest("Request body is required.", "body", "REQUIRED");
        }

        if (request.Title is not null)
        {
            ValidateRequiredText(request.Title, "title", "Reminder title is required.");
        }

        if (request.Amount.HasValue)
        {
            ValidateReminderAmount(request.Amount.Value, "amount");
        }

        if (request.Frequency is not null)
        {
            ValidateEnumValue<ReminderFrequency>(request.Frequency, "frequency", "INVALID_REMINDER_FREQUENCY", required: true);
        }

        ValidateDayOfMonth(request.DayOfMonth, "dayOfMonth");
        ValidateNotifyDaysBefore(request.NotifyDaysBefore, "notifyDaysBefore");

        if (request.Status is not null)
        {
            ValidateEnumValue<ReminderStatus>(request.Status, "status", "INVALID_REMINDER_STATUS", required: true);
        }

        return Task.CompletedTask;
    }

    public Task ValidateAdminUsersRequest(UserRequest.GetAdminUsersRequest request)
    {
        if (request is null)
        {
            throw AppValidationException.BadRequest("Request query is required.", "query", "REQUIRED");
        }

        if (request.PageIndex <= 0)
        {
            throw AppValidationException.BadRequest("Page index must be greater than zero.", "pageIndex", "INVALID_PAGE_INDEX");
        }

        if (request.PageSize <= 0)
        {
            throw AppValidationException.BadRequest("Page size must be greater than zero.", "pageSize", "INVALID_PAGE_SIZE");
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            ValidateEnumValue<AccountStatus>(request.Status, "status", "INVALID_ACCOUNT_STATUS", required: true);
        }

        return Task.CompletedTask;
    }

    public Task ValidateAdminUserStatusRequest(UserRequest.UserStatusRequest request)
    {
        if (request is null)
        {
            throw AppValidationException.BadRequest("Request body is required.", "body", "REQUIRED");
        }

        if (request.UserId == Guid.Empty)
        {
            throw AppValidationException.BadRequest("User id is required.", "userId", "REQUIRED");
        }

        ValidateEnumValue<AccountStatus>(request.Status, "status", "INVALID_ACCOUNT_STATUS", required: true);

        return Task.CompletedTask;
    }

    private static void ValidateRequiredText(string value, string field, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw AppValidationException.BadRequest(message, field, "REQUIRED");
        }
    }

    private static void ValidateReminderAmount(decimal amount, string field)
    {
        if (amount < 0)
        {
            throw AppValidationException.BadRequest("Reminder amount must be greater than or equal to zero.", field, "INVALID_REMINDER_AMOUNT");
        }
    }

    private static void ValidateDayOfMonth(int? dayOfMonth, string field)
    {
        if (dayOfMonth.HasValue && (dayOfMonth.Value < 1 || dayOfMonth.Value > 31))
        {
            throw AppValidationException.BadRequest("Day of month must be between 1 and 31.", field, "INVALID_DAY_OF_MONTH");
        }
    }

    private static void ValidateNotifyDaysBefore(int? notifyDaysBefore, string field)
    {
        if (notifyDaysBefore.HasValue && notifyDaysBefore.Value < 0)
        {
            throw AppValidationException.BadRequest("Notify days before must be greater than or equal to zero.", field, "INVALID_NOTIFY_DAYS_BEFORE");
        }
    }

    private static void ValidateEnumValue<TEnum>(string? value, string field, string code, bool required)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (required)
            {
                throw AppValidationException.BadRequest($"{field} is required.", field, "REQUIRED");
            }

            return;
        }

        if (!Enum.TryParse<TEnum>(value.Trim(), ignoreCase: true, out _))
        {
            throw AppValidationException.BadRequest($"Invalid {field}.", field, code);
        }
    }
}
