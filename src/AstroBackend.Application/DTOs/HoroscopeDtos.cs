namespace AstroBackend.Application.DTOs
{
    public record HoroscopeDto(
     Guid Id,
     string Sign,
     string Period,
     DateTime PeriodStart,
     string Content,
     int Love,
     int Career,
     int Finance
 );

    public record CreateHoroscopeRequest(
        string Sign,
        string Period,
        DateTime PeriodStart,
        string Content,
        int Love,
        int Career,
        int Finance
    );

    public record UpdateHoroscopeRequest(
        string Content,
        int Love,
        int Career,
        int Finance
    );
}
