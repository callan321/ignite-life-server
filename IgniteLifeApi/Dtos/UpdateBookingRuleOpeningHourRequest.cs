namespace Server.Dtos;
public class UpdateBookingRuleOpeningHourRequest
{
    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly? OpenTime { get; set; }
    public TimeOnly? CloseTime { get; set; }
}
