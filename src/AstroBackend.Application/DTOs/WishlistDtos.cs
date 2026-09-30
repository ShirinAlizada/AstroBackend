using System;
using System.Collections.Generic;
using System.Text;

namespace AstroBackend.Application.DTOs
{
    public record WishlistItemDto(Guid ProductId, DateTime CreatedAt);

}
