using AstroBackend.Application.Astrology;
using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;

namespace AstroBackend.Application.Services;

public class SynastryService : ISynastryService
{
    public SynastryResponse CalculateCompatibility(SynastryRequest request)
    {
        string signA = request.SignA;
        string signB = request.SignB;

        if (!string.IsNullOrWhiteSpace(request.DateA) && DateTime.TryParse(request.DateA, out var da))
            signA = AstrologyEngine.SunSignFromDate(da);

        if (!string.IsNullOrWhiteSpace(request.DateB) && DateTime.TryParse(request.DateB, out var db))
            signB = AstrologyEngine.SunSignFromDate(db);

        return AstrologyEngine.ComputeSynastry(signA, signB);
    }
}
