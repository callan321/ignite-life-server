using System.ComponentModel.DataAnnotations;

namespace IgniteLifeApi.Application.Dtos.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(3, ErrorMessage = "Password must be at least 3 characters long.")]
        public required string Password { get; set; }
        public bool RememberMe { get; set; } = false;
    }
}
