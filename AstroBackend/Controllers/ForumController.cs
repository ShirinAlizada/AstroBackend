using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers;

public class ForumController : BaseApiController
{
    private readonly IForumService _forumService;
    private readonly ICurrentUserService _currentUserService;

    public ForumController(IForumService forumService, ICurrentUserService currentUserService)
    {
        _forumService = forumService;
        _currentUserService = currentUserService;
    }

    [HttpGet("topics")]
    public async Task<ActionResult<IReadOnlyList<ForumTopicDto>>> GetTopics([FromQuery] string? category, CancellationToken ct)
    {
        var list = await _forumService.GetTopicsAsync(category, ct);
        return Ok(list);
    }

    [HttpGet("topics/{id:guid}")]
    public async Task<ActionResult<ForumTopicDto>> GetTopicById(Guid id, CancellationToken ct)
    {
        var topic = await _forumService.GetTopicByIdAsync(id, ct);
        return Ok(topic);
    }

    [Authorize]
    [HttpPost("topics")]
    public async Task<ActionResult<ForumTopicDto>> CreateTopic([FromBody] CreateTopicRequest request, CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        var authorName = _currentUserService.Email?.Split('@')[0] ?? "İstifadəçi";
        var created = await _forumService.CreateTopicAsync(_currentUserService.UserId.Value, authorName, request, ct);
        return Ok(created);
    }

    [HttpGet("topics/{topicId:guid}/replies")]
    public async Task<ActionResult<IReadOnlyList<ForumReplyDto>>> GetReplies(Guid topicId, CancellationToken ct)
    {
        var list = await _forumService.GetRepliesAsync(topicId, ct);
        return Ok(list);
    }

    [Authorize]
    [HttpPost("topics/{topicId:guid}/replies")]
    public async Task<ActionResult<ForumReplyDto>> CreateReply(Guid topicId, [FromBody] CreateReplyRequest request, CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        var authorName = _currentUserService.Email?.Split('@')[0] ?? "İstifadəçi";
        var created = await _forumService.CreateReplyAsync(topicId, _currentUserService.UserId.Value, authorName, request, ct);
        return Ok(created);
    }

    [Authorize]
    [HttpDelete("topics/{id:guid}")]
    public async Task<IActionResult> DeleteTopic(Guid id, CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        bool isAdmin = _currentUserService.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;
        await _forumService.DeleteTopicAsync(id, _currentUserService.UserId.Value, isAdmin, ct);
        return Ok(new { message = "Mövzu silindi." });
    }

    [Authorize]
    [HttpDelete("replies/{id:guid}")]
    public async Task<IActionResult> DeleteReply(Guid id, CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        bool isAdmin = _currentUserService.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;
        await _forumService.DeleteReplyAsync(id, _currentUserService.UserId.Value, isAdmin, ct);
        return Ok(new { message = "Rəy silindi." });
    }
}
