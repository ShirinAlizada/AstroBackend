using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers;

public class SynastryController : BaseApiController
{
    private readonly ISynastryService _synastryService;

    public SynastryController(ISynastryService synastryService)
    {
        _synastryService = synastryService;
    }

    [HttpPost("calculate")]
    public ActionResult<SynastryResponse> Calculate([FromBody] SynastryRequest request)
    {
        var result = _synastryService.CalculateCompatibility(request);
        return Ok(result);
    }
}
