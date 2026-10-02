using AstroBackend.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroBackend.Domain.Entities
{
    public class UserSubscription : BaseEntity
    {
        public Guid UserId { get; set; }
        public string PlanKey { get; set; } = string.Empty;
        public string Status { get; set; } = "active"; // active | cancelled | expired
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime CurrentPeriodEnd { get; set; }
        public bool CancelAtPeriodEnd { get; set; } = false;
        public string BillingPeriod { get; set; } = "monthly"; // monthly | yearly

        // Navigation
        public virtual User User { get; set; } = null!;
        public virtual SubscriptionPlan Plan { get; set; } = null!;
    }

}
