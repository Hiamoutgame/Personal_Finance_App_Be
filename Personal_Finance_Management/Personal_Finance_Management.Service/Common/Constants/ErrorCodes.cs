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
    public const string FormInvalid = "FORM_INVALID";

    public const string UserNotFound = "USER_NOT_FOUND";
    public const string AccountBanned = "ACCOUNT_BANNED";
    public const string AuthConflict = "AUTH_CONFLICT";

    public const string JarNotFound = "JAR_NOT_FOUND";
    public const string InsufficientJarBalance = "INSUFFICIENT_JAR_BALANCE";

    public const string CategoryNotFound = "CATEGORY_NOT_FOUND";
    public const string FinancialAccountNotFound = "FINANCIAL_ACCOUNT_NOT_FOUND";
    public const string TransactionNotFound = "TRANSACTION_NOT_FOUND";
    public const string GoalNotFound = "GOAL_NOT_FOUND";
    public const string LimitNotFound = "LIMIT_NOT_FOUND";
    public const string ReminderNotFound = "REMINDER_NOT_FOUND";
    public const string ImportJobNotFound = "IMPORT_JOB_NOT_FOUND";
    public const string ImportNotFound = "IMPORT_NOT_FOUND";
    public const string ImportDraftNotFound = "IMPORT_DRAFT_NOT_FOUND";

    public const string FileTooLarge = "FILE_TOO_LARGE";
    public const string UnsupportedFileExtension = "UNSUPPORTED_FILE_EXTENSION";
    public const string UnsupportedOcrLayout = "UNSUPPORTED_OCR_LAYOUT";
    public const string ImageTooLarge = "IMAGE_TOO_LARGE";
    public const string UnsupportedImageFormat = "UNSUPPORTED_IMAGE_FORMAT";
    public const string InvalidImage = "INVALID_IMAGE";
    public const string ImageDimensionsTooLarge = "IMAGE_DIMENSIONS_TOO_LARGE";

    // Transaction
    public const string InvalidTransactionType = "INVALID_TRANSACTION_TYPE";
    public const string InvalidTransactionAmount = "INVALID_TRANSACTION_AMOUNT";
    public const string InvalidAmount = "INVALID_AMOUNT";
    public const string TransactionDateInFuture = "TRANSACTION_DATE_IN_FUTURE";
    public const string InvalidTransferTarget = "INVALID_TRANSFER_TARGET";

    // FinancialAccount
    public const string FinancialAccountNameRequired = "FINANCIAL_ACCOUNT_NAME_REQUIRED";
    public const string FinancialAccountNameTooLong = "FINANCIAL_ACCOUNT_NAME_TOO_LONG";
    public const string InvalidFinancialAccountType = "INVALID_FINANCIAL_ACCOUNT_TYPE";
    public const string InvalidCurrency = "INVALID_CURRENCY";
    public const string FinancialAccountAlreadyExists = "FINANCIAL_ACCOUNT_ALREADY_EXISTS";
    public const string BankNameRequired = "BANK_NAME_REQUIRED";
    public const string BankNameTooLong = "BANK_NAME_TOO_LONG";
    public const string BankCodeTooLong = "BANK_CODE_TOO_LONG";
    public const string BankAccountNumberRequired = "BANK_ACCOUNT_NUMBER_REQUIRED";
    public const string BankAccountNumberTooLong = "BANK_ACCOUNT_NUMBER_TOO_LONG";
    public const string AccountHolderNameTooLong = "ACCOUNT_HOLDER_NAME_TOO_LONG";
    public const string LinkedAccountAlreadyExists = "LINKED_ACCOUNT_ALREADY_EXISTS";
    public const string LinkedAccountBalanceReadOnly = "LINKED_ACCOUNT_BALANCE_READ_ONLY";

    // Goal
    public const string GoalTitleRequired = "GOAL_TITLE_REQUIRED";
    public const string InvalidGoalTargetAmount = "INVALID_GOAL_TARGET_AMOUNT";
    public const string GoalDueDateInPast = "GOAL_DUE_DATE_IN_PAST";

    // Limit
    public const string InvalidLimitAmount = "INVALID_LIMIT_AMOUNT";
    public const string InvalidAlertPercentage = "INVALID_ALERT_PERCENTAGE";
    public const string InvalidLimitTargetType = "INVALID_LIMIT_TARGET_TYPE";

    // Reminder
    public const string ReminderStartDateInPast = "REMINDER_START_DATE_IN_PAST";
    public const string InvalidReminderAmount = "INVALID_REMINDER_AMOUNT";
    public const string InvalidDayOfMonth = "INVALID_DAY_OF_MONTH";
    public const string InvalidNotifyDaysBefore = "INVALID_NOTIFY_DAYS_BEFORE";

    // SePay
    public const string SepayStateRequired = "SEPAY_STATE_REQUIRED";
    public const string SepaySessionNotFound = "SEPAY_SESSION_NOT_FOUND";
    public const string SepayAccountNotFound = "SEPAY_ACCOUNT_NOT_FOUND";
    public const string SepayAccountAlreadyLinked = "SEPAY_ACCOUNT_ALREADY_LINKED";
    public const string SepayConfigMissing = "SEPAY_CONFIG_MISSING";
    public const string SepaySyncLinkedAccountRequired = "SEPAY_SYNC_LINKED_ACCOUNT_REQUIRED";
    public const string SepayAccountConflict = "SEPAY_ACCOUNT_CONFLICT";
    public const string SepayWebhookUnauthorized = "SEPAY_WEBHOOK_UNAUTHORIZED";
    public const string SepayWebhookInvalid = "SEPAY_WEBHOOK_INVALID";
    public const string SepaySyncInvalid = "SEPAY_SYNC_INVALID";
    public const string SepayTokenMissing = "SEPAY_TOKEN_MISSING";
    public const string SepayTokenInvalid = "SEPAY_TOKEN_INVALID";
    public const string SepayTokenExchangeFailed = "SEPAY_TOKEN_EXCHANGE_FAILED";
    public const string SepayTokenRefreshFailed = "SEPAY_TOKEN_REFRESH_FAILED";
    public const string SepayAccountsFailed = "SEPAY_ACCOUNTS_FAILED";
    public const string SepayTransactionsFailed = "SEPAY_TRANSACTIONS_FAILED";
    public const string SepayResponseInvalid = "SEPAY_RESPONSE_INVALID";
    public const string SepaySyncFailed = "SEPAY_SYNC_FAILED";
}
