namespace Server.Dtos;

public class UpdateBookingRulesRequest
{
    public int? BufferBetweenBookingsMinutes { get; set; }

    public List<UpdateBookingRuleOpeningHourRequest>? OpeningHours { get; set; }
}