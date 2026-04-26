namespace Personal_Finance_Management.Service.Validations;

public class AppValidationException : Exception
{
    private AppValidationException(string message, int statusCode, object details) : base(message)
    {
        StatusCode = statusCode;
        Details = details;
    }

    public int StatusCode { get; }
    public object Details { get; }

    public static AppValidationException BadRequest(string message, string field, string code)
    {
        return BadRequest(message, new { field, code }, code);
    }

    public static AppValidationException BadRequest(string message, object details, string code)
    {
        return new AppValidationException(message, 400, new { details, code });
    }

    public static AppValidationException Conflict(string message, string field, string code)
    {
        return new AppValidationException(message, 409, new { field, code });
    }
}
