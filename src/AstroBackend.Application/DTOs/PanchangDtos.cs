
namespace AstroBackend.Application.DTOs
{
    public record PanchangRequest(
    DateTime? Date,
    double Latitude = 40.4093,
    double Longitude = 49.8671
);

    public record GuidanceItemDto(
        string Category,
        string Verdict, // əlverişli, neytral, ehtiyatlı ol
        string Reason
    );

    public record PanchangResponse(
        string DateISO,
        int TithiIndex,
        string TithiName,
        string Paksha,
        string TithiType,
        string NakshatraName,
        string NakshatraQuality,
        string YogaName,
        string KaranaName,
        int Weekday,
        string DayColor,
        List<GuidanceItemDto> Guidance
    );

}
