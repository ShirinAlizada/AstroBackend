using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers
{
    [Authorize]
    public class NotificationsController : BaseApiController
    {
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;

        public NotificationsController(INotificationService notificationService, ICurrentUserService currentUserService)
        {
            _notificationService = notificationService;
            _currentUserService = currentUserService;
        }

        private Guid CurrentUserId => _currentUserService.UserId ?? throw new UnauthorizedException("Giriş edilməyib.");

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetMy(CancellationToken ct)
        {
            var list = await _notificationService.GetMyNotificationsAsync(CurrentUserId, ct);
            return Ok(list);
        }

        [HttpPatch("{id:guid}/read")]
        public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
        {
            await _notificationService.MarkReadAsync(id, CurrentUserId, ct);
            return Ok(new { message = "Bildiriş oxunmuş kimi işarələndi." });
        }

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllRead(CancellationToken ct)
        {
            await _notificationService.MarkAllReadAsync(CurrentUserId, ct);
            return Ok(new { message = "Bütün bildirişlər oxunmuş kimi işarələndi." });
        }
    }

}
