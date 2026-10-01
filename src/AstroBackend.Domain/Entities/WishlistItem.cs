using AstroBackend.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroBackend.Domain.Entities
{
    public class WishlistItem : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }

        // Navigation
        public virtual User User { get; set; } = null!;
        public virtual ShopProduct Product { get; set; } = null!;
    }

}
