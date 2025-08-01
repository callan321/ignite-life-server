namespace Server.Dtos;

public class UpdateBookingServiceTypeRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public TimeSpan? Duration { get; set; }
}
