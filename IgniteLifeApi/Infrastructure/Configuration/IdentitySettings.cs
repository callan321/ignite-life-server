using System.ComponentModel.DataAnnotations;

namespace IgniteLifeApi.Infrastructure.Configuration
{
    public class IdentitySettings
    {
        [Range(1, int.MaxValue)]
        public int PasswordRequiredLength { get; set; } = 12;

        public bool RequireDigit { get; set; } = true;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireNonAlphanumeric { get; set; } = true;

        public bool LockoutAllowedForNewUsers { get; set; } = true;

        [Range(1, int.MaxValue)]
        public int LockoutMaxFailedAccessAttempts { get; set; } = 5;

        [Range(1, int.MaxValue)]
        public int LockoutDefaultLockoutMinutes { get; set; } = 15;
    }
}
