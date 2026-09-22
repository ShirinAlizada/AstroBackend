using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers;

public class NatalChartsController : BaseApiController
{
    private readonly INatalChartService _chartService;
    private readonly ICurrentUserService _currentUserService;

    public NatalChartsController(INatalChartService chartService, ICurrentUserService currentUserService)
    {
        _chartService = chartService;
        _currentUserService = currentUserService;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<NatalChartResponse>> GetMyChart(CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        var chart = await _chartService.GetMyChartAsync(_currentUserService.UserId.Value, ct);
        return Ok(chart);
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<NatalChartResponse>> CalculateChart([FromBody] CalculateChartRequest request)
    {
        var chart = await _chartService.CalculateChartAsync(request);
        return Ok(chart);
    }

    [Authorize]
    [HttpPost("save")]
    public async Task<IActionResult> SaveMyChart([FromBody] SaveNatalChartRequest request, CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        await _chartService.SaveMyChartAsync(_currentUserService.UserId.Value, request, ct);
        return Ok(new { message = "Natal xəritə uğurla saxlanıldı." });
    }
}
