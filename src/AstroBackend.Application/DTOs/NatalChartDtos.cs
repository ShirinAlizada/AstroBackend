namespace AstroBackend.Application.DTOs
{
    public record PlanetPositionDto(string Name, string Sign, int Degree, int? House, bool Retrograde);
    public record HousePositionDto(int Index, string Sign, int Degree);

    public record NatalChartResponse(
        List<PlanetPositionDto> Planets,
        List<HousePositionDto> Houses,
        string Sun,
        string Moon,
        string Ascendant,
        string Midheaven
    );

    public record CalculateChartRequest(
        string Date, // YYYY-MM-DD
        string Time, // HH:mm
        double Latitude,
        double Longitude
    );

    public record SaveNatalChartRequest(
        string ChartJson
    );
}
