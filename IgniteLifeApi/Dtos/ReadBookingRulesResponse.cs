namespace Server.Dtos;

public class ReadBookingRulesResponse
{
    public int MaxAdvanceBookingDays { get; set; }
    public int BufferBetweenBookingsMinutes { get; set; }
    public int SlotDurationMinutes { get; set; }
    public int MinAdvanceBookingHours { get; set; }
    public List<ReadBookingRuleOpeningHourResponse> OpeningHours { get; set; } = [];
    public List<ReadBookingRuleOpeningExceptionResponse> OpeningExceptions { get; set; } = [];
}
