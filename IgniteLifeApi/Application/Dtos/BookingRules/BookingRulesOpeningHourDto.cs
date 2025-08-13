namespace IgniteLifeApi.Application.Dtos.BookingRules
{
    public class BookingRulesOpeningHourDto
    {
        public Guid Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly OpenTimeUtc { get; set; }
        public TimeOnly CloseTimeUtc { get; set; }
        public bool IsClosed { get; set; }
    }
}
