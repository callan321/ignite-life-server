using IgniteLifeApi.Domain.Entities.Common;

namespace IgniteLifeApi.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public bool IsPersistent { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = default!;

        // Revocation / rotation
        public DateTime? RevokedAtUtc { get; set; }
        public string? ReplacedByTokenHash { get; set; }
    }
}
