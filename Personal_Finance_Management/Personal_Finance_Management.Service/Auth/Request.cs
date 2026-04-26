using System.ComponentModel.DataAnnotations;

namespace Personal_Finance_Management.Service.Auth;

public class Request
{
    public class RegisterRequest
    {
        [Required]
        [MaxLength(50)]
        public required string Username { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public required string Email { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public required string Password { get; set; }

        [MaxLength(300)]
        public string? FullName { get; set; }
    }
}
