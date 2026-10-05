using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers
{
    public class SubscriptionsController : BaseApiController
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ICurrentUserService _currentUserService;

        public SubscriptionsController(ISubscriptionService subscriptionService, ICurrentUserService currentUserService)
        {
            _subscriptionService = subscriptionService;
            _currentUserService = currentUserService;
        }

        [HttpGet("plans")]
        public async Task<ActionResult<IReadOnlyList<SubscriptionPlanDto>>> GetPlans([FromQuery] string? lang, CancellationToken ct)
        {
            var plans = await _subscriptionService.GetActivePlansAsync(lang, ct);
            return Ok(plans);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserSubscriptionDto?>> GetMySubscription(CancellationToken ct)
        {
            if (!_currentUserService.UserId.HasValue)
                throw new UnauthorizedException("Giriş edilməyib.");

            var sub = await _subscriptionService.GetMySubscriptionAsync(_currentUserService.UserId.Value, ct);
            return Ok(sub);
        }

        [Authorize]
        [HttpPost("purchase")]
        public async Task<ActionResult<UserSubscriptionDto>> Purchase([FromBody] PurchasePlanRequest request, CancellationToken ct)
        {
            if (!_currentUserService.UserId.HasValue)
                throw new UnauthorizedException("Giriş edilməyib.");

            var sub = await _subscriptionService.PurchaseAsync(_currentUserService.UserId.Value, request, ct);
            return Ok(sub);
        }

        [Authorize]
        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel(CancellationToken ct)
        {
            if (!_currentUserService.UserId.HasValue)
                throw new UnauthorizedException("Giriş edilməyib.");

            await _subscriptionService.CancelAsync(_currentUserService.UserId.Value, ct);
            return Ok(new { message = "Abunəlik ləğv edildi." });
        }

        [Authorize]
        [HttpGet("payments")]
        public async Task<ActionResult<IReadOnlyList<PaymentTransactionDto>>> GetMyPayments(CancellationToken ct)
        {
            if (!_currentUserService.UserId.HasValue)
                throw new UnauthorizedException("Giriş edilməyib.");

            var payments = await _subscriptionService.GetMyPaymentsAsync(_currentUserService.UserId.Value, ct);
            return Ok(payments);
        }
    }

}
