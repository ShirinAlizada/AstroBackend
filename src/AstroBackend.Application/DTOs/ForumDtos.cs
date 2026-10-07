namespace AstroBackend.Application.DTOs
{
    public record ForumTopicDto(
        Guid Id,
        Guid UserId,
        string AuthorName,
        string Category,
        string Title,
        string? TitleEn,
        string? TitleRu,
        string Body,
        string? BodyEn,
        string? BodyRu,
        bool IsHidden,
        DateTime CreatedAt,
        int ReplyCount
    );

    public record CreateTopicRequest(
        string Category,
        string Title,
        string Body,
        string? TitleEn = null,
        string? TitleRu = null,
        string? BodyEn = null,
        string? BodyRu = null
    );

    public record ForumReplyDto(
        Guid Id,
        Guid TopicId,
        Guid UserId,
        string AuthorName,
        string Body,
        string? BodyEn,
        string? BodyRu,
        bool IsHidden,
        DateTime CreatedAt
    );

    public record CreateReplyRequest(
        string Body,
        string? BodyEn = null,
        string? BodyRu = null
    );

    /// <summary>
    /// Admin tərəfindən (real istifadəçi hesabı olmadan) forum mövzusu yaratmaq üçün —
    /// məs. seed/redaktə məqsədli invented-author məzmun. AuthorName sərbəst mətn kimi
    /// göstərilir, mövzu isə FK tələbinə görə cari admin istifadəçisinin Id-sinə bağlanır
    /// (bax ForumService.CreateTopicAsync — userId və authorName artıq ayrı parametrlərdir).
    /// </summary>
    public record AdminCreateTopicRequest(
        string AuthorName,
        string Category,
        string Title,
        string Body,
        string? TitleEn = null,
        string? TitleRu = null,
        string? BodyEn = null,
        string? BodyRu = null
    );

    public record AdminCreateReplyRequest(
        string AuthorName,
        string Body,
        string? BodyEn = null,
        string? BodyRu = null
    );

    public record AdminUserDto(
        Guid Id,
        string Email,
        string FullName,
        string Role,
        bool IsActive,
        DateTime CreatedAt,
        string? SunSign,
        string? BirthPlace
    );
}
