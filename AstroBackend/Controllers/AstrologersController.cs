using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers;

public class AstrologersController : BaseApiController
{
    private readonly IAstrologerService _astrologerService;

    public AstrologersController(IAstrologerService astrologerService)
    {
        _astrologerService = astrologerService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AstrologerDto>>> GetVerifiedAstrologers(CancellationToken ct)
    {
        var list = await _astrologerService.GetVerifiedAstrologersAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AstrologerDto>> GetById(Guid id, CancellationToken ct)
    {
        var astrologer = await _astrologerService.GetByIdAsync(id, ct);
        return Ok(astrologer);
    }
}
