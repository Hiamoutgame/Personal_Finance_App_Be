namespace Personal_Finance_Management.Service.Reminder;

public class Response
{
    public class GetRemindersResponse
    {
        public List<ReminderResponse> Data { get; set; }
    }

    public class ReminderResponse
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public decimal Amount { get; set; }
        public required string Frequency { get; set; }
        public DateTimeOffset NextDueDate { get; set; }
        public required string Status { get; set; }
    }

    public class ReminderActionResponse
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Frequency { get; set; }
        public DateTimeOffset NextDueDate { get; set; }
        public required string Status { get; set; }
    }

    public class MessageResponse
    {
        public required string Message { get; set; }
    }
}