namespace Personal_Finance_Management.Service.Common.Constants;

public static class ErrorCodes
{
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string BadRequest = "BAD_REQUEST";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string NotFound = "NOT_FOUND";
    public const string Conflict = "CONFLICT";
    public const string InternalError = "INTERNAL_ERROR";

    public const string Required = "REQUIRED";
    public const string InvalidLoginCredentials = "INVALID_LOGIN_CREDENTIALS";
    public const string InvalidPageIndex = "INVALID_PAGE_INDEX";
    public const string InvalidPageSize = "INVALID_PAGE_SIZE";

    public const string UserNotFound = "USER_NOT_FOUND";
    public const string AccountBanned = "ACCOUNT_BANNED";

    public const string JarNotFound = "JAR_NOT_FOUND";
    public const string InsufficientJarBalance = "INSUFFICIENT_JAR_BALANCE";

    public const string CategoryNotFound = "CATEGORY_NOT_FOUND";
    public const string FinancialAccountNotFound = "FINANCIAL_ACCOUNT_NOT_FOUND";
    public const string TransactionNotFound = "TRANSACTION_NOT_FOUND";
    public const string GoalNotFound = "GOAL_NOT_FOUND";
    public const string LimitNotFound = "LIMIT_NOT_FOUND";
    public const string ReminderNotFound = "REMINDER_NOT_FOUND";
    public const string ImportJobNotFound = "IMPORT_JOB_NOT_FOUND";

    public const string FileTooLarge = "FILE_TOO_LARGE";
    public const string UnsupportedFileExtension = "UNSUPPORTED_FILE_EXTENSION";
    public const string UnsupportedOcrLayout = "UNSUPPORTED_OCR_LAYOUT";
    public const string ImageTooLarge = "IMAGE_TOO_LARGE";
}
