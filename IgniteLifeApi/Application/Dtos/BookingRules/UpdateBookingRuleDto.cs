namespace IgniteLifeApi.Application.Dtos.BookingRules
{
    public class UpdateBookingRulesDto
    {
        public int? MaxAdvanceBookingDays { get; set; }
        public int? BufferBetweenBookingsMinutes { get; set; }
        public int? SlotDurationMinutes { get; set; }
        public int? MinAdvanceBookingHours { get; set; }

        public List<UpdateBookingRuleOpeningHourDto>? OpeningHours { get; set; }
    }

}
