using AstroBackend.Application.Astrology;
using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Exceptions;

namespace AstroBackend.Application.Services;

public class ProfileService : IProfileService
{
    private readonly IGenericRepository<Profile> _profileRepo;
    private readonly IGenericRepository<User> _userRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ProfileService(IGenericRepository<Profile> profileRepo, IGenericRepository<User> userRepo, IUnitOfWork unitOfWork)
    {
        _profileRepo = profileRepo;
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProfileDto> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile == null)
            throw new NotFoundException("Profil tapılmadı.");

        return MapToDto(profile);
    }

    public async Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var profile = await _profileRepo.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile == null)
        {
            profile = new Profile { UserId = userId };
            await _profileRepo.AddAsync(profile, ct);
        }

        profile.FullName = request.FullName ?? profile.FullName;
        profile.AvatarUrl = request.AvatarUrl ?? profile.AvatarUrl;
        profile.BirthDate = request.BirthDate ?? profile.BirthDate;
        profile.BirthTime = request.BirthTime ?? profile.BirthTime;
        profile.BirthPlace = request.BirthPlace ?? profile.BirthPlace;
        profile.BirthLat = request.BirthLat ?? profile.BirthLat;
        profile.BirthLon = request.BirthLon ?? profile.BirthLon;
        profile.Bio = request.Bio ?? profile.Bio;

        if (!string.IsNullOrWhiteSpace(profile.BirthDate))
        {
            var chart = AstrologyEngine.ComputeNatalChart(profile.BirthDate, profile.BirthTime ?? "12:00", profile.BirthLat ?? 40.4093, profile.BirthLon ?? 49.8671);
            profile.SunSign = chart.Sun;
            profile.MoonSign = chart.Moon;
            profile.Ascendant = chart.Ascendant;
        }

        profile.UpdatedAt = DateTime.UtcNow;
        _profileRepo.Update(profile);

        // Update FullName on User as well if provided
        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            var user = await _userRepo.GetByIdAsync(userId, ct);
            if (user != null)
            {
                user.FullName = request.FullName;
                _userRepo.Update(user);
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return MapToDto(profile);
    }

    private static ProfileDto MapToDto(Profile p) => new(
        p.UserId,
        p.FullName,
        p.AvatarUrl,
        p.BirthDate,
        p.BirthTime,
        p.BirthPlace,
        p.BirthLat,
        p.BirthLon,
        p.TzOffset,
        p.SunSign,
        p.MoonSign,
        p.Ascendant,
        p.Bio,
        p.CreatedAt
    );
}
