namespace Server.Models;

public class BookingRuleOpeningException
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public Guid BookingRuleId { get; set; }
}
