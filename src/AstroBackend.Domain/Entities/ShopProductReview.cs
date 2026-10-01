using AstroBackend.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroBackend.Domain.Entities
{
    public class ShopProductReview : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }

        // Navigation
        public virtual ShopProduct Product { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }

}
