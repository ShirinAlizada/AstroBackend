using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Enums;
using AstroBackend.Domain.Exceptions;

namespace AstroBackend.Application.Services;

public class AdminService : IAdminService
{
    private readonly IGenericRepository<User> _userRepo;
    private readonly IGenericRepository<Profile> _profileRepo;
    private readonly IUnitOfWork _unitOfWork;

    public AdminService(IGenericRepository<User> userRepo, IGenericRepository<Profile> profileRepo, IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _profileRepo = profileRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<AdminUserDto>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var users = _userRepo.Query().OrderByDescending(u => u.CreatedAt).ToList();
        var userIds = users.Select(u => u.Id).ToList();

        var profiles = (await _profileRepo.FindAsync(p => userIds.Contains(p.UserId), ct))
            .ToDictionary(p => p.UserId, p => p);

        return users.Select(u =>
        {
            profiles.TryGetValue(u.Id, out var p);
            return new AdminUserDto(
                u.Id,
                u.Email,
                u.FullName,
                u.Role.ToString().ToLower(),
                u.IsActive,
                u.CreatedAt,
                p?.SunSign,
                p?.BirthPlace
            );
        }).ToList();
    }

    public async Task UpdateUserRoleAsync(Guid userId, string role, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null) throw new NotFoundException("İstifadəçi tapılmadı.");

        if (!Enum.TryParse<AppRole>(role, true, out var newRole))
            throw new BadRequestException("Rol yanlışdır.");

        user.Role = newRole;
        user.UpdatedAt = DateTime.UtcNow;
        _userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ToggleUserActiveStatusAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null) throw new NotFoundException("İstifadəçi tapılmadı.");

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        _userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
