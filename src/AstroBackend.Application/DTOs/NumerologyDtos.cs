
namespace AstroBackend.Application.DTOs
{
    public record NumerologyRequest(
     string FullName,
     string BirthDate // YYYY-MM-DD
 );

    public record NumerologyItemDto(
        int Number,
        string Name,
        string Title,
        string Meaning
    );

    public record NumerologyResponse(
        int LifePath,
        int Destiny,
        int SoulUrge,
        int Personality,
        int Birthday,
        List<NumerologyItemDto> Details
    );

}
