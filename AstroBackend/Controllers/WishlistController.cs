using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers
{
    [Authorize]
    public class WishlistController : BaseApiController
    {
        private readonly IWishlistService _wishlistService;
        private readonly ICurrentUserService _currentUserService;

        public WishlistController(IWishlistService wishlistService, ICurrentUserService currentUserService)
        {
            _wishlistService = wishlistService;
            _currentUserService = currentUserService;
        }

        private Guid CurrentUserId => _currentUserService.UserId ?? throw new UnauthorizedException("Giriş edilməyib.");

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<WishlistItemDto>>> GetMyWishlist(CancellationToken ct)
        {
            var list = await _wishlistService.GetMyWishlistAsync(CurrentUserId, ct);
            return Ok(list);
        }

        [HttpPost("{productId:guid}")]
        public async Task<IActionResult> Add(Guid productId, CancellationToken ct)
        {
            await _wishlistService.AddAsync(CurrentUserId, productId, ct);
            return Ok(new { message = "Sevimlilərə əlavə edildi." });
        }

        [HttpDelete("{productId:guid}")]
        public async Task<IActionResult> Remove(Guid productId, CancellationToken ct)
        {
            await _wishlistService.RemoveAsync(CurrentUserId, productId, ct);
            return Ok(new { message = "Sevimlilərdən silindi." });
        }
    }

}
