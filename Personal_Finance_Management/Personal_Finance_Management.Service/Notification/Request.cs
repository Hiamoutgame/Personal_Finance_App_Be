namespace Personal_Finance_Management.Service.notification;

public class Request
{
    public class UpdateStatusRequest
    {
        
        public List<Guid>? Ids { get; set; } 
        
        public required bool IsRead { get; set; } 
        
        public bool MarkAll { get; set; } = false;
    }
}