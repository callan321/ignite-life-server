namespace IgniteLifeApi.Application.Dtos.Auth
{
    public class VerifyEmailRequest
    {
        public required string Email { get; set; }
        public required string Token { get; set; }
    }
}
