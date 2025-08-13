using IgniteLifeApi.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace IgniteLifeApi.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>, IHasTimestamps
    {
        // IdentityUser properties
        public ICollection<RefreshToken> RefreshTokens { get; set; } = default!;

        // IhasTimestamps interface
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
