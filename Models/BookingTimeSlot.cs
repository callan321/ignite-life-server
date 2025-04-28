namespace Server.Models;

public class BookingTimeSlot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool IsAvailable { get; set; } = true;
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public Guid? BookingId { get; set; }
    public Booking? Booking { get; set; }

}
