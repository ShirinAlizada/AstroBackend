using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using System.Text.Json;

namespace AstroBackend.Application.Services;

/// <summary>
/// İstifadəçinin brauzer Web Push (VAPID) abunəliklərini idarə edir. Göndərmə əməliyyatının
/// özü IPushService-ə (Infrastructure, WebPush paketi) həvalə olunur — bu sinif yalnız
/// "kimə" göndərmək lazım olduğunu bilir və hər abunəlik üçün best-effort cəhd edir.
/// </summary>
public class PushSubscriptionService : IPushSubscriptionService
{
    private readonly IGenericRepository<PushSubscription> _subscriptionRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPushService _pushService;

    public PushSubscriptionService(
        IGenericRepository<PushSubscription> subscriptionRepo,
        IUnitOfWork unitOfWork,
        IPushService pushService)
    {
        _subscriptionRepo = subscriptionRepo;
        _unitOfWork = unitOfWork;
        _pushService = pushService;
    }

    public async Task SubscribeAsync(Guid userId, PushSubscribeRequest request, CancellationToken ct = default)
    {
        var existing = await _subscriptionRepo.FirstOrDefaultAsync(s => s.Endpoint == request.Endpoint, ct);
        if (existing == null)
        {
            var subscription = new PushSubscription
            {
                UserId = userId,
                Endpoint = request.Endpoint,
                P256dh = request.Keys.P256dh,
                AuthKey = request.Keys.Auth
            };
            await _subscriptionRepo.AddAsync(subscription, ct);
        }
        else
        {
            // Eyni endpoint fərqli istifadəçi tərəfindən yenidən abunə olunubsa (paylaşılan
            // cihaz/brauzer), ən son abunəçiyə köçürülür.
            existing.UserId = userId;
            existing.P256dh = request.Keys.P256dh;
            existing.AuthKey = request.Keys.Auth;
            existing.UpdatedAt = DateTime.UtcNow;
            _subscriptionRepo.Update(existing);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UnsubscribeAsync(Guid userId, string endpoint, CancellationToken ct = default)
    {
        var existing = await _subscriptionRepo.FirstOrDefaultAsync(s => s.Endpoint == endpoint && s.UserId == userId, ct);
        if (existing != null)
        {
            _subscriptionRepo.Delete(existing);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }

    public async Task NotifyUserAsync(Guid userId, string title, string body, string? link, CancellationToken ct = default)
    {
        var subscriptions = await _subscriptionRepo.FindAsync(s => s.UserId == userId, ct);
        if (subscriptions.Count == 0) return;

        var payload = JsonSerializer.Serialize(new { title, body, url = link });

        foreach (var sub in subscriptions)
        {
            try
            {
                await _pushService.SendAsync(sub.Endpoint, sub.P256dh, sub.AuthKey, payload, ct);
            }
            catch
            {
                // Best-effort: bir abunəliyin göndərilməsi (məs. köhnəlmiş endpoint) uğursuz
                // olsa belə, qalan abunəliklərə göndərməyə davam edirik, əsas əməliyyatı pozmuruq.
            }
        }
    }
}
