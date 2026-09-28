namespace AstroBackend.Application.DTOs
{
    public record SynastryRequest(
      string SignA,
      string SignB,
      string? DateA = null,
      string? DateB = null
  );

    /// <summary>
    /// Tam natal xəritə əsaslı uyğunluq sorğusu: hər iki şəxsin doğum tarixi,
    /// saatı və yeri göndərilir ki, yalnız Günəş bürcü deyil, Ay/Venera/Mars/
    /// Merkuri və Ascendant də faktiki hesablanmış mövqelərdən müqayisə edilsin.
    /// PersonA boş buraxılarsa və istifadəçi daxil olubsa, onun saxlanılmış
    /// natal xəritəsi istifadə olunur (frontend-dəki "öz xəritəm" davranışı ilə eyni).
    /// </summary>
    public record SynastryChartRequest(
        CalculateChartRequest? PersonA,
        CalculateChartRequest PersonB
    );

    public record PlanetPairDetailDto(
        string Planet,
        string Symbol,
        string SignA,
        string SignB,
        string ElementA,
        string ElementB,
        string Aspect,
        int Score
    );

    public record SynastryResponse(
        int Overall,
        int Love,
        int Friendship,
        int Communication,
        List<string> Notes,
        List<PlanetPairDetailDto> Details
    );
}
