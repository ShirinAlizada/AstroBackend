using AstroBackend.Application.Astrology;
using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Exceptions;

namespace AstroBackend.Application.Services;

public class SynastryService : ISynastryService
{
    private readonly INatalChartService _natalChartService;

    public SynastryService(INatalChartService natalChartService)
    {
        _natalChartService = natalChartService;
    }

    public SynastryResponse CalculateCompatibility(SynastryRequest request)
    {
        string signA = request.SignA;
        string signB = request.SignB;

        if (!string.IsNullOrWhiteSpace(request.DateA) && DateTime.TryParse(request.DateA, out var da))
            signA = AstrologyEngine.SunSignFromDate(da);

        if (!string.IsNullOrWhiteSpace(request.DateB) && DateTime.TryParse(request.DateB, out var db))
            signB = AstrologyEngine.SunSignFromDate(db);

        return AstrologyEngine.ComputeSynastry(signA, signB, request.Lang);
    }

    public async Task<SynastryResponse> CalculateFromChartsAsync(SynastryChartRequest request, Guid? currentUserId, CancellationToken ct = default)
    {
        NatalChartResponse chartA;
        if (request.PersonA is { } personA)
        {
            chartA = AstrologyEngine.ComputeNatalChart(personA.Date, personA.Time, personA.Latitude, personA.Longitude);
        }
        else if (currentUserId.HasValue)
        {
            // Frontend-in "loginli istifadəçi üçün öz saxlanılmış xəritəsini istifadə et" davranışı.
            chartA = await _natalChartService.GetMyChartAsync(currentUserId.Value, ct);
        }
        else
        {
            throw new BadRequestException("Birinci şəxsin doğum məlumatları göndərilməlidir və ya hesaba daxil olunmalıdır.");
        }

        var chartB = AstrologyEngine.ComputeNatalChart(request.PersonB.Date, request.PersonB.Time, request.PersonB.Latitude, request.PersonB.Longitude);

        return AstrologyEngine.ComputeSynastryFromCharts(chartA, chartB, request.Lang);
    }
}
