namespace IgniteLifeApi.Application.Dtos.BookingRules
{
    public class BookingRulesBlockedPeriodDto
    {
        public Guid Id { get; set; }
        public DateTime StartDateTimeUtc { get; set; }
        public DateTime EndDateTimeUtc { get; set; }
        public string? Description { get; set; }
    }
}
