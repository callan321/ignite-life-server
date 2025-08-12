using IgniteLifeApi.Domain.Entities.Common;

namespace IgniteLifeApi.Domain.Entities
{
    public class BookingRuleBlockedPeriod : BaseEntity
    {
        public required DateTime StartDateTime { get; set; }
        public required DateTime EndDateTime { get; set; }
        public string? Description { get; set; } = null;
    }
}