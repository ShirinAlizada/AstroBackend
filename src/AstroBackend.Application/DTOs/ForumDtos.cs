namespace AstroBackend.Application.DTOs
{
    public record ForumTopicDto(
        Guid Id,
        Guid UserId,
        string AuthorName,
        string Category,
        string Title,
        string Body,
        bool IsHidden,
        DateTime CreatedAt,
        int ReplyCount
    );

    public record CreateTopicRequest(
        string Category,
        string Title,
        string Body
    );

    public record ForumReplyDto(
        Guid Id,
        Guid TopicId,
        Guid UserId,
        string AuthorName,
        string Body,
        bool IsHidden,
        DateTime CreatedAt
    );

    public record CreateReplyRequest(
        string Body
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
