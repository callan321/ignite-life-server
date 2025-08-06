using System.ComponentModel.DataAnnotations;

namespace IgniteLifeApi.Application.Dtos.Auth
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format.")]
        public required string Email { get; set; }
    }
}
