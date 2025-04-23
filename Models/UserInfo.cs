namespace Server.Models
{
    public class UserInfo
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserProfileId { get; set; }
        public UserProfile UserProfile { get; set; } = null!;

        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

    }
}
