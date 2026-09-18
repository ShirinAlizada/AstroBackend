namespace AstroBackend.Application.DTOs
{
    public record AstrologerDto(
     Guid Id,
     Guid? UserId,
     string DisplayName,
     string? Title,
     string? Bio,
     List<string> Specialties,
     List<string> Languages,
     int PriceAzn,
     decimal Rating,
     string? AvatarUrl,
     bool Verified
 );
    public record CreateAstrologerRequest(
    string DisplayName,
    string? Title,
    string? Bio,
    List<string> Specialties,
    List<string> Languages,
    int PriceAzn,
    string? AvatarUrl
);
    public record UpdateAstrologerRequest(
    string DisplayName,
    string? Title,
    string? Bio,
    List<string> Specialties,
    List<string> Languages,
    int PriceAzn,
    string? AvatarUrl,
    bool? Verified
);
}
