namespace Server.Models;

public class BookingRules
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool IsDefault { get; set; } = false;

    // How much break time after each service
    public int BufferBetweenBookingsMinutes { get; set; } = 30;

    // One-to-many: opening hours per day
    public List<BookingRuleOpeningHour> OpeningHours { get; set; } = [];

    // One-to-many: opening exceptions
    public List<BookingRuleOpeningException> OpeningExceptions { get; set; } = [];
}
