using System;

namespace AstroBackend.Application.DTOs
{
    public record ContactMessageDto(
        Guid Id,
        string Name,
        string Email,
        string Message,
        bool IsRead,
        DateTime CreatedAt
    );

    public record CreateContactMessageRequest(
        string Name,
        string Email,
        string Message
    );
}
