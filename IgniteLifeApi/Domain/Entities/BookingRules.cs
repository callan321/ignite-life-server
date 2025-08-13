using IgniteLifeApi.Domain.Entities.Common;
using IgniteLifeApi.Domain.Enums;

namespace IgniteLifeApi.Domain.Entities
{
    public class BookingRules : BaseEntity
    {
        public string TimeZoneId { get; set; } = "UTC";

        // How far in advance can a booking be made
        public int MaxAdvanceBookingDays { get; set; } = 30;

        // How much
        public int BufferBetweenBookingsMinutes { get; set; } = 30;
        // How long each booking slot is
        public int SlotDurationMinutes { get; set; } = 30;

        // How long before a booking can be made
        public int MinAdvanceBookingHours { get; set; } = 12;
        // One-to-many: opening hours per day
        public List<BookingRuleOpeningHour> OpeningHours { get; set; } = [];
        public List<BookingRuleBlockedPeriod> BlockedPeriods { get; set; } = [];
    }
}