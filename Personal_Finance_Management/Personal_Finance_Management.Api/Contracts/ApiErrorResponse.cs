namespace Personal_Finance_Management.Api.Contracts;

public class ApiErrorResponse
{
    public bool success { get; set; }
    public string error { get; set; } = string.Empty;
    public string message { get; set; } = string.Empty;
    public object? details { get; set; }
    public string traceId { get; set; } = string.Empty;
}
