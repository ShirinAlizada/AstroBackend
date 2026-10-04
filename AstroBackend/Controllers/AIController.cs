using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers
{
    [Authorize]
    public class AIController : BaseApiController
    {
        private readonly IAIService _aiService;

        public AIController(IAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost]
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] DirectAiRequest request, CancellationToken ct)
        {
            if (request?.Messages == null || request.Messages.Count == 0)
            {
                return BadRequest("Mesaj göndərilməyib.");
            }

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
