using System.ComponentModel.DataAnnotations;

namespace IgniteLifeApi.Application.Dtos.BookingRuleBlockedPeriod
{
    public class UpdateBookingRuleBlockedPeriodRequest
    {
        public Guid Id { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string? Description { get; set; }
    }
}
