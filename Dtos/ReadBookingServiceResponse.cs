namespace Server.Dtos;

public class ReadBookingServiceResponse
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required string Duration { get; set; }
}
