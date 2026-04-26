namespace Personal_Finance_Management.Service.Auth;

public class Response
{
    public class RegisterResponse
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Token { get; set; }
    }
}
