using AstroBackend.Domain.Common;

namespace AstroBackend.Domain.Entities
{
    /// <summary>
    /// Brauzer Web Push abunəliyi (VAPID). Frontend-dəki push_subscriptions cədvəlinin
    /// bu backend-dəki analoqu — mövcud DB-daxili Notification zəngindən ayrı, əlavə
    /// bir kanal (tab bağlı olanda da xəbərdarlıq almaq üçün).
    /// </summary>
    public class PushSubscription : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string P256dh { get; set; } = string.Empty;
        public string AuthKey { get; set; } = string.Empty;

        // Navigation
        public virtual User User { get; set; } = null!;
    }
}
