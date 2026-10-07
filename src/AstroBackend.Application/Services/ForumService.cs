using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AstroBackend.Application.Services;

public class ForumService : IForumService
{
    private readonly IGenericRepository<ForumTopic> _topicRepo;
    private readonly IGenericRepository<ForumReply> _replyRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IPushSubscriptionService _pushSubscriptionService;

    public ForumService(
        IGenericRepository<ForumTopic> topicRepo,
        IGenericRepository<ForumReply> replyRepo,
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IPushSubscriptionService pushSubscriptionService)
    {
        _topicRepo = topicRepo;
        _replyRepo = replyRepo;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _pushSubscriptionService = pushSubscriptionService;
    }

    public async Task<PagedResult<ForumTopicDto>> GetTopicsAsync(string? category, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var query = _topicRepo.Query().Where(t => !t.IsHidden);

        if (!string.IsNullOrWhiteSpace(category) && category != "hamısı")
            query = query.Where(t => t.Category == category);

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var totalCount = await query.CountAsync(ct);
        var topics = await query.OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        var topicIds = topics.Select(t => t.Id).ToList();

        var replyCounts = await _replyRepo.Query()
            .Where(r => topicIds.Contains(r.TopicId) && !r.IsHidden)
            .GroupBy(r => r.TopicId)
            .Select(g => new { TopicId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.TopicId, g => g.Count, ct);

        var items = topics.Select(t => new ForumTopicDto(
            t.Id,
            t.UserId,
            t.AuthorName,
            t.Category,
            t.Title,
            t.TitleEn,
            t.TitleRu,
            t.Body,
            t.BodyEn,
            t.BodyRu,
            t.IsHidden,
            t.CreatedAt,
            replyCounts.TryGetValue(t.Id, out var cnt) ? cnt : 0
        )).ToList();

        return new PagedResult<ForumTopicDto>(items, page, pageSize, totalCount);
    }

    public async Task<ForumTopicDto> GetTopicByIdAsync(Guid id, CancellationToken ct = default)
    {
        var topic = await _topicRepo.GetByIdAsync(id, ct);
        if (topic == null || topic.IsHidden)
            throw new NotFoundException("Mövzu tapılmadı.");

        var count = await _replyRepo.Query().CountAsync(r => r.TopicId == id && !r.IsHidden, ct);

        return new ForumTopicDto(
            topic.Id,
            topic.UserId,
            topic.AuthorName,
            topic.Category,
            topic.Title,
            topic.TitleEn,
            topic.TitleRu,
            topic.Body,
            topic.BodyEn,
            topic.BodyRu,
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
            TitleEn = string.IsNullOrWhiteSpace(request.TitleEn) ? null : request.TitleEn.Trim(),
            TitleRu = string.IsNullOrWhiteSpace(request.TitleRu) ? null : request.TitleRu.Trim(),
            Body = request.Body,
            BodyEn = string.IsNullOrWhiteSpace(request.BodyEn) ? null : request.BodyEn.Trim(),
            BodyRu = string.IsNullOrWhiteSpace(request.BodyRu) ? null : request.BodyRu.Trim(),
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
            topic.TitleEn,
            topic.TitleRu,
            topic.Body,
            topic.BodyEn,
            topic.BodyRu,
            topic.IsHidden,
            topic.CreatedAt,
            0
        );
    }

    public async Task<IReadOnlyList<ForumReplyDto>> GetRepliesAsync(Guid topicId, CancellationToken ct = default)
    {
        var replies = await _replyRepo.Query()
            .Where(r => r.TopicId == topicId && !r.IsHidden)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(ct);

        return replies.Select(r => new ForumReplyDto(
            r.Id,
            r.TopicId,
            r.UserId,
            r.AuthorName,
            r.Body,
            r.BodyEn,
            r.BodyRu,
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
            BodyEn = string.IsNullOrWhiteSpace(request.BodyEn) ? null : request.BodyEn.Trim(),
            BodyRu = string.IsNullOrWhiteSpace(request.BodyRu) ? null : request.BodyRu.Trim(),
            IsHidden = false
        };

        await _replyRepo.AddAsync(reply, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Mövzu sahibinə bildiriş — yalnız başqası cavab verdikdə (özünə bildiriş göndərilmir)
        if (topic.UserId != userId)
        {
            var preview = request.Body.Length > 120 ? request.Body[..120] + "…" : request.Body;
            await _notificationService.CreateAsync(
                topic.UserId,
                "forum_reply",
                "Mövzuna yeni cavab",
                $"{authorName}: {preview}",
                $"/forum/{topicId}",
                ct);

            // Brauzer Web Push — tab bağlı olsa belə xəbərdarlıq (best-effort, DB bildirişinə əlavə kanal).
            await _pushSubscriptionService.NotifyUserAsync(
                topic.UserId,
                "Mövzuna yeni cavab",
                $"{authorName}: {preview}",
                $"/forum/{topicId}",
                ct);
        }

        return new ForumReplyDto(
            reply.Id,
            reply.TopicId,
            reply.UserId,
            reply.AuthorName,
            reply.Body,
            reply.BodyEn,
            reply.BodyRu,
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

