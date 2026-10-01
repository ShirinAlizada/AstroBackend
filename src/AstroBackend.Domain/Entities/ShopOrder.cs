using AstroBackend.Domain.Common;
using AstroBackend.Domain.Enums;

namespace AstroBackend.Domain.Entities
{
    public class ShopOrder : BaseEntity
    {
        public Guid UserId { get; set; }
        /// <summary>Endirimdən əvvəlki cəm (aralıq cəm).</summary>
        public int SubtotalAzn { get; set; }
        /// <summary>Tətbiq olunan endirim kodunun faizi (0 = endirim yoxdur).</summary>
        public int DiscountPct { get; set; } = 0;
        /// <summary>Endirimdən sonra faktiki ödənilən cəm.</summary>
        public int TotalAzn { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Note { get; set; }
        public ShopOrderStatus Status { get; set; } = ShopOrderStatus.Yeni;

        // Navigation
        public virtual User User { get; set; } = null!;
        public virtual ICollection<ShopOrderItem> Items { get; set; } = new List<ShopOrderItem>();
    }

}
