using System;
using System.Collections.Generic;
using System.Text;

namespace AstroBackend.Application.DTOs
{
    public record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string? Body,
    string? Link,
    bool IsRead,
    DateTime CreatedAt
    );
}
