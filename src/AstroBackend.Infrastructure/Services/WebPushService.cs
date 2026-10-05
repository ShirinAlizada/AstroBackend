using AstroBackend.Application.Interfaces.Services;
using WebPush;

namespace AstroBackend.Infrastructure.Services
{
    /// <summary>
    /// IPushService-in Web Push (VAPID) protokolu ilə tətbiqi — "WebPush" NuGet paketi
    /// (web-push-csharp) üzərindən. VAPID açarları appsettings.json-dan (Vapid:PublicKey/
    /// PrivateKey/Subject) ServiceRegistration vasitəsilə ötürülür.
    /// </summary>
    public class WebPushService : IPushService
    {
        private readonly string _publicKey;
        private readonly string _privateKey;
        private readonly string _subject;

        public WebPushService(string publicKey, string privateKey, string subject)
        {
            _publicKey = publicKey;
            _privateKey = privateKey;
            _subject = subject;
        }

        public async Task SendAsync(string endpoint, string p256dh, string authKey, string payloadJson, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_publicKey) || string.IsNullOrWhiteSpace(_privateKey))
                return; // VAPID konfiqurasiya olunmayıbsa, sakitcə heç nə etmir (best-effort kanal).

            var subscription = new WebPush.PushSubscription(endpoint, p256dh, authKey);
            var vapidDetails = new VapidDetails(_subject, _publicKey, _privateKey);
            var client = new WebPushClient();

            await client.SendNotificationAsync(subscription, payloadJson, vapidDetails, ct);
        }
    }
}
