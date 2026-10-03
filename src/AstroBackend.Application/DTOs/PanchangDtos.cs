
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

    /// <summary>Bugünkü iki planet arasında tapılan aspekt (Qərb astrologiyası, bax: PanchangEngine).</summary>
    public record AspectHitDto(
        string PlanetA,
        string PlanetB,
        string SignA,
        string SignB,
        string Aspect
    );

    /// <summary>
    /// 2026-10 yenidənqurmasından sonra bu cavab artıq Vedik Panchang sahələrini (Tithi/Nakşatra/
    /// Yoga/Karana) daşımır — əvəzində frontend-dəki `daily-guide.ts`/`DailyGuideContext` ilə
    /// paralel olaraq Ay bürcü/ünsürü, gün hakimi planet, gün rəngi və bugünkü planet aspektləri
    /// daşıyır. Ad (`PanchangResponse`) marşrut/DI uyğunluğu üçün saxlanılıb.
    /// </summary>
    public record PanchangResponse(
        string DateISO,
        string MoonSign,
        string MoonElement,
        string DayRuler,
        int Weekday,
        string DayColor,
        List<AspectHitDto> Aspects,
        List<GuidanceItemDto> Guidance
    );

}
