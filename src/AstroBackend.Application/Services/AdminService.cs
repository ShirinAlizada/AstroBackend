using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Security;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Enums;
using AstroBackend.Domain.Exceptions;

namespace AstroBackend.Application.Services;

public class AdminService : IAdminService
{
    private readonly IGenericRepository<User> _userRepo;
    private readonly IGenericRepository<Profile> _profileRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public AdminService(
        IGenericRepository<User> userRepo,
        IGenericRepository<Profile> profileRepo,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _profileRepo = profileRepo;
        _passwordHasher = passwordHasher;
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

    public async Task UpdateUserRoleAsync(Guid targetUserId, string role, Guid currentUserId, bool isSuperAdmin, CancellationToken ct = default)
    {
        if (!isSuperAdmin)
            throw new ForbiddenException("Rol dəyişdirmək icazəsi yalnız Super Adminə məxsusdur.");

        var user = await _userRepo.GetByIdAsync(targetUserId, ct);
        if (user == null) throw new NotFoundException("İstifadəçi tapılmadı.");

        if (!Enum.TryParse<AppRole>(role.Replace("_", ""), true, out var newRole))
            throw new BadRequestException("Rol yanlışdır.");

        if (user.Role == AppRole.SuperAdmin && targetUserId == currentUserId)
            throw new BadRequestException("Öz Super Admin rolunuzu dəyişə bilməzsiniz.");

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

    public async Task<AdminUserDto> CreateUserAsync(RegisterRequest request, string role, Guid currentUserId, bool isSuperAdmin, CancellationToken ct = default)
    {
        if (!isSuperAdmin)
            throw new ForbiddenException("İstifadəçi yaratmaq səlahiyyəti yalnız Super Adminə məxsusdur.");

        var existing = await _userRepo.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim(), ct);
        if (existing != null)
            throw new BadRequestException("Bu e-poçt ilə istifadəçi artıq mövcuddur.");

        if (!Enum.TryParse<AppRole>(role.Replace("_", ""), true, out var appRole))
            appRole = AppRole.User;

        var user = new User
        {
            Email = request.Email.ToLower().Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FullName = request.FullName.Trim(),
            Role = appRole,
            IsActive = true
        };

        await _userRepo.AddAsync(user, ct);

        var profile = new Profile
        {
            UserId = user.Id,
            FullName = user.FullName
        };
        await _profileRepo.AddAsync(profile, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return new AdminUserDto(user.Id, user.Email, user.FullName, user.Role.ToString().ToLower(), user.IsActive, user.CreatedAt, null, null);
    }

    public async Task DeleteUserAsync(Guid targetUserId, Guid currentUserId, bool isSuperAdmin, CancellationToken ct = default)
    {
        if (!isSuperAdmin)
            throw new ForbiddenException("İstifadəçi silmək səlahiyyəti yalnız Super Adminə məxsusdur.");

        if (targetUserId == currentUserId)
            throw new BadRequestException("Öz hesabınızı silə bilməzsiniz.");

        var user = await _userRepo.GetByIdAsync(targetUserId, ct);
        if (user == null) throw new NotFoundException("İstifadəçi tapılmadı.");

        if (user.Role == AppRole.SuperAdmin)
            throw new BadRequestException("Başqa bir Super Admin hesabını silə bilməzsiniz.");

        _userRepo.Delete(user);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}

