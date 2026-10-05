using AstroBackend.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroBackend.Domain.Entities
{

    /// <summary>
    /// Abunəlik paketi kataloqu (ictimai oxuna bilən, admin tərəfindən idarə olunan).
    /// "Pulsuz" paket üçün bu cədvəldə sətir yoxdur — frontenddəki FREE_PLAN kimi,
    /// heç bir aktiv abunəliyi olmayan istifadəçi üçün tətbiq səviyyəsində defolt tətbiq edilir.
    /// </summary>
    public class SubscriptionPlan : BaseEntity
    {
        /// <summary>Sabit açar ("standart", "premium") — digər cədvəllərdən alternativ açar kimi istinad olunur.</summary>
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Tagline { get; set; }
        public string? TaglineEn { get; set; }
        public string? TaglineRu { get; set; }
        public int PriceAzn { get; set; }
        /// <summary>Bu sətirdəki qiymət aylıq qiymətdir; illik qiymət (endirimli) tətbiq səviyyəsində hesablanır.</summary>
        public string BillingPeriod { get; set; } = "monthly";
        /// <summary>JSON massiv kimi saxlanılır (List&lt;string&gt; dəyər çeviricisi ilə) — SQL Server-də native massiv dəstəyi yoxdur.</summary>
        public List<string> Features { get; set; } = new();
        public List<string>? FeaturesEn { get; set; }
        public List<string>? FeaturesRu { get; set; }
        /// <summary>null = limitsiz.</summary>
        public int? AiMessagesPerDay { get; set; }
        public bool SynastryFullDetail { get; set; } = false;
        public int BookingDiscountPct { get; set; } = 0;
        public short SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

}
