namespace Server.Dtos;

public class ReadBookingRuleOpeningExceptionResponse
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}