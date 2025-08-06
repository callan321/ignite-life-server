using IgniteLifeApi.Domain.Models.Interfaces;

namespace IgniteLifeApi.Domain.Models.Common
{
    public class BaseEntity : IHasTimestamps
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
