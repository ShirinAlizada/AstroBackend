
namespace AstroBackend.Application.DTOs
{
    public record ChatThreadDto(
    Guid Id,
    Guid UserId,
    string Title,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

    public record CreateThreadRequest(
        string? Title
    );

    public record ChatMessageDto(
        Guid Id,
        Guid ThreadId,
        Guid UserId,
        string Role,
        string Content,
        DateTime CreatedAt
    );

    public record SendMessageRequest(
        string Message
    );

}
