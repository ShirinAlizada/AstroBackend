using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Exceptions;

namespace AstroBackend.Application.Services;

public class JournalService : IJournalService
{
    private readonly IGenericRepository<JournalEntry> _journalRepo;
    private readonly IUnitOfWork _unitOfWork;

    public JournalService(IGenericRepository<JournalEntry> journalRepo, IUnitOfWork unitOfWork)
    {
        _journalRepo = journalRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<JournalEntryDto>> GetMyEntriesAsync(Guid userId, CancellationToken ct = default)
    {
        var list = _journalRepo.Query()
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.EntryDate)
            .ToList();

        return list.Select(MapToDto).ToList();
    }

    public async Task<JournalEntryDto> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var entry = await _journalRepo.GetByIdAsync(id, ct);
        if (entry == null || entry.UserId != userId)
            throw new NotFoundException("Jurnal qeydi tapılmadı.");

        return MapToDto(entry);
    }

    public async Task<JournalEntryDto> CreateEntryAsync(Guid userId, CreateJournalEntryRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            throw new BadRequestException("Jurnal mətni mütləq daxil edilməlidir.");

        var entry = new JournalEntry
        {
            UserId = userId,
            EntryDate = request.EntryDate,
            Mood = Math.Clamp(request.Mood, 1, 5),
            Title = request.Title,
            Content = request.Content,
            TransitNote = request.TransitNote
        };

        await _journalRepo.AddAsync(entry, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapToDto(entry);
    }

    public async Task<JournalEntryDto> UpdateEntryAsync(Guid id, Guid userId, UpdateJournalEntryRequest request, CancellationToken ct = default)
    {
        var entry = await _journalRepo.GetByIdAsync(id, ct);
        if (entry == null || entry.UserId != userId)
            throw new NotFoundException("Jurnal qeydi tapılmadı.");

        entry.EntryDate = request.EntryDate;
        entry.Mood = Math.Clamp(request.Mood, 1, 5);
        entry.Title = request.Title;
        entry.Content = request.Content;
        entry.TransitNote = request.TransitNote;
        entry.UpdatedAt = DateTime.UtcNow;

        _journalRepo.Update(entry);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapToDto(entry);
    }

    public async Task DeleteEntryAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var entry = await _journalRepo.GetByIdAsync(id, ct);
        if (entry == null || entry.UserId != userId)
            throw new NotFoundException("Jurnal qeydi tapılmadı.");

        _journalRepo.Delete(entry);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private static JournalEntryDto MapToDto(JournalEntry j) => new(
        j.Id,
        j.EntryDate,
        j.Mood,
        j.Title,
        j.Content,
        j.TransitNote,
        j.CreatedAt
    );
}
