
namespace AstroBackend.Application.DTOs
{
    public record NumerologyRequest(
    string FullName,
    string BirthDate, // YYYY-MM-DD
    string? Lang = null,
    int? ForYear = null
);

    public record NumerologyItemDto(
        int Number,
        string Name,
        string Title,
        string Meaning,
        List<string> Keywords,
        List<int> Compatible
    );

    public record NumerologyResponse(
        int LifePath,
        int Destiny,
        int SoulUrge,
        int Personality,
        int Birthday,
        int Maturity,
        int PersonalYear,
        string PersonalYearTheme,
        List<NumerologyItemDto> Details
    );

}
