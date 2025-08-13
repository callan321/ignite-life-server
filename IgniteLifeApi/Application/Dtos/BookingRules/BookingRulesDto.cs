namespace IgniteLifeApi.Application.Dtos.BookingRules
{
    public class BookingRulesDto
    {
        public Guid Id { get; set; }
        public int MaxAdvanceBookingDays { get; set; }
        public int BufferBetweenBookingsMinutes { get; set; }
        public int SlotDurationMinutes { get; set; }
        public int MinAdvanceBookingHours { get; set; }
        public List<BookingRulesOpeningHourDto> OpeningHours { get; set; } = [];
        public List<BookingRulesBlockedPeriodDto> BlockedPeriods { get; set; } = [];
    }
}
