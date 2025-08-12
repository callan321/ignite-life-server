using System.ComponentModel.DataAnnotations;

namespace IgniteLifeApi.Infrastructure.Configuration
{
    public class JwtSettings
    {
        [Required, MinLength(32, ErrorMessage = "JWT Secret should be at least 32 characters.")]
        public string Secret { get; set; } = default!;
        [Required] public string Issuer { get; set; } = default!;
        [Required] public string Audience { get; set; } = default!;
        [Range(1, int.MaxValue)] public int AccessTokenExpiryMinutes { get; set; } = 15;
        [Range(1, int.MaxValue)] public int SlidingRefreshTokenExpiryHours { get; set; } = 24;
        [Range(1, int.MaxValue)] public int RememberMeRefreshTokenExpiryDays { get; set; } = 30;

        // Cookies
        [Required] public string AccessTokenCookieName { get; set; } = "access_token";
        [Required] public string RefreshTokenCookieName { get; set; } = "refresh_token";
        public string CookiePath { get; set; } = "/";
        public string? CookieDomain { get; set; }
    }
}
