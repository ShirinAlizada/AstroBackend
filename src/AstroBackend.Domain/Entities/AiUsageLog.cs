using AstroBackend.Domain.Common;

namespace AstroBackend.Domain.Entities
{
    /// <summary>
    /// AIController-in sərbəst (heç bir ChatThread-ə yazılmayan) birbaşa AI çağırışları üçün
    /// gündəlik istifadə sayğacı — SubscriptionPlan.AiMessagesPerDay (və abunəliyi olmayan
    /// istifadəçilər üçün appsettings-dəki AiLimits:FreeMessagesPerDay) limitini tətbiq etmək
    /// məqsədilə. Hər istifadəçi/gün üçün bir sətir (UserId+UsageDate unikal), hər çağırışda
    /// Count 1 artırılır.
    /// </summary>
    public class AiUsageLog : BaseEntity
    {
        public Guid UserId { get; set; }
        public DateOnly UsageDate { get; set; }
        public int Count { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
