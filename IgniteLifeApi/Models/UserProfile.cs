namespace Server.Models
{
    public class UserProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Link to Auth0
        public string Auth0Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = string.Empty;

        // one to one relationship with UserInfo
        public UserInfo? Info { get; set; }

        // one to many relationship with Booking
        public List<Booking> Bookings { get; set; } = [];
    }
}
