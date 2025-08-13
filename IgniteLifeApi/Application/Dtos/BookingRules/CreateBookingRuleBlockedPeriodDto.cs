namespace IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod
{
    public class CreateBookingRuleBlockedPeriodDto
    {
        public Guid BookingRulesId { get; set; }
        public DateTime? StartDateTimeUtc { get; set; }
        public DateTime? EndDateTimeUtc { get; set; }
        public string? Description { get; set; }
    }
}
