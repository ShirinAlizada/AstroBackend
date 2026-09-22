using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers;

[Authorize]
public class JournalController : BaseApiController
{
    private readonly IJournalService _journalService;
    private readonly ICurrentUserService _currentUserService;

    public JournalController(IJournalService journalService, ICurrentUserService currentUserService)
    {
        _journalService = journalService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<JournalEntryDto>>> GetMyEntries(CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        var list = await _journalService.GetMyEntriesAsync(_currentUserService.UserId.Value, ct);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JournalEntryDto>> GetById(Guid id, CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        var entry = await _journalService.GetByIdAsync(id, _currentUserService.UserId.Value, ct);
        return Ok(entry);
    }

    [HttpPost]
    public async Task<ActionResult<JournalEntryDto>> CreateEntry([FromBody] CreateJournalEntryRequest request, CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        var created = await _journalService.CreateEntryAsync(_currentUserService.UserId.Value, request, ct);
        return Ok(created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<JournalEntryDto>> UpdateEntry(Guid id, [FromBody] UpdateJournalEntryRequest request, CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        var updated = await _journalService.UpdateEntryAsync(id, _currentUserService.UserId.Value, request, ct);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEntry(Guid id, CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        await _journalService.DeleteEntryAsync(id, _currentUserService.UserId.Value, ct);
        return Ok(new { message = "Jurnal qeydi silindi." });
    }
}
