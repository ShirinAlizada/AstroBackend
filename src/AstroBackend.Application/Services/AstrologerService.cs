using System.Text.Json;
using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Exceptions;

namespace AstroBackend.Application.Services;

public class AstrologerService : IAstrologerService
{
    private readonly IGenericRepository<Astrologer> _astrologerRepo;
    private readonly IUnitOfWork _unitOfWork;

    public AstrologerService(IGenericRepository<Astrologer> astrologerRepo, IUnitOfWork unitOfWork)
    {
        _astrologerRepo = astrologerRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<AstrologerDto>> GetVerifiedAstrologersAsync(CancellationToken ct = default)
    {
        var list = await _astrologerRepo.FindAsync(a => a.Verified, ct);
        return list.OrderByDescending(a => a.Rating).Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<AstrologerDto>> GetAllAstrologersAsync(CancellationToken ct = default)
    {
        var list = await _astrologerRepo.GetAllAsync(ct);
        return list.OrderByDescending(a => a.CreatedAt).Select(MapToDto).ToList();
    }

    public async Task<AstrologerDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var astrologer = await _astrologerRepo.GetByIdAsync(id, ct);
        if (astrologer == null)
            throw new NotFoundException("Astroloq tapılmadı.");

        return MapToDto(astrologer);
    }

    public async Task<AstrologerDto> CreateAstrologerAsync(Guid? userId, CreateAstrologerRequest request, CancellationToken ct = default)
    {
        var astrologer = new Astrologer
        {
            UserId = userId,
            DisplayName = request.DisplayName,
            Title = request.Title,
            Bio = request.Bio,
            SpecialtiesJson = JsonSerializer.Serialize(request.Specialties),
            LanguagesJson = JsonSerializer.Serialize(request.Languages),
            PriceAzn = request.PriceAzn,
            AvatarUrl = request.AvatarUrl,
            Rating = 5.0m,
            Verified = false
        };

        await _astrologerRepo.AddAsync(astrologer, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapToDto(astrologer);
    }

    public async Task<AstrologerDto> UpdateAstrologerAsync(Guid id, UpdateAstrologerRequest request, CancellationToken ct = default)
    {
        var astrologer = await _astrologerRepo.GetByIdAsync(id, ct);
        if (astrologer == null)
            throw new NotFoundException("Astroloq tapılmadı.");

        astrologer.DisplayName = request.DisplayName;
        astrologer.Title = request.Title;
        astrologer.Bio = request.Bio;
        astrologer.SpecialtiesJson = JsonSerializer.Serialize(request.Specialties);
        astrologer.LanguagesJson = JsonSerializer.Serialize(request.Languages);
        astrologer.PriceAzn = request.PriceAzn;
        astrologer.AvatarUrl = request.AvatarUrl;

        if (request.Verified.HasValue)
            astrologer.Verified = request.Verified.Value;

        astrologer.UpdatedAt = DateTime.UtcNow;
        _astrologerRepo.Update(astrologer);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapToDto(astrologer);
    }

    public async Task VerifyAstrologerAsync(Guid id, bool verified, CancellationToken ct = default)
    {
        var astrologer = await _astrologerRepo.GetByIdAsync(id, ct);
        if (astrologer == null)
            throw new NotFoundException("Astroloq tapılmadı.");

        astrologer.Verified = verified;
        astrologer.UpdatedAt = DateTime.UtcNow;
        _astrologerRepo.Update(astrologer);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAstrologerAsync(Guid id, CancellationToken ct = default)
    {
        var astrologer = await _astrologerRepo.GetByIdAsync(id, ct);
        if (astrologer == null)
            throw new NotFoundException("Astroloq tapılmadı.");

        _astrologerRepo.Delete(astrologer);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private static AstrologerDto MapToDto(Astrologer a)
    {
        List<string> specs = [];
        List<string> langs = [];
        try { specs = JsonSerializer.Deserialize<List<string>>(a.SpecialtiesJson) ?? []; } catch { }
        try { langs = JsonSerializer.Deserialize<List<string>>(a.LanguagesJson) ?? []; } catch { }

        return new AstrologerDto(
            a.Id,
            a.UserId,
            a.DisplayName,
            a.Title,
            a.Bio,
            specs,
            langs,
            a.PriceAzn,
            a.Rating,
            a.AvatarUrl,
            a.Verified
        );
    }
}
