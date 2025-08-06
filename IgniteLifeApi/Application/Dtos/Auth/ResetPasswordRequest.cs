using System.ComponentModel.DataAnnotations;

namespace IgniteLifeApi.Application.Dtos.Auth
{
    public class ResetPasswordRequest
    {
        public required string Email { get; set; }

        public required string Token { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(3, ErrorMessage = "Password must be at least 3 characters long.")]
        public required string NewPassword { get; set; }
    }
}
