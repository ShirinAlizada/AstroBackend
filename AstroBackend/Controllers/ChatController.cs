using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers
{
    [Authorize]
    public class ChatController : BaseApiController
    {
        private readonly IChatService _chatService;
        private readonly ICurrentUserService _currentUserService;

        public ChatController(IChatService chatService, ICurrentUserService currentUserService)
        {
            _chatService = chatService;
            _currentUserService = currentUserService;
        }

        [HttpGet("threads")]
        public async Task<ActionResult<IReadOnlyList<ChatThreadDto>>> GetThreads(CancellationToken ct)
        {
            if (!_currentUserService.UserId.HasValue) throw new UnauthorizedException("Giriş edilməyib.");
            var list = await _chatService.GetThreadsAsync(_currentUserService.UserId.Value, ct);
            return Ok(list);
        }

        [HttpPost("threads")]
        public async Task<ActionResult<ChatThreadDto>> CreateThread([FromBody] CreateThreadRequest request, CancellationToken ct)
        {
            if (!_currentUserService.UserId.HasValue) throw new UnauthorizedException("Giriş edilməyib.");
            var created = await _chatService.CreateThreadAsync(_currentUserService.UserId.Value, request, ct);
            return Ok(created);
        }

        [HttpDelete("threads/{id:guid}")]
        public async Task<IActionResult> DeleteThread(Guid id, CancellationToken ct)
        {
            if (!_currentUserService.UserId.HasValue) throw new UnauthorizedException("Giriş edilməyib.");
            await _chatService.DeleteThreadAsync(id, _currentUserService.UserId.Value, ct);
            return Ok(new { message = "Söhbət silindi." });
        }

        [HttpGet("threads/{threadId:guid}/messages")]
        public async Task<ActionResult<IReadOnlyList<ChatMessageDto>>> GetMessages(Guid threadId, CancellationToken ct)
        {
            if (!_currentUserService.UserId.HasValue) throw new UnauthorizedException("Giriş edilməyib.");
            var list = await _chatService.GetMessagesAsync(threadId, _currentUserService.UserId.Value, ct);
            return Ok(list);
        }

        [HttpPost("threads/{threadId:guid}/messages")]
        public async Task<ActionResult<ChatMessageDto>> SendMessage(Guid threadId, [FromBody] SendMessageRequest request, CancellationToken ct)
        {
            if (!_currentUserService.UserId.HasValue) throw new UnauthorizedException("Giriş edilməyib.");
            var reply = await _chatService.SendMessageAsync(threadId, _currentUserService.UserId.Value, request, ct);
            return Ok(reply);
        }
    }

}
