using IgniteLifeApi.Domain.Interfaces;

namespace IgniteLifeApi.Domain.Entities.Common
{
    public class BaseEntity : IHasTimestamps
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
