namespace Server.Dtos;

public class CreateBookingServiceTypeRequest
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required TimeSpan Duration { get; set; }
}
