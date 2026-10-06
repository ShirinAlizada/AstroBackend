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
    private readonly IEmailService _emailService;
    private readonly string _frontendBaseUrl;

    public AuthService(
        IGenericRepository<User> userRepo,
        IGenericRepository<Profile> profileRepo,
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        string frontendBaseUrl)
    {
        _userRepo = userRepo;
        _profileRepo = profileRepo;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _frontendBaseUrl = frontendBaseUrl;
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

        if (!user.IsActive)
            throw new ForbiddenException("İstifadəçi hesabı deaktiv edilmişdir.");

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

    public async Task LogoutAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null)
            return; // İstifadəçi artıq yoxdursa, sakitcə qayıt — logout idempotent olmalıdır.

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userRepo.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim(), ct);
        if (user == null)
            return; // E-poçtun qeydiyyatda olub-olmadığını açıqlamamaq üçün sakitcə qayıdır (enumeration-un qarşısını alır).

        user.PasswordResetToken = _tokenService.GenerateRefreshToken();
        user.PasswordResetTokenExpiryTime = DateTime.UtcNow.AddHours(1);
        await _unitOfWork.SaveChangesAsync(ct);

        var resetLink = $"{_frontendBaseUrl}/sifre-sifirla?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(user.PasswordResetToken)}";

        // Best-effort — SMTP konfiqurasiya olunmayıbsa və ya göndərmə xətası olarsa, token artıq
        // DB-də saxlanılıb (istifadəçi dəstəyi ilə əl ilə də paylaşıla bilər), əməliyyat kəsilmir.
        try
        {
            await _emailService.SendAsync(
                user.Email,
                "Şifrənizi sıfırlayın — Virgo Astrology",
                $"<p>Salam {System.Net.WebUtility.HtmlEncode(user.FullName)},</p>" +
                $"<p>Şifrənizi sıfırlamaq üçün <a href=\"{resetLink}\">bu linkə</a> klikləyin. Link 1 saat etibarlıdır.</p>" +
                "<p>Əgər bu sorğunu siz etməmisinizsə, bu e-poçtu nəzərə almayın.</p>",
                ct);
        }
        catch
        {
            // Email göndərilməsə belə, token qeydə alınıb — bu kritik xəta deyil.
        }
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userRepo.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim(), ct);
        if (user == null
            || string.IsNullOrEmpty(user.PasswordResetToken)
            || user.PasswordResetToken != request.Token
            || user.PasswordResetTokenExpiryTime == null
            || user.PasswordResetTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new BadRequestException("Sıfırlama linki etibarsızdır və ya vaxtı bitmişdir.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiryTime = null;
        // Şifrə sıfırlandıqdan sonra əvvəlki sessiyalar davam etməməlidir.
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
