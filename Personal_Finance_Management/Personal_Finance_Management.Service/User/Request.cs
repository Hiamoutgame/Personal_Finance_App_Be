namespace Personal_Finance_Management.Service.User;

public class Request
{
    public class UpdateUserRequest
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string phone { get; set; }
        public string avatarUrl { get; set; }
    }
}