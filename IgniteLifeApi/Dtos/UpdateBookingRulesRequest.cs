namespace Server.Dtos;

public class UpdateBookingRulesRequest
{
    public int? MaxAdvanceBookingDays { get; set; }
    public int? BufferBetweenBookingsMinutes { get; set; }
    public int? SlotDurationMinutes { get; set; }
    public int? MinAdvanceBookingHours { get; set; }

    public List<UpdateBookingRuleOpeningHourRequest>? OpeningHours { get; set; }
    public List<UpdateBookingRuleOpeningExceptionRequest>? OpeningExceptions { get; set; }
}
