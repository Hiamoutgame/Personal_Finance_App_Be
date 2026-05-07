using FluentValidation;
using AdminRequest = Personal_Finance_Management.Service.Admin.Request;

namespace Personal_Finance_Management.Service.Validations;

public class AdminDashboardRequestValidator : AbstractValidator<AdminRequest.AdminDashboardRequest>
{
    private static readonly string[] AllowedTimeframes = ["day", "month", "year"];

    public AdminDashboardRequestValidator()
    {
        RuleFor(request => request.Timeframe)
            .Must(value => string.IsNullOrWhiteSpace(value)
                || AllowedTimeframes.Contains(value.Trim().ToLowerInvariant()))
            .WithMessage("Invalid timeframe. Allowed values: day, month, year.")
            .WithErrorCode("INVALID_TIMEFRAME");
    }
}

public class AdminAuditLogsRequestValidator : AbstractValidator<AdminRequest.AdminAuditLogsRequest>
{
    public AdminAuditLogsRequestValidator()
    {
        RuleFor(request => request.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.")
            .WithErrorCode("INVALID_PAGE");

        RuleFor(request => request.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0.")
            .WithErrorCode("INVALID_PAGE_SIZE");

        RuleFor(request => request)
            .Must(request => !request.FromDate.HasValue
                || !request.ToDate.HasValue
                || request.FromDate.Value <= request.ToDate.Value)
            .WithName(nameof(AdminRequest.AdminAuditLogsRequest.FromDate))
            .WithMessage("fromDate must be earlier than toDate.")
            .WithErrorCode("INVALID_DATE_RANGE");
    }
}
