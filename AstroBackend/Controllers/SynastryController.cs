using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers;

public class SynastryController : BaseApiController
{
    private readonly ISynastryService _synastryService;
    private readonly ICurrentUserService _currentUserService;

    public SynastryController(ISynastryService synastryService, ICurrentUserService currentUserService)
    {
        _synastryService = synastryService;
        _currentUserService = currentUserService;
    }

    /// <summary>Sürətli (yalnız Günəş bürcü əsaslı) təxmini uyğunluq — geriyə uyğunluq üçün saxlanılıb.</summary>
    [HttpPost("calculate")]
    public ActionResult<SynastryResponse> Calculate([FromBody] SynastryRequest request)
    {
        var result = _synastryService.CalculateCompatibility(request);
        return Ok(result);
    }

    /// <summary>
    /// Tam natal xəritə əsaslı uyğunluq: hər iki şəxsin doğum tarixi/saatı/yerindən Günəş, Ay,
    /// Venera, Mars, Merkuri və Ascendant üçün faktiki bürcləri hesablayıb müqayisə edir
    /// (frontend-dəki Cütlük Xəritəsi səhifəsi ilə eyni məntiq). PersonA göndərilməzsə və
    /// istifadəçi daxil olubsa, onun saxlanılmış natal xəritəsi istifadə olunur.
    /// </summary>
    [HttpPost("calculate-chart")]
    public async Task<ActionResult<SynastryResponse>> CalculateFromCharts([FromBody] SynastryChartRequest request, CancellationToken ct)
    {
        var result = await _synastryService.CalculateFromChartsAsync(request, _currentUserService.UserId, ct);
        return Ok(result);
    }
}