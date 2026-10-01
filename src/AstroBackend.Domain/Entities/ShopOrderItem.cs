using AstroBackend.Domain.Common;

namespace AstroBackend.Domain.Entities
{
    public class ShopOrderItem : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid? ProductId { get; set; }
        /// <summary>Sifariş anındakı ad — məhsul sonradan dəyişsə/silinsə belə sifariş qeydi qorunur.</summary>
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public int UnitPriceAzn { get; set; }

        // Navigation
        public virtual ShopOrder Order { get; set; } = null!;
        public virtual ShopProduct? Product { get; set; }
    }

}
