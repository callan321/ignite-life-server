namespace IgniteLifeApi.Application.Dtos.BowenServices
{
    public class BowenServiceResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public string Description { get; set; } = default!;
        public bool IsMultiSession { get; set; }
        public int? SessionCount { get; set; }
        public bool IsGroupSession { get; set; }
        public int? MaxGroupSize { get; set; }
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageAltText { get; set; }
    }
}
