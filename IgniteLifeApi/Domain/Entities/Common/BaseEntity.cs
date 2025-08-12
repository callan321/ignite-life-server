using IgniteLifeApi.Domain.Interfaces;

namespace IgniteLifeApi.Domain.Entities.Common
{
    public class BaseEntity : IHasTimestamps
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
