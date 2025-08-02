using System.ComponentModel.DataAnnotations;

namespace IgniteLifeApi.Dtos.BookingRuleBlockedPeriod
{
    public class UpdateBookingRuleBlockedPeriodDto
    {
        public Guid Id { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}
