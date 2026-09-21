using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Exceptions;

namespace AstroBackend.Application.Services;

public class ForumService : IForumService
{
    private readonly IGenericRepository<ForumTopic> _topicRepo;
    private readonly IGenericRepository<ForumReply> _replyRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ForumService(
        IGenericRepository<ForumTopic> topicRepo,
        IGenericRepository<ForumReply> replyRepo,
        IUnitOfWork unitOfWork)
    {
        _topicRepo = topicRepo;
        _replyRepo = replyRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ForumTopicDto>> GetTopicsAsync(string? category, CancellationToken ct = default)
    {
        var query = _topicRepo.Query().Where(t => !t.IsHidden);

        if (!string.IsNullOrWhiteSpace(category) && category != "hamısı")
            query = query.Where(t => t.Category == category);

        var topics = query.OrderByDescending(t => t.CreatedAt).ToList();
        var topicIds = topics.Select(t => t.Id).ToList();

        var replyCounts = _replyRepo.Query()
            .Where(r => topicIds.Contains(r.TopicId) && !r.IsHidden)
            .GroupBy(r => r.TopicId)
            .Select(g => new { TopicId = g.Key, Count = g.Count() })
            .ToDictionary(g => g.TopicId, g => g.Count);

        return topics.Select(t => new ForumTopicDto(
            t.Id,
            t.UserId,
            t.AuthorName,
            t.Category,
            t.Title,
            t.Body,
            t.IsHidden,
            t.CreatedAt,
            replyCounts.TryGetValue(t.Id, out var cnt) ? cnt : 0
        )).ToList();
    }

    public async Task<ForumTopicDto> GetTopicByIdAsync(Guid id, CancellationToken ct = default)
    {
        var topic = await _topicRepo.GetByIdAsync(id, ct);
        if (topic == null || topic.IsHidden)
            throw new NotFoundException("Mövzu tapılmadı.");

        var count = _replyRepo.Query().Count(r => r.TopicId == id && !r.IsHidden);

        return new ForumTopicDto(
            topic.Id,
            topic.UserId,
            topic.AuthorName,
            topic.Category,
            topic.Title,
            topic.Body,
            topic.IsHidden,
            topic.CreatedAt,
            count
        );
    }

    public async Task<ForumTopicDto> CreateTopicAsync(Guid userId, string authorName, CreateTopicRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body))
            throw new BadRequestException("Başlıq və mətn mütləq daxil edilməlidir.");

        var topic = new ForumTopic
        {
            UserId = userId,
            AuthorName = authorName,
            Category = string.IsNullOrWhiteSpace(request.Category) ? "ümumi" : request.Category,
            Title = request.Title,
            Body = request.Body,
            IsHidden = false
        };

        await _topicRepo.AddAsync(topic, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new ForumTopicDto(
            topic.Id,
            topic.UserId,
            topic.AuthorName,
            topic.Category,
            topic.Title,
            topic.Body,
            topic.IsHidden,
            topic.CreatedAt,
            0
        );
    }

    public async Task<IReadOnlyList<ForumReplyDto>> GetRepliesAsync(Guid topicId, CancellationToken ct = default)
    {
        var replies = _replyRepo.Query()
            .Where(r => r.TopicId == topicId && !r.IsHidden)
            .OrderBy(r => r.CreatedAt)
            .ToList();

        return replies.Select(r => new ForumReplyDto(
            r.Id,
            r.TopicId,
            r.UserId,
            r.AuthorName,
            r.Body,
            r.IsHidden,
            r.CreatedAt
        )).ToList();
    }

    public async Task<ForumReplyDto> CreateReplyAsync(Guid topicId, Guid userId, string authorName, CreateReplyRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Body))
            throw new BadRequestException("Rəy mətni daxil edilməlidir.");

        var topic = await _topicRepo.GetByIdAsync(topicId, ct);
        if (topic == null || topic.IsHidden)
            throw new NotFoundException("Mövzu tapılmadı.");

        var reply = new ForumReply
        {
            TopicId = topicId,
            UserId = userId,
            AuthorName = authorName,
            Body = request.Body,
            IsHidden = false
        };

        await _replyRepo.AddAsync(reply, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new ForumReplyDto(
            reply.Id,
            reply.TopicId,
            reply.UserId,
            reply.AuthorName,
            reply.Body,
            reply.IsHidden,
            reply.CreatedAt
        );
    }

    public async Task SetTopicHiddenAsync(Guid topicId, bool isHidden, CancellationToken ct = default)
    {
        var topic = await _topicRepo.GetByIdAsync(topicId, ct);
        if (topic == null) throw new NotFoundException("Mövzu tapılmadı.");

        topic.IsHidden = isHidden;
        topic.UpdatedAt = DateTime.UtcNow;
        _topicRepo.Update(topic);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task SetReplyHiddenAsync(Guid replyId, bool isHidden, CancellationToken ct = default)
    {
        var reply = await _replyRepo.GetByIdAsync(replyId, ct);
        if (reply == null) throw new NotFoundException("Rəy tapılmadı.");

        reply.IsHidden = isHidden;
        reply.UpdatedAt = DateTime.UtcNow;
        _replyRepo.Update(reply);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteTopicAsync(Guid topicId, Guid userId, bool isAdmin, CancellationToken ct = default)
    {
        var topic = await _topicRepo.GetByIdAsync(topicId, ct);
        if (topic == null) throw new NotFoundException("Mövzu tapılmadı.");

        if (!isAdmin && topic.UserId != userId)
            throw new ForbiddenException("Bu mövzunu silməyə icazəniz yoxdur.");

        _topicRepo.Delete(topic);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteReplyAsync(Guid replyId, Guid userId, bool isAdmin, CancellationToken ct = default)
    {
        var reply = await _replyRepo.GetByIdAsync(replyId, ct);
        if (reply == null) throw new NotFoundException("Rəy tapılmadı.");

        if (!isAdmin && reply.UserId != userId)
            throw new ForbiddenException("Bu rəyi silməyə icazəniz yoxdur.");

        _replyRepo.Delete(reply);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
