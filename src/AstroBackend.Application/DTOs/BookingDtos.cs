namespace AstroBackend.Application.DTOs
{
    public record BookingDto(
     Guid Id,
     Guid UserId,
     Guid AstrologerId,
     string AstrologerName,
     string? AstrologerTitle,
     string SessionType,
     DateTime ScheduledAt,
     string Status,
     string? Note,
     DateTime CreatedAt
 );

    public record CreateBookingRequest(
        Guid AstrologerId,
        string SessionType,
        DateTime ScheduledAt,
        string? Note
    );

    public record UpdateBookingStatusRequest(
        string Status
    );
}
