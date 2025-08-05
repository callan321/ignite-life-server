using System.ComponentModel.DataAnnotations;

namespace IgniteLifeApi.Dtos.BookingRuleBlockedPeriod
{
    public class CreateBookingRuleBlockedPeriodDto
    {
        [Required]
        public DateTime? StartDateTime { get; set; }

        [Required]
        public DateTime? EndDateTime { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
