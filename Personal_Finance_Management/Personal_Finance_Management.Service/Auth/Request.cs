using System.ComponentModel.DataAnnotations;

namespace Personal_Finance_Management.Service.Auth;

public class Request
{
    public class RegisterRequest
    {
        [Required]

        public required string Username { get; set; }

        [Required]

        public required string Email { get; set; }

        [Required]

        public required string Password { get; set; }

        [Required]
        public required string FirstName { get; set; }

        [Required]
        public required string LastName { get; set; }
    }
    public class LoginRequest
    {
        [Required]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}
