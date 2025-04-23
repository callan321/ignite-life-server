namespace Server.Dtos;

public class ReadBookingRuleOpeningHourResponse
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
}