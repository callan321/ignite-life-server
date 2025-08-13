using IgniteLifeApi.Domain.Entities.Common;

namespace IgniteLifeApi.Domain.Entities
{
    public class BowenService : BaseEntity
    {
        public string Title { get; set; } = default!;
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public string Description { get; set; } = default!;

        // Business flags
        public bool IsMultiSession { get; set; }
        public int? SessionCount { get; set; }
        public bool IsGroupSession { get; set; }
        public int? MaxGroupSize { get; set; }
        public bool IsActive { get; set; } = true;

        // Image (store reference, not the file)
        public string? ImageUrl { get; set; }
        public string? ImageAltText { get; set; }
    }
}
