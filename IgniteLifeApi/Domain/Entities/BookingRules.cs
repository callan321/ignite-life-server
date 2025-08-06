namespace IgniteLifeApi.Domain.Entities;

public class BookingRules
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool IsDefault { get; set; } = false;

    // How far in advance can a booking be made
    public int MaxAdvanceBookingDays { get; set; } = 30;

    // How much break time after each service
    public int BufferBetweenBookingsMinutes { get; set; } = 30;
    // How long each booking slot is
    public int SlotDurationMinutes { get; set; } = 30;

    // How long before a booking can be made
    public int MinAdvanceBookingHours { get; set; } = 12;
    // One-to-many: opening hours per day
    public List<BookingRuleOpeningHour> OpeningHours { get; set; } = [];
}
