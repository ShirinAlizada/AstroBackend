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

        /// <summary>Şifrə sıfırlama (forgot-password) axını üçün bir dəfəlik token — RefreshToken ilə eyni naxış.</summary>
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiryTime { get; set; }

        // Navigation Properties
        public virtual Profile? Profile { get; set; }
        public virtual NatalChart? NatalChart { get; set; }
        public virtual Astrologer? AstrologerProfile { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
        public virtual ICollection<ForumTopic> ForumTopics { get; set; } = new List<ForumTopic>();
        public virtual ICollection<ForumReply> ForumReplies { get; set; } = new List<ForumReply>();
        public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
        public virtual ICollection<ChatThread> ChatThreads { get; set; } = new List<ChatThread>();
        public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
        public virtual ICollection<ShopOrder> ShopOrders { get; set; } = new List<ShopOrder>();
        public virtual ICollection<ShopProductReview> ShopProductReviews { get; set; } = new List<ShopProductReview>();
        public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual UserSubscription? Subscription { get; set; }
        public virtual ICollection<PaymentTransaction> Payments { get; set; } = new List<PaymentTransaction>();
        public virtual ICollection<PushSubscription> PushSubscriptions { get; set; } = new List<PushSubscription>();
    }

}
