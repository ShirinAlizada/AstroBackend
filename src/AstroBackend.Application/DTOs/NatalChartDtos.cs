namespace AstroBackend.Application.DTOs
{
    public record PlanetPositionDto(
     string Name,
     string Sign,
     int Degree,
     int Minute,
     int? House,
     bool Retrograde
 );

    public record HousePositionDto(
        int Index,
        string Sign,
        int Degree,
        int Minute
    );

    public record AnglePointDto(
        string Sign,
        int Degree,
        int Minute
    );

    public record NatalChartResponse(
        List<PlanetPositionDto> Planets,
        List<HousePositionDto> Houses,
        string Sun,
        string Moon,
        AnglePointDto Ascendant,
        AnglePointDto Midheaven
    );

    public record CalculateChartRequest(
        string Date, // YYYY-MM-DD
        string Time, // HH:mm
        double Latitude = 40.4093,
        double Longitude = 49.8671
    );

    public record SaveNatalChartRequest(
        string ChartJson
    );

}
