namespace AstroBackend.Application.DTOs
{
    public record ProfileDto(
    Guid UserId,
    string? FullName,
    string? AvatarUrl,
    string? BirthDate,
    string? BirthTime,
    string? BirthPlace,
    double? BirthLat,
    double? BirthLon,
    double? TzOffset,
    string? SunSign,
    string? MoonSign,
    string? Ascendant,
    string? Bio,
    DateTime CreatedAt
);

    public record UpdateProfileRequest(
        string? FullName,
        string? AvatarUrl,
        string? BirthDate,
        string? BirthTime,
        string? BirthPlace,
        double? BirthLat,
        double? BirthLon,
        string? Bio
    );
}
