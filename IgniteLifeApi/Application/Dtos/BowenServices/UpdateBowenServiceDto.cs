namespace IgniteLifeApi.Application.Dtos.BowenServices
{
    public class UpdateBowenServiceDto
    {
        public string? Title { get; init; }
        public decimal? Price { get; init; }
        public int? DurationMinutes { get; init; }

        public string? Description { get; init; }
        public bool? IsMultiSession { get; init; }
        public int? SessionCount { get; init; }
        public bool? IsGroupSession { get; init; }
        public int? MaxGroupSize { get; init; }

        public bool? IsActive { get; init; } = true;

        public string? ImageUrl { get; init; }
        public string? ImageAltText { get; init; }
    }
}
