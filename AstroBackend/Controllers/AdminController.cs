using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : BaseApiController
{
    private readonly IAdminService _adminService;
    private readonly IAstrologerService _astrologerService;
    private readonly IBookingService _bookingService;
    private readonly IHoroscopeService _horoscopeService;
    private readonly IForumService _forumService;

    public AdminController(
        IAdminService adminService,
        IAstrologerService astrologerService,
        IBookingService bookingService,
        IHoroscopeService horoscopeService,
        IForumService forumService)
    {
        _adminService = adminService;
        _astrologerService = astrologerService;
        _bookingService = bookingService;
        _horoscopeService = horoscopeService;
        _forumService = forumService;
    }

    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> GetAllUsers(CancellationToken ct)
    {
        var users = await _adminService.GetAllUsersAsync(ct);
        return Ok(users);
    }

    [HttpPatch("users/{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromQuery] string role, CancellationToken ct)
    {
        await _adminService.UpdateUserRoleAsync(id, role, ct);
        return Ok(new { message = "İstifadəçi rolu yeniləndi." });
    }

    [HttpPatch("users/{id:guid}/status")]
    public async Task<IActionResult> ToggleStatus(Guid id, CancellationToken ct)
    {
        await _adminService.ToggleUserActiveStatusAsync(id, ct);
        return Ok(new { message = "İstifadəçi statusu dəyişdirildi." });
    }

    [HttpGet("astrologers")]
    public async Task<ActionResult<IReadOnlyList<AstrologerDto>>> GetAllAstrologers(CancellationToken ct)
    {
        var list = await _astrologerService.GetAllAstrologersAsync(ct);
        return Ok(list);
    }

    [HttpPost("astrologers")]
    public async Task<ActionResult<AstrologerDto>> CreateAstrologer([FromBody] CreateAstrologerRequest request, CancellationToken ct)
    {
        var created = await _astrologerService.CreateAstrologerAsync(null, request, ct);
        return Ok(created);
    }

    [HttpPut("astrologers/{id:guid}")]
    public async Task<ActionResult<AstrologerDto>> UpdateAstrologer(Guid id, [FromBody] UpdateAstrologerRequest request, CancellationToken ct)
    {
        var updated = await _astrologerService.UpdateAstrologerAsync(id, request, ct);
        return Ok(updated);
    }

    [HttpPatch("astrologers/{id:guid}/verify")]
    public async Task<IActionResult> VerifyAstrologer(Guid id, [FromQuery] bool verified, CancellationToken ct)
    {
        await _astrologerService.VerifyAstrologerAsync(id, verified, ct);
        return Ok(new { message = $"Astroloq təsdiqlənməsi: {verified}." });
    }

    [HttpDelete("astrologers/{id:guid}")]
    public async Task<IActionResult> DeleteAstrologer(Guid id, CancellationToken ct)
    {
        await _astrologerService.DeleteAstrologerAsync(id, ct);
        return Ok(new { message = "Astroloq silindi." });
    }

    [HttpGet("bookings")]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> GetAllBookings(CancellationToken ct)
    {
        var list = await _bookingService.GetAllBookingsAsync(ct);
        return Ok(list);
    }

    [HttpPost("horoscopes")]
    public async Task<ActionResult<HoroscopeDto>> CreateHoroscope([FromBody] CreateHoroscopeRequest request, CancellationToken ct)
    {
        var created = await _horoscopeService.CreateHoroscopeAsync(request, ct);
        return Ok(created);
    }

    [HttpPut("horoscopes/{id:guid}")]
    public async Task<ActionResult<HoroscopeDto>> UpdateHoroscope(Guid id, [FromBody] UpdateHoroscopeRequest request, CancellationToken ct)
    {
        var updated = await _horoscopeService.UpdateHoroscopeAsync(id, request, ct);
        return Ok(updated);
    }

    [HttpDelete("horoscopes/{id:guid}")]
    public async Task<IActionResult> DeleteHoroscope(Guid id, CancellationToken ct)
    {
        await _horoscopeService.DeleteHoroscopeAsync(id, ct);
        return Ok(new { message = "Horoskop silindi." });
    }

    [HttpPatch("forum/topics/{id:guid}/hide")]
    public async Task<IActionResult> SetTopicHidden(Guid id, [FromQuery] bool isHidden, CancellationToken ct)
    {
        await _forumService.SetTopicHiddenAsync(id, isHidden, ct);
        return Ok(new { message = $"Mövzunun gizliliyi dəyişdirildi: {isHidden}." });
    }

    [HttpPatch("forum/replies/{id:guid}/hide")]
    public async Task<IActionResult> SetReplyHidden(Guid id, [FromQuery] bool isHidden, CancellationToken ct)
    {
        await _forumService.SetReplyHiddenAsync(id, isHidden, ct);
        return Ok(new { message = $"Rəyin gizliliyi dəyişdirildi: {isHidden}." });
    }
}
