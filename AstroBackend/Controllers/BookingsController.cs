using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers;

[Authorize]
public class BookingsController : BaseApiController
{
    private readonly IBookingService _bookingService;
    private readonly ICurrentUserService _currentUserService;

    public BookingsController(IBookingService bookingService, ICurrentUserService currentUserService)
    {
        _bookingService = bookingService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<ActionResult<BookingDto>> CreateBooking([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        var booking = await _bookingService.CreateBookingAsync(_currentUserService.UserId.Value, request, ct);
        return Ok(booking);
    }

    [HttpGet("my-bookings")]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> GetMyBookings(CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        var list = await _bookingService.GetMyBookingsAsync(_currentUserService.UserId.Value, ct);
        return Ok(list);
    }

    [HttpGet("astrologer-bookings")]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> GetAstrologerBookings(CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        var list = await _bookingService.GetAstrologerBookingsAsync(_currentUserService.UserId.Value, ct);
        return Ok(list);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<BookingDto>> UpdateStatus(Guid id, [FromBody] UpdateBookingStatusRequest request, CancellationToken ct)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedException("Giriş edilməyib.");

        bool isAdmin = _currentUserService.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;
        var updated = await _bookingService.UpdateStatusAsync(id, _currentUserService.UserId.Value, isAdmin, request.Status, ct);
        return Ok(updated);
    }
}
