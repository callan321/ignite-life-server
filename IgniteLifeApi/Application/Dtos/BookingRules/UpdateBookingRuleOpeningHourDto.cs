namespace IgniteLifeApi.Application.Dtos.BookingRules
{
    public class UpdateBookingRuleOpeningHourDto
    {
        public Guid Id { get; set; }
        public TimeOnly? OpenTimeUtc { get; set; }
        public TimeOnly? CloseTimeUtc { get; set; }
        public bool? IsClosed { get; set; }
    }
}
