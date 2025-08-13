using IgniteLifeApi.Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace IgniteLifeApi.Domain.Entities
{
    [Index(nameof(TokenHash), IsUnique = true)]
    [Index(nameof(UserId))]
    [Index(nameof(ExpiresAtUtc))]
    public class RefreshToken : BaseEntity
    {
        // Store HASH (hex/64) — never raw tokens
        [Required, MaxLength(128)]
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsPersistent { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = default!;

        // Revocation / rotation
        public DateTime? RevokedAtUtc { get; set; }
        public string? ReplacedByTokenHash { get; set; }

        // Telemetry (optional)
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
