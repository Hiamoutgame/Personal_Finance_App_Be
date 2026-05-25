namespace Personal_Finance_Management.Service.Common.Constants;

public static class ErrorMessages
{
    public const string Required = "Trường này là bắt buộc.";
    public const string InvalidLoginCredentials = "Email hoặc mật khẩu không đúng.";
    public const string InvalidPageIndex = "Trang không hợp lệ.";
    public const string InvalidPageSize = "Kích thước trang không hợp lệ.";

    public const string UserNotFound = "Không tìm thấy người dùng.";
    public const string AccountBanned = "Tài khoản đã bị khóa.";

    public const string JarNotFound = "Không tìm thấy hũ.";
    public const string InsufficientJarBalance = "Số tiền trong hũ không đủ để thực hiện giao dịch.";

    public const string CategoryNotFound = "Không tìm thấy danh mục.";
    public const string FinancialAccountNotFound = "Không tìm thấy nguồn tiền.";
    public const string TransactionNotFound = "Không tìm thấy giao dịch.";
    public const string GoalNotFound = "Không tìm thấy mục tiêu.";
    public const string LimitNotFound = "Không tìm thấy hạn mức.";
    public const string ReminderNotFound = "Không tìm thấy lời nhắc.";
    public const string ImportJobNotFound = "Không tìm thấy phiên import.";

    public const string FileTooLarge = "Kích thước file vượt quá giới hạn cho phép.";
    public const string UnsupportedFileExtension = "Định dạng file không được hỗ trợ.";
    public const string UnsupportedOcrLayout = "Layout OCR không hợp lệ. Chỉ hỗ trợ: none, invoice, document.";
    public const string ImageTooLarge = "Ảnh quá lớn. Vượt quá kích thước pixel cho phép.";
}
