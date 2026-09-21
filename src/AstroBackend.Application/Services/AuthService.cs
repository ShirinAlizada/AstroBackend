using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Security;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Enums;
using AstroBackend.Domain.Exceptions;

namespace AstroBackend.Application.Services;

public class AuthService : IAuthService
{
    private readonly IGenericRepository<User> _userRepo;
    private readonly IGenericRepository<Profile> _profileRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        IGenericRepository<User> userRepo,
        IGenericRepository<Profile> profileRepo,
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IPasswordHasher passwordHasher)
    {
        _userRepo = userRepo;
        _profileRepo = profileRepo;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new BadRequestException("E-poçt və şifrə mütləq daxil edilməlidir.");

        var existing = await _userRepo.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim(), ct);
        if (existing != null)
            throw new BadRequestException("Bu e-poçt ünvanı ilə artıq qeydiyyatdan keçilib.");

        var user = new User
        {
            Email = request.Email.ToLower().Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FullName = request.FullName.Trim(),
            Role = AppRole.User,
            IsActive = true
        };

        await _userRepo.AddAsync(user, ct);

        var profile = new Profile
        {
            UserId = user.Id,
            FullName = user.FullName
        };
        await _profileRepo.AddAsync(profile, ct);

        user.RefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _unitOfWork.SaveChangesAsync(ct);

        var token = _tokenService.GenerateAccessToken(user);
        return new AuthResponse(user.Id, user.Email, user.FullName, user.Role.ToString().ToLower(), token, user.RefreshToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userRepo.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim(), ct);
        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedException("E-poçt və ya şifrə yanlışdır.");

        if (!user.IsActive)
            throw new ForbiddenException("İstifadəçi hesabı deaktiv edilmişdir.");

        user.RefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _unitOfWork.SaveChangesAsync(ct);

        var token = _tokenService.GenerateAccessToken(user);
        return new AuthResponse(user.Id, user.Email, user.FullName, user.Role.ToString().ToLower(), token, user.RefreshToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var user = await _userRepo.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, ct);
        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new UnauthorizedException("Yeniləmə tokeni etibarsızdır və ya vaxtı bitmişdir.");

        user.RefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _unitOfWork.SaveChangesAsync(ct);

        var token = _tokenService.GenerateAccessToken(user);
        return new AuthResponse(user.Id, user.Email, user.FullName, user.Role.ToString().ToLower(), token, user.RefreshToken);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null)
            throw new NotFoundException("İstifadəçi tapılmadı.");

        if (!_passwordHasher.VerifyPassword(request.OldPassword, user.PasswordHash))
            throw new BadRequestException("Cari şifrə yanlışdır.");

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
