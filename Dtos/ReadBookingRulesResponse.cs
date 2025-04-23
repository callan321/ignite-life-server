namespace Server.Dtos;

public class ReadBookingRulesResponse
{
    public int BufferBetweenBookingsMinutes { get; set; }
    public List<ReadBookingRuleOpeningHourResponse> OpeningHours { get; set; } = [];
    public List<ReadBookingRuleOpeningExceptionResponse> OpeningExceptions { get; set; } = [];
}
