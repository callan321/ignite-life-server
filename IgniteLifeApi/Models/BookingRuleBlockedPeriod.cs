namespace IgniteLifeApi.Models;

public class BookingRuleBlockedPeriod
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required DateTime StartDateTime { get; set; }
    public required DateTime EndDateTime { get; set; }
    public string? Description { get; set; } = null;
}
