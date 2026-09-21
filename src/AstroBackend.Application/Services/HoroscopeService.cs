using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Enums;
using AstroBackend.Domain.Exceptions;

namespace AstroBackend.Application.Services;

public class HoroscopeService : IHoroscopeService
{
    private readonly IGenericRepository<Horoscope> _horoscopeRepo;
    private readonly IUnitOfWork _unitOfWork;

    public HoroscopeService(IGenericRepository<Horoscope> horoscopeRepo, IUnitOfWork unitOfWork)
    {
        _horoscopeRepo = horoscopeRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<HoroscopeDto>> GetHoroscopesAsync(string? sign, string? period, CancellationToken ct = default)
    {
        var query = _horoscopeRepo.Query();

        if (!string.IsNullOrWhiteSpace(sign))
            query = query.Where(h => h.Sign.ToLower() == sign.ToLower());

        if (!string.IsNullOrWhiteSpace(period) && Enum.TryParse<HoroscopePeriod>(period, true, out var pEnum))
            query = query.Where(h => h.Period == pEnum);

        var list = query.OrderByDescending(h => h.PeriodStart).ToList();
        return list.Select(MapToDto).ToList();
    }

    public async Task<HoroscopeDto?> GetCurrentHoroscopeAsync(string sign, string period, CancellationToken ct = default)
    {
        if (!Enum.TryParse<HoroscopePeriod>(period, true, out var pEnum))
            pEnum = HoroscopePeriod.Daily;

        var horoscope = _horoscopeRepo.Query()
            .Where(h => h.Sign.ToLower() == sign.ToLower() && h.Period == pEnum)
            .OrderByDescending(h => h.PeriodStart)
            .FirstOrDefault();

        return horoscope != null ? MapToDto(horoscope) : null;
    }

    public async Task<HoroscopeDto> CreateHoroscopeAsync(CreateHoroscopeRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<HoroscopePeriod>(request.Period, true, out var pEnum))
            pEnum = HoroscopePeriod.Daily;

        var horoscope = new Horoscope
        {
            Sign = request.Sign,
            Period = pEnum,
            PeriodStart = request.PeriodStart,
            Content = request.Content,
            Love = request.Love,
            Career = request.Career,
            Finance = request.Finance
        };

        await _horoscopeRepo.AddAsync(horoscope, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapToDto(horoscope);
    }

    public async Task<HoroscopeDto> UpdateHoroscopeAsync(Guid id, UpdateHoroscopeRequest request, CancellationToken ct = default)
    {
        var horoscope = await _horoscopeRepo.GetByIdAsync(id, ct);
        if (horoscope == null)
            throw new NotFoundException("Horoskop tapılmadı.");

        horoscope.Content = request.Content;
        horoscope.Love = request.Love;
        horoscope.Career = request.Career;
        horoscope.Finance = request.Finance;
        horoscope.UpdatedAt = DateTime.UtcNow;

        _horoscopeRepo.Update(horoscope);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapToDto(horoscope);
    }

    public async Task DeleteHoroscopeAsync(Guid id, CancellationToken ct = default)
    {
        var horoscope = await _horoscopeRepo.GetByIdAsync(id, ct);
        if (horoscope == null)
            throw new NotFoundException("Horoskop tapılmadı.");

        _horoscopeRepo.Delete(horoscope);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private static HoroscopeDto MapToDto(Horoscope h) => new(
        h.Id,
        h.Sign,
        h.Period.ToString().ToLower(),
        h.PeriodStart,
        h.Content,
        h.Love,
        h.Career,
        h.Finance
    );
}
