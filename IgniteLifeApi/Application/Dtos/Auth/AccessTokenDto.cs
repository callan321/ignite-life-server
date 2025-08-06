namespace IgniteLifeApi.Application.Dtos.Auth
{
    public class AccessTokenDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiry { get; set; }
    }
}