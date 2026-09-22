using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers;

[Authorize]
public class ProfilesController : BaseApiController
{
    private readonly IProfileService _profileService;
    private readonly ICurrentUserService _currentUserService;

    public ProfilesController(IProfileService profileService, ICurrentUserService currentUserService)
    {
        _profileService = profileService;
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
}
