namespace Personal_Finance_Management.Service.User;

public class Request
{
    public class UpdateUserRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
    }
    public class UserIdRequest
    {
        public Guid UserId { get; set; }
    }
    public class UserStatusRequest
    {
        public Guid UserId { get; set; }

    }
}
