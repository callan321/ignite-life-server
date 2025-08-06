namespace IgniteLifeApi.Domain.Entities;

public class BookingServiceType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required TimeSpan Duration { get; set; }
}
