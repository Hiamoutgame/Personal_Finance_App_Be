namespace Personal_Finance_Management.Service.notification;

public class Response
{
    public class GetNotificationsResponse 
    {
        public List<NotificationResponse> Items { get; set; } = new List<NotificationResponse>();
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public int UnreadCount { get; set; } 
    }

    // Từng dòng thông báo
    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public required string Type { get; set; }
        public required string Title { get; set; }
        public required string Body { get; set; }
        public bool IsRead { get; set; }
        public DateTimeOffset OccurredAt { get; set; } 
    }

    // Kết quả trả về sau khi cập nhật trạng thái
    public class UpdateStatusResponse
    {
        public int UpdatedCount { get; set; }
        public int UnreadCount { get; set; }
    }
}