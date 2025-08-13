using IgniteLifeApi.Domain.Entities.Common;

namespace IgniteLifeApi.Domain.Entities
{
    public class BookingRuleOpeningHour : BaseEntity
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly OpenTimeUtc { get; set; }
        public TimeOnly CloseTimeUtc { get; set; }
        public bool IsClosed { get; set; } = false;
        public Guid BookingRulesId { get; set; }
        public BookingRules BookingRules { get; set; } = default!;
    }
}