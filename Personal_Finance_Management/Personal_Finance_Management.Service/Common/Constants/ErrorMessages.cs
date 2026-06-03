namespace Personal_Finance_Management.Service.Common.Constants;

public static class ErrorMessages
{
    public const string Required = "Trường này là bắt buộc.";
    public const string InvalidLoginCredentials = "Email hoặc mật khẩu không đúng.";
    public const string InvalidPageIndex = "Trang không hợp lệ.";
    public const string InvalidPageSize = "Kích thước trang không hợp lệ.";
    public const string PageMustBeGreaterThanZero = "Page must be greater than zero.";
    public const string PageSizeBetween1And100 = "Page size must be between 1 and 100.";
    public const string FormInvalid = "Invalid form data.";

    public const string UserNotFound = "Không tìm thấy người dùng.";
    public const string UserNotFoundEn = "User not found.";
    public const string AccountBanned = "Tài khoản đã bị khóa.";
    public const string UsernameAlreadyExists = "Username already exists.";
    public const string EmailAlreadyExists = "Email already exists.";

    public const string JarNotFound = "Không tìm thấy hũ.";
    public const string JarNotFoundEn = "Jar not found.";
    public const string InsufficientJarBalance = "Số tiền trong hũ không đủ để thực hiện giao dịch.";

    public const string CategoryNotFound = "Không tìm thấy danh mục.";
    public const string FinancialAccountNotFound = "Không tìm thấy nguồn tiền.";
    public const string TransactionNotFound = "Không tìm thấy giao dịch.";
    public const string GoalNotFound = "Không tìm thấy mục tiêu.";
    public const string LimitNotFound = "Không tìm thấy hạn mức.";
    public const string ReminderNotFound = "Không tìm thấy lời nhắc.";
    public const string ImportJobNotFound = "Không tìm thấy phiên import.";
    public const string ImportNotFound = "Import not found.";
    public const string ImportDraftNotFound = "Import draft not found.";

    public const string FileTooLarge = "Kích thước file vượt quá giới hạn cho phép.";
    public const string UnsupportedFileExtension = "Định dạng file không được hỗ trợ.";
    public const string UnsupportedOcrLayout = "Layout OCR không hợp lệ. Chỉ hỗ trợ: none, invoice, document.";
    public const string ImageTooLarge = "Ảnh quá lớn. Vượt quá kích thước pixel cho phép.";
    public const string UnsupportedImageFormat = "Unsupported image format. Only JPG, JPEG, PNG, and BMP are allowed.";
    public const string InvalidImage = "Invalid image file.";
    public const string ImageDimensionsTooLarge = "Image dimensions are too large. Maximum allowed size is 5000x5000 pixels.";

    // Transaction
    public const string InvalidTransactionType = "Loại giao dịch không hợp lệ.";
    public const string InvalidTransactionAmount = "Số tiền giao dịch phải lớn hơn 0.";
    public const string TransactionDateInFuture = "Không thể tạo giao dịch trong tương lai.";
    public const string TransferSourceTargetSame = "Hũ nguồn và hũ đích phải khác nhau.";
    public const string InvalidTransferInfo = "Thông tin chuyển tiền không hợp lệ.";
    public const string InvalidDraftTransactionType = "Invalid draft transaction type.";
    public const string AmountMustBeGreaterThanZero = "Amount must be greater than zero.";

    // FinancialAccount
    public const string FinancialAccountNameRequired = "Financial account name is required";
    public const string FinancialAccountNameTooLong = "Financial account name is too long";
    public const string InvalidFinancialAccountType = "Invalid financial account type";
    public const string InvalidCurrency = "Currency must be a 3-letter code, for example VND";
    public const string FinancialAccountAlreadyExists = "Financial account already exists";
    public const string BankNameRequired = "Bank name is required";
    public const string BankNameTooLong = "Bank name is too long";
    public const string BankCodeTooLong = "Bank code is too long";
    public const string BankAccountNumberRequired = "Bank account number is required";
    public const string BankAccountNumberTooLong = "Bank account number is too long";
    public const string AccountHolderNameTooLong = "Account holder name is too long";
    public const string LinkedAccountAlreadyExists = "Linked bank account already exists";
    public const string LinkedAccountBalanceReadOnly = "Linked bank account balance cannot be updated manually.";

    // Goal
    public const string GoalTitleRequired = "Goal title is required.";
    public const string InvalidGoalTargetAmount = "Target amount must be greater than zero.";
    public const string GoalDueDateInPast = "Goal due date cannot be in the past.";

    // Limit
    public const string InvalidLimitAmount = "Limit amount must be greater than zero.";
    public const string InvalidAlertPercentage = "Alert percentage must be between 1 and 100.";
    public const string InvalidLimitTargetType = "Invalid limit target type.";

    // Reminder
    public const string ReminderStartDateInPast = "Reminder start date cannot be in the past.";
    public const string InvalidReminderAmount = "Reminder amount must be greater than or equal to zero.";
    public const string InvalidDayOfMonth = "Day of month must be between 1 and 31.";
    public const string InvalidNotifyDaysBefore = "Notify days before must be greater than or equal to zero.";

    // SePay
    public const string SepayStateRequired = "SePay callback state is required.";
    public const string SepaySessionNotFound = "SePay connection session not found.";
    public const string SepayAccountNotFound = "No SePay bank account found.";
    public const string SepayAccountAlreadyLinked = "SePay bank account is already linked to another user.";
    public const string SepayRedirectUriMissing = "SePay redirect URI is not configured.";
    public const string SepaySyncLinkedAccountRequired = "Only linked bank account can sync SePay transactions.";
    public const string SepayAccountConflict = "Multiple linked financial accounts match SePay account.";
    public const string SepayWebhookTokenMissing = "SePay webhook API key is not configured.";
    public const string SepayWebhookTokenInvalid = "Invalid SePay webhook API key.";
    public const string SepayWebhookVerificationHeaderRequired = "SePay webhook authorization header is required.";
    public const string SepayWebhookDataInvalid = "SePay webhook payload is invalid.";
    public const string SepayTokenMissing = "SePay token is missing.";
    public const string SepayTokenFormatInvalid = "SePay token format is invalid.";
    public const string SepayTokenPayloadInvalid = "SePay token payload is invalid.";
    public const string SepayTokenEncryptionKeyMissing = "SePay token encryption key is not configured.";
    public const string SepayTokenExchangeFailed = "SePay token exchange failed.";
    public const string SepayTokenRefreshFailed = "SePay token refresh failed.";
    public const string SepayTokenResponseInvalid = "SePay token response is invalid.";
    public const string SepayAccountsFailed = "SePay accounts request failed.";
    public const string SepayTransactionsFailed = "SePay transactions request failed.";
    public const string SepayCredentialMissing = "SePay credential is not configured.";
    public const string SepayResponseInvalid = "SePay response is invalid.";
    public const string SepayResponseMissingData = "SePay response is missing data.";
    public const string SepayApiKeyMissing = "SePay API key is not configured.";
    public const string SepayTransactionsUrlMissing = "SePay transactions URL is not configured.";
    public const string SepaySyncFailed = "SePay transaction sync failed.";
}
