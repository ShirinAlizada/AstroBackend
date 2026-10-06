using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers;

public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(IAuthService authService, ICurrentUserService currentUserService)
    {
        _authService = authService;
        _currentUserService = currentUserService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var response = await _authService.RegisterAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var response = await _authService.LoginAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var response = await _authService.RefreshTokenAsync(request, ct);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<object> GetCurrentUser()
    {
        return Ok(new
        {
            userId = _currentUserService.UserId,
            email = _currentUserService.Email,
            role = _currentUserService.Role
        });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        await _authService.ChangePasswordAsync(_currentUserService.UserId.Value, request, ct);
        return Ok(new { message = "Şifrə uğurla dəyişdirildi." });
    }

    /// <summary>Cari refresh tokeni serverdə etibarsız edir — logout-dan sonra həmin token yeni access-token almaq üçün istifadə edilə bilməz.</summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        await _authService.LogoutAsync(_currentUserService.UserId.Value, ct);
        return Ok(new { message = "Çıxış edildi." });
    }

    /// <summary>E-poçt qeydiyyatda mövcuddursa, sıfırlama linki göndərir. Mövcud olmasa belə eyni cavabı qaytarır (e-poçtun qeydiyyatda olub-olmadığını açıqlamır).</summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        await _authService.ForgotPasswordAsync(request, ct);
        return Ok(new { message = "Əgər bu e-poçt qeydiyyatdadırsa, sıfırlama linki göndərildi." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        await _authService.ResetPasswordAsync(request, ct);
        return Ok(new { message = "Şifrə uğurla sıfırlandı. Yeni şifrənizlə daxil ola bilərsiniz." });
    }
}
