using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Exceptions;

namespace AstroBackend.Application.Services
{
    /// <summary>
    /// IAiUsageService-in tətbiqi. İstifadəçinin aktiv abunəliyi varsa onun planının
    /// AiMessagesPerDay dəyərini (null = limitsiz), yoxdursa konfiqurasiya edilə bilən
    /// "pulsuz istifadəçi" defoltunu tətbiq edir. Hər çağırış bugünkü (UTC) sayğacı 1 artırır.
    /// </summary>
    public class AiUsageService : IAiUsageService
    {
        private readonly IGenericRepository<AiUsageLog> _usageRepo;
        private readonly IGenericRepository<SubscriptionPlan> _planRepo;
        private readonly IGenericRepository<UserSubscription> _subscriptionRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly int _freeMessagesPerDay;

        public AiUsageService(
            IGenericRepository<AiUsageLog> usageRepo,
            IGenericRepository<SubscriptionPlan> planRepo,
            IGenericRepository<UserSubscription> subscriptionRepo,
            IUnitOfWork unitOfWork,
            int freeMessagesPerDay)
        {
            _usageRepo = usageRepo;
            _planRepo = planRepo;
            _subscriptionRepo = subscriptionRepo;
            _unitOfWork = unitOfWork;
            _freeMessagesPerDay = freeMessagesPerDay;
        }

        public async Task EnsureWithinDailyLimitAsync(Guid userId, CancellationToken ct = default)
        {
            int? limit = _freeMessagesPerDay; // defolt: aktiv abunəliyi olmayan ("pulsuz") istifadəçi

            var sub = await _subscriptionRepo.FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "active", ct);
            if (sub != null)
            {
                var plan = await _planRepo.FirstOrDefaultAsync(p => p.Key == sub.PlanKey, ct);
                if (plan != null)
                    limit = plan.AiMessagesPerDay; // null = limitsiz
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var log = await _usageRepo.FirstOrDefaultAsync(l => l.UserId == userId && l.UsageDate == today, ct);

            if (limit.HasValue)
            {
                var used = log?.Count ?? 0;
                if (used >= limit.Value)
                    throw new BadRequestException($"Gündəlik AI sorğu limitiniz ({limit.Value}) bitib. Sabah yenidən cəhd edin və ya planınızı yüksəldin.");
            }

            if (log == null)
            {
                log = new AiUsageLog { UserId = userId, UsageDate = today, Count = 1 };
                await _usageRepo.AddAsync(log, ct);
            }
            else
            {
                log.Count += 1;
                log.UpdatedAt = DateTime.UtcNow;
                _usageRepo.Update(log);
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
