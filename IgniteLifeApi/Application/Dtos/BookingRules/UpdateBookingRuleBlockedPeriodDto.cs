namespace IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod
{
    public class UpdateBookingRuleBlockedPeriodDto
    {
        public DateTime? StartDateTimeUtc { get; set; }
        public DateTime? EndDateTimeUtc { get; set; }
        public string? Description { get; set; }
    }
}
