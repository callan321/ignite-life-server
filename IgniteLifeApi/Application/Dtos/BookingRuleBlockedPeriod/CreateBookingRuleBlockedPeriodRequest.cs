using System.ComponentModel.DataAnnotations;

namespace IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod
{
    public class CreateBookingRuleBlockedPeriodRequest
    {
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string? Description { get; set; }
    }
}
