namespace Personal_Finance_Management.Service.Reminder;

public class Request
{
        public class CreateReminderRequest
        {
            public required string Title { get; set; }
            public decimal Amount { get; set; }
            public required string Frequency { get; set; }
            public int? DayOfMonth { get; set; }
            public DateTimeOffset StartDate { get; set; }
            public Guid? CategoryId { get; set; }
            public int NotifyDaysBefore { get; set; }
            public string? Note { get; set; }
        }

        public class UpdateReminderRequest
        {
            public string? Title { get; set; }
            public decimal? Amount { get; set; }
            public string? Frequency { get; set; }
            public int? DayOfMonth { get; set; }
            public string? Status { get; set; } // Ví dụ: "Active", "Inactive"
            public int? NotifyDaysBefore { get; set; }
            public string? Note { get; set; }
        }
}