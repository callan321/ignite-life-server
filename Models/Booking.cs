namespace Server.Models;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? UserProfileId { get; set; }
    public UserProfile? User { get; set; }

    public Guid? ServiceId { get; set; }
    public BookingService? Service { get; set; }

    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
}
