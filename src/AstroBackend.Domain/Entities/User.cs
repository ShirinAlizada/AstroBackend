using AstroBackend.Domain.Common;
using AstroBackend.Domain.Enums;

namespace AstroBackend.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public AppRole Role { get; set; } = AppRole.User;
        public bool IsActive { get; set; } = true;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Navigation Properties
        public virtual Profile? Profile { get; set; }
        public virtual NatalChart? NatalChart { get; set; }
        public virtual Astrologer? AstrologerProfile { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
        public virtual ICollection<ForumTopic> ForumTopics { get; set; } = new List<ForumTopic>();
        public virtual ICollection<ForumReply> ForumReplies { get; set; } = new List<ForumReply>();
    }
}
