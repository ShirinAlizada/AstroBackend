using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers;

public class HoroscopesController : BaseApiController
{
    private readonly IHoroscopeService _horoscopeService;

    public HoroscopesController(IHoroscopeService horoscopeService)
    {
        _horoscopeService = horoscopeService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HoroscopeDto>>> GetHoroscopes(
        [FromQuery] string? sign,
        [FromQuery] string? period,
        CancellationToken ct)
    {
        var list = await _horoscopeService.GetHoroscopesAsync(sign, period, ct);
        return Ok(list);
    }

    [HttpGet("current")]
    public async Task<ActionResult<HoroscopeDto>> GetCurrentHoroscope(
        [FromQuery] string sign,
        [FromQuery] string period = "daily",
        CancellationToken ct = default)
    {
        var horoscope = await _horoscopeService.GetCurrentHoroscopeAsync(sign, period, ct);
        if (horoscope == null) return NotFound(new { message = "Bu bürc və dövr üçün proqnoz tapılmadı." });
        return Ok(horoscope);
    }
}
