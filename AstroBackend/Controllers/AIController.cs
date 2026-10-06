using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Application.Services;
using AstroBackend.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AstroBackend.Controllers
{
    /// <summary>
    /// Sərbəst AI çağırışları (astroloq söhbəti / məqalə köməkçisi). Hər çağırış pul xərcinə
    /// səbəb olduğu üçün iki qat qorunur: (1) "ai" rate-limit siyasəti (istifadəçi üzrə,
    /// Program.cs-də təyin olunub) və (2) IAiUsageService ilə gündəlik sorğu kvotası
    /// (abunəlik planına görə, yoxdursa pulsuz defolt limit).
    /// </summary>
    [Authorize]
    [EnableRateLimiting("ai")]
    public class AIController : BaseApiController
    {
        private readonly IAIService _aiService;
        private readonly IAiUsageService _aiUsageService;
        private readonly ICurrentUserService _currentUserService;

        public AIController(IAIService aiService, IAiUsageService aiUsageService, ICurrentUserService currentUserService)
        {
            _aiService = aiService;
            _aiUsageService = aiUsageService;
            _currentUserService = currentUserService;
        }

        [HttpPost]
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] DirectAiRequest request, CancellationToken ct)
        {
            if (request?.Messages == null || request.Messages.Count == 0)
            {
                return BadRequest("Mesaj göndərilməyib.");
            }

            if (!_currentUserService.UserId.HasValue)
                throw new UnauthorizedException("Giriş edilməyib.");

            await _aiUsageService.EnsureWithinDailyLimitAsync(_currentUserService.UserId.Value, ct);

            string systemPrompt = request.Mode == "article"
                ? ArticleService.ArticleSystemPrompt
                : ChatService.AstrologerSystemPrompt;

            var result = await _aiService.GenerateTextAsync(systemPrompt, request.Messages, ct);
            return Content(result, "text/plain; charset=utf-8");
        }

        [HttpPost("stream")]
        public async Task Stream([FromBody] DirectAiRequest request, CancellationToken ct)
        {
            if (request?.Messages == null || request.Messages.Count == 0)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("Mesaj göndərilməyib.", ct);
                return;
            }

            if (!_currentUserService.UserId.HasValue)
            {
                Response.StatusCode = 401;
                await Response.WriteAsync("Giriş edilməyib.", ct);
                return;
            }

            try
            {
                await _aiUsageService.EnsureWithinDailyLimitAsync(_currentUserService.UserId.Value, ct);
            }
            catch (BadRequestException ex)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync(ex.Message, ct);
                return;
            }

            string systemPrompt = request.Mode == "article"
                ? ArticleService.ArticleSystemPrompt
                : ChatService.AstrologerSystemPrompt;

            Response.ContentType = "text/plain; charset=utf-8";
            Response.Headers.Append("Cache-Control", "no-cache, no-transform");

            await foreach (var delta in _aiService.StreamTextAsync(systemPrompt, request.Messages, ct))
            {
                await Response.WriteAsync(delta, ct);
                await Response.Body.FlushAsync(ct);
            }
        }
    }

}
