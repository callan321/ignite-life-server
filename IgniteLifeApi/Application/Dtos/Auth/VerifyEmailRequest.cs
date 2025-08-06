using System.ComponentModel.DataAnnotations;

namespace IgniteLifeApi.Application.Dtos.Auth
{
    public class VerifyEmailRequest
    {
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format.")]
        public required string Email { get; set; }
        public required string Token { get; set; }
    }
}
