namespace IgniteLifeApi.Application.Dtos.Auth
{
    public class TokenResponse
    {
        public AccessTokenDto AccessToken { get; set; } = default!;
        public RefreshTokenDto RefreshToken { get; set; } = default!;
    }
}
