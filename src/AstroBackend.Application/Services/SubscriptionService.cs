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

    public async Task<IReadOnlyList<SubscriptionPlanDto>> GetActivePlansAsync(string? lang = null, CancellationToken ct = default)
    {
        var plans = await _planRepo.FindAsync(p => p.IsActive, ct);
        return plans
            .OrderBy(p => p.SortOrder)
            .Select(p => MapPlan(p, lang))
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

    // --- Admin CRUD (xam, çoxdilli sahələrlə) ---

    public async Task<IReadOnlyList<AdminSubscriptionPlanDto>> AdminGetAllPlansAsync(CancellationToken ct = default)
    {
        var plans = await _planRepo.GetAllAsync(ct);
        return plans.OrderBy(p => p.SortOrder).Select(MapAdminPlan).ToList();
    }

    public async Task<AdminSubscriptionPlanDto> AdminCreatePlanAsync(CreateSubscriptionPlanRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.Name))
            throw new BadRequestException("Açar (key) və ad mütləq daxil edilməlidir.");

        var existing = await _planRepo.FirstOrDefaultAsync(p => p.Key == request.Key, ct);
        if (existing != null)
            throw new BadRequestException("Bu açarla (key) paket artıq mövcuddur.");

        var plan = new SubscriptionPlan
        {
            Key = request.Key.Trim(),
            Name = request.Name.Trim(),
            Tagline = request.Tagline,
            TaglineEn = request.TaglineEn,
            TaglineRu = request.TaglineRu,
            PriceAzn = request.PriceAzn,
            BillingPeriod = string.IsNullOrWhiteSpace(request.BillingPeriod) ? "monthly" : request.BillingPeriod,
            Features = request.Features ?? new List<string>(),
            FeaturesEn = request.FeaturesEn,
            FeaturesRu = request.FeaturesRu,
            AiMessagesPerDay = request.AiMessagesPerDay,
            SynastryFullDetail = request.SynastryFullDetail,
            BookingDiscountPct = request.BookingDiscountPct,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };

        await _planRepo.AddAsync(plan, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapAdminPlan(plan);
    }

    public async Task<AdminSubscriptionPlanDto> AdminUpdatePlanAsync(Guid id, UpdateSubscriptionPlanRequest request, CancellationToken ct = default)
    {
        var plan = await _planRepo.GetByIdAsync(id, ct);
        if (plan == null)
            throw new NotFoundException("Abunəlik paketi tapılmadı.");

        plan.Name = request.Name.Trim();
        plan.Tagline = request.Tagline;
        plan.TaglineEn = request.TaglineEn;
        plan.TaglineRu = request.TaglineRu;
        plan.PriceAzn = request.PriceAzn;
        plan.BillingPeriod = string.IsNullOrWhiteSpace(request.BillingPeriod) ? "monthly" : request.BillingPeriod;
        plan.Features = request.Features ?? new List<string>();
        plan.FeaturesEn = request.FeaturesEn;
        plan.FeaturesRu = request.FeaturesRu;
        plan.AiMessagesPerDay = request.AiMessagesPerDay;
        plan.SynastryFullDetail = request.SynastryFullDetail;
        plan.BookingDiscountPct = request.BookingDiscountPct;
        plan.SortOrder = request.SortOrder;
        plan.IsActive = request.IsActive;
        plan.UpdatedAt = DateTime.UtcNow;

        _planRepo.Update(plan);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapAdminPlan(plan);
    }

    public async Task AdminDeletePlanAsync(Guid id, CancellationToken ct = default)
    {
        var plan = await _planRepo.GetByIdAsync(id, ct);
        if (plan == null)
            throw new NotFoundException("Abunəlik paketi tapılmadı.");

        _planRepo.Delete(plan);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    /// <summary>lang == "en"/"ru" olduqda uyğun tərcümə sütunu, boş/null olduqda isə Azərbaycan mətni istifadə olunur.</summary>
    private static SubscriptionPlanDto MapPlan(SubscriptionPlan plan, string? lang)
    {
        string? tagline = plan.Tagline;
        List<string> features = plan.Features;

        if (string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase))
        {
            tagline = string.IsNullOrWhiteSpace(plan.TaglineEn) ? plan.Tagline : plan.TaglineEn;
            features = (plan.FeaturesEn != null && plan.FeaturesEn.Count > 0) ? plan.FeaturesEn : plan.Features;
        }
        else if (string.Equals(lang, "ru", StringComparison.OrdinalIgnoreCase))
        {
            tagline = string.IsNullOrWhiteSpace(plan.TaglineRu) ? plan.Tagline : plan.TaglineRu;
            features = (plan.FeaturesRu != null && plan.FeaturesRu.Count > 0) ? plan.FeaturesRu : plan.Features;
        }

        return new(
            plan.Key,
            plan.Name,
            tagline,
            plan.PriceAzn,
            plan.BillingPeriod,
            features,
            plan.AiMessagesPerDay,
            plan.SynastryFullDetail,
            plan.BookingDiscountPct
        );
    }

    private static AdminSubscriptionPlanDto MapAdminPlan(SubscriptionPlan plan) => new(
        plan.Id,
        plan.Key,
        plan.Name,
        plan.Tagline,
        plan.TaglineEn,
        plan.TaglineRu,
        plan.PriceAzn,
        plan.BillingPeriod,
        plan.Features,
        plan.FeaturesEn,
        plan.FeaturesRu,
        plan.AiMessagesPerDay,
        plan.SynastryFullDetail,
        plan.BookingDiscountPct,
        plan.SortOrder,
        plan.IsActive
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
