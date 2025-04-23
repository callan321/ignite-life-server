namespace Server.Models
{
    public class BookingService
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required decimal Price { get; set; }
        public required string Duration { get; set; }
    }
}
