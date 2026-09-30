using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers;

[Authorize]
public class ProfilesController : BaseApiController
{
    private static readonly string[] AllowedAvatarExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long MaxAvatarBytes = 3 * 1024 * 1024; // 3MB

    private readonly IProfileService _profileService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;

    public ProfilesController(IProfileService profileService, IFileStorageService fileStorageService, ICurrentUserService currentUserService)
    {
        _profileService = profileService;
        _fileStorageService = fileStorageService;
        _currentUserService = currentUserService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ProfileDto>> GetMyProfile(CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        var profile = await _profileService.GetProfileAsync(_currentUserService.UserId.Value, ct);
        return Ok(profile);
    }

    [HttpPut("me")]
    public async Task<ActionResult<ProfileDto>> UpdateMyProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        var updated = await _profileService.UpdateProfileAsync(_currentUserService.UserId.Value, request, ct);
        return Ok(updated);
    }

    [HttpPost("me/avatar")]
    public async Task<ActionResult<ProfileDto>> UploadMyAvatar(IFormFile file, CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        if (file == null || file.Length == 0)
            throw new BadRequestException("Fayl seçilməyib.");

        if (file.Length > MaxAvatarBytes)
            throw new BadRequestException("Fayl ölçüsü 3MB-dan böyük ola bilməz.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedAvatarExtensions.Contains(extension))
            throw new BadRequestException("Yalnız şəkil faylları (jpg, jpeg, png, webp, gif) qəbul olunur.");

        var userId = _currentUserService.UserId.Value;

        await using var stream = file.OpenReadStream();
        var avatarUrl = await _fileStorageService.SaveAvatarAsync(userId, stream, extension, ct);

        var updated = await _profileService.UpdateProfileAsync(
            userId,
            new UpdateProfileRequest(null, avatarUrl, null, null, null, null, null, null),
            ct);

        return Ok(updated);
    }
}
