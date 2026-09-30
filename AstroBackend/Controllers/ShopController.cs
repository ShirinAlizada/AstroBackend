using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers
{
    public class ShopController : BaseApiController
    {
        private readonly IShopService _shopService;
        private readonly ICurrentUserService _currentUserService;

        public ShopController(IShopService shopService, ICurrentUserService currentUserService)
        {
            _shopService = shopService;
            _currentUserService = currentUserService;
        }

        [HttpGet("products")]
        public async Task<ActionResult<IReadOnlyList<ShopProductDto>>> GetProducts([FromQuery] string? search, [FromQuery] string? sort, CancellationToken ct)
        {
            var list = await _shopService.GetActiveProductsAsync(search, sort, ct);
            return Ok(list);
        }

        [HttpGet("products/{id:guid}/reviews")]
        public async Task<ActionResult<IReadOnlyList<ShopProductReviewDto>>> GetReviews(Guid id, CancellationToken ct)
        {
            var list = await _shopService.GetReviewsAsync(id, ct);
            return Ok(list);
        }

        [Authorize]
        [HttpPost("products/{id:guid}/reviews")]
        public async Task<ActionResult<ShopProductReviewDto>> UpsertReview(Guid id, [FromBody] UpsertReviewRequest request, CancellationToken ct)
        {
            if (!_currentUserService.UserId.HasValue)
                throw new UnauthorizedException("Giriş edilməyib.");

            var review = await _shopService.UpsertReviewAsync(id, _currentUserService.UserId.Value, request, ct);
            return Ok(review);
        }

        [Authorize]
        [HttpPost("orders")]
        public async Task<ActionResult<ShopOrderDto>> PlaceOrder([FromBody] PlaceShopOrderRequest request, CancellationToken ct)
        {
            if (!_currentUserService.UserId.HasValue)
                throw new UnauthorizedException("Giriş edilməyib.");

            var order = await _shopService.PlaceOrderAsync(_currentUserService.UserId.Value, request, ct);
            return Ok(order);
        }

        [Authorize]
        [HttpGet("my-orders")]
        public async Task<ActionResult<IReadOnlyList<ShopOrderDto>>> GetMyOrders(CancellationToken ct)
        {
            if (!_currentUserService.UserId.HasValue)
                throw new UnauthorizedException("Giriş edilməyib.");

            var list = await _shopService.GetMyOrdersAsync(_currentUserService.UserId.Value, ct);
            return Ok(list);
        }
    }

}
