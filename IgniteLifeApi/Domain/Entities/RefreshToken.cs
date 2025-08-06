using IgniteLifeApi.Domain.Models.Common;

namespace IgniteLifeApi.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsPersistent { get; set; }
        public Guid UserId { get; set; }
        public AdminUser User { get; set; } = default!;
    }
}
