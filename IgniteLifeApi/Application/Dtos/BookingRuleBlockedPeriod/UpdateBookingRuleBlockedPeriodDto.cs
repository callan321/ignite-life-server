namespace IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod
{
    public class UpdateBookingRuleBlockedPeriodDto
    {
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string? Description { get; set; }
    }
}
