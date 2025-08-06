using IgniteLifeApi.Domain.Models.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace IgniteLifeApi.Domain.Entities
{
    public class AdminUser : IdentityUser<Guid>, IHasTimestamps
    {
        public ICollection<RefreshToken> RefreshTokens { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
