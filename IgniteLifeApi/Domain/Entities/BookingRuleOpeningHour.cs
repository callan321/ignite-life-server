namespace IgniteLifeApi.Domain.Entities
{
    public class BookingRuleOpeningHour
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly OpenTime { get; set; }
        public TimeOnly CloseTime { get; set; }
        public Guid BookingRulesId { get; set; }
    }
}