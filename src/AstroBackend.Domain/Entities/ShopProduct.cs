using AstroBackend.Domain.Common;
using AstroBackend.Domain.Enums;

namespace AstroBackend.Domain.Entities
{
    public class ShopProduct : BaseEntity
    {
        public ShopCategory Category { get; set; } = ShopCategory.Tarot;
        public string Slug { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string? NameRu { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? DescriptionEn { get; set; }
        public string? DescriptionRu { get; set; }
        public int PriceAzn { get; set; }
        public string? UnitLabel { get; set; }
        public string? ImageUrl { get; set; }
        public short SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        /// <summary>Stokda qalan say. Sifariş verilərkən yoxlanılır və uğurlu sifarişdə azaldılır.</summary>
        public int Stock { get; set; } = 100;

        // Navigation
        public virtual ICollection<ShopOrderItem> OrderItems { get; set; } = new List<ShopOrderItem>();
        public virtual ICollection<ShopProductReview> Reviews { get; set; } = new List<ShopProductReview>();
    }

}
