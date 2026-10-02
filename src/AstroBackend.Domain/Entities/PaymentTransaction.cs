using AstroBackend.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroBackend.Domain.Entities
{
    public class PaymentTransaction : BaseEntity
    {
        public Guid UserId { get; set; }
        public string PlanKey { get; set; } = string.Empty;
        public int AmountAzn { get; set; }
        public string Provider { get; set; } = "mock";
        public string Status { get; set; } = "succeeded"; // succeeded | failed | refunded
        public string? Note { get; set; }
        public string BillingPeriod { get; set; } = "monthly"; // monthly | yearly

        // Navigation
        public virtual User User { get; set; } = null!;
        public virtual SubscriptionPlan Plan { get; set; } = null!;
    }

}
