using IgniteLifeApi.Domain.Entities.Common;

namespace IgniteLifeApi.Domain.Entities
{
    public class BookingRuleBlockedPeriod : BaseEntity
    {
        public required DateTime StartDateTimeUtc { get; set; }
        public required DateTime EndDateTimeUtc { get; set; }
        public string? Description { get; set; } = null;
        public required Guid BookingRulesId { get; set; }
        public BookingRules BookingRules { get; set; } = default!;
    }
}