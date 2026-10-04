using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Exceptions;

namespace AstroBackend.Application.Services;

/// <summary>
/// ISubscriptionService-in tətbiqi. "Pulsuz" paket üçün UserSubscription/PaymentTransaction
/// sətri heç vaxt yazılmır — SubscriptionPlan.cs-dəki doc-comment-ə uyğun olaraq, aktiv
/// abunəliyi olmayan istifadəçi tətbiq səviyyəsində "pulsuz" hesab olunur.
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly IGenericRepository<SubscriptionPlan> _planRepo;
    private readonly IGenericRepository<UserSubscription> _subscriptionRepo;
    private readonly IGenericRepository<PaymentTransaction> _paymentRepo;
    private readonly IUnitOfWork _unitOfWork;

    public SubscriptionService(
        IGenericRepository<SubscriptionPlan> planRepo,
        IGenericRepository<UserSubscription> subscriptionRepo,
        IGenericRepository<PaymentTransaction> paymentRepo,
        IUnitOfWork unitOfWork)
    {
        _planRepo = planRepo;
        _subscriptionRepo = subscriptionRepo;
        _paymentRepo = paymentRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<SubscriptionPlanDto>> GetActivePlansAsync(CancellationToken ct = default)
    {
        var plans = await _planRepo.FindAsync(p => p.IsActive, ct);
        return plans
            .OrderBy(p => p.SortOrder)
            .Select(MapPlan)
            .ToList();
    }

    public async Task<UserSubscriptionDto?> GetMySubscriptionAsync(Guid userId, CancellationToken ct = default)
    {
        var sub = await _subscriptionRepo.FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "active", ct);
        return sub == null ? null : MapSubscription(sub);
    }

    public async Task<UserSubscriptionDto> PurchaseAsync(Guid userId, PurchasePlanRequest request, CancellationToken ct = default)
    {
        var plan = await _planRepo.FirstOrDefaultAsync(p => p.Key == request.PlanKey && p.IsActive, ct);
        if (plan == null)
            throw new NotFoundException("Abunəlik paketi tapılmadı.");

        var billingPeriod = request.BillingPeriod?.ToLower() == "yearly" ? "yearly" : "monthly";
        // Provisional illik qiymətləndirmə: illik = aylıq x 10 (2 ay endirim). Real endirim
        // kataloqu əlavə olunana qədər bu dəyər istifadə olunur.
        var amount = billingPeriod == "yearly" ? plan.PriceAzn * 10 : plan.PriceAzn;
        var periodEnd = billingPeriod == "yearly" ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow.AddMonths(1);

        var existing = await _subscriptionRepo.FirstOrDefaultAsync(s => s.UserId == userId, ct);
        UserSubscription subscription;
        if (existing == null)
        {
            subscription = new UserSubscription
            {
                UserId = userId,
                PlanKey = plan.Key,
                Status = "active",
                StartedAt = DateTime.UtcNow,
                CurrentPeriodEnd = periodEnd,
                CancelAtPeriodEnd = false,
                BillingPeriod = billingPeriod
            };
            await _subscriptionRepo.AddAsync(subscription, ct);
        }
        else
        {
            existing.PlanKey = plan.Key;
            existing.Status = "active";
            existing.StartedAt = DateTime.UtcNow;
            existing.CurrentPeriodEnd = periodEnd;
            existing.CancelAtPeriodEnd = false;
            existing.BillingPeriod = billingPeriod;
            existing.UpdatedAt = DateTime.UtcNow;
            _subscriptionRepo.Update(existing);
            subscription = existing;
        }

        var payment = new PaymentTransaction
        {
            UserId = userId,
            PlanKey = plan.Key,
            AmountAzn = amount,
            Provider = "mock",
            Status = "succeeded",
            Note = $"{plan.Name} ({billingPeriod}) abunəlik alışı",
            BillingPeriod = billingPeriod
        };
        await _paymentRepo.AddAsync(payment, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return MapSubscription(subscription);
    }

    public async Task CancelAsync(Guid userId, CancellationToken ct = default)
    {
        var sub = await _subscriptionRepo.FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "active", ct);
        if (sub == null)
            throw new NotFoundException("Aktiv abunəlik tapılmadı.");

        // Dövr sonuna qədər aktiv qalır, sonra tətbiq səviyyəsində "pulsuz"a keçir.
        sub.CancelAtPeriodEnd = true;
        sub.UpdatedAt = DateTime.UtcNow;
        _subscriptionRepo.Update(sub);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<PaymentTransactionDto>> GetMyPaymentsAsync(Guid userId, CancellationToken ct = default)
    {
        var payments = await _paymentRepo.FindAsync(p => p.UserId == userId, ct);
        return payments
            .OrderByDescending(p => p.CreatedAt)
            .Select(MapPayment)
            .ToList();
    }

    private static SubscriptionPlanDto MapPlan(SubscriptionPlan plan) => new(
        plan.Key,
        plan.Name,
        plan.Tagline,
        plan.PriceAzn,
        plan.BillingPeriod,
        plan.Features,
        plan.AiMessagesPerDay,
        plan.SynastryFullDetail,
        plan.BookingDiscountPct
    );

    private static UserSubscriptionDto MapSubscription(UserSubscription sub) => new(
        sub.PlanKey,
        sub.Status,
        sub.StartedAt,
        sub.CurrentPeriodEnd,
        sub.BillingPeriod
    );

    private static PaymentTransactionDto MapPayment(PaymentTransaction payment) => new(
        payment.Id,
        payment.PlanKey,
        payment.AmountAzn,
        payment.Provider,
        payment.Status,
        payment.Note,
        payment.BillingPeriod,
        payment.CreatedAt
    );
}
