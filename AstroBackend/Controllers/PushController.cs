using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers;

/// <summary>Brauzer Web Push (VAPID) abunəlik idarəetməsi.</summary>
public class PushController : BaseApiController
{
    private readonly IPushSubscriptionService _pushSubscriptionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IConfiguration _configuration;

    public PushController(
        IPushSubscriptionService pushSubscriptionService,
        ICurrentUserService currentUserService,
        IConfiguration configuration)
    {
        _pushSubscriptionService = pushSubscriptionService;
        _currentUserService = currentUserService;
        _configuration = configuration;
    }

    /// <summary>Frontend-in brauzerdə PushManager.subscribe() çağırarkən istifadə edəcəyi ictimai VAPID açarı.</summary>
    [HttpGet("public-key")]
    public ActionResult<object> GetPublicKey()
    {
        var key = _configuration["Vapid:PublicKey"] ?? string.Empty;
        return Ok(new { publicKey = key });
    }

    [Authorize]
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscribeRequest request, CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        await _pushSubscriptionService.SubscribeAsync(_currentUserService.UserId.Value, request, ct);
        return Ok(new { message = "Push bildirişlərinə abunə olundu." });
    }

    [Authorize]
    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] PushUnsubscribeRequest request, CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        await _pushSubscriptionService.UnsubscribeAsync(_currentUserService.UserId.Value, request.Endpoint, ct);
        return Ok(new { message = "Push abunəliyi ləğv edildi." });
    }
}
