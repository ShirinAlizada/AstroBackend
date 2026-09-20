using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Enums;
using AstroBackend.Domain.Exceptions;

namespace AstroBackend.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<Astrologer> _astrologerRepo;
        private readonly IUnitOfWork _unitOfWork;

        public BookingService(
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<Astrologer> astrologerRepo,
            IUnitOfWork unitOfWork)
        {
            _bookingRepo = bookingRepo;
            _astrologerRepo = astrologerRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<BookingDto> CreateBookingAsync(Guid userId, CreateBookingRequest request, CancellationToken ct = default)
        {
            var astrologer = await _astrologerRepo.GetByIdAsync(request.AstrologerId, ct);
            if (astrologer == null)
                throw new NotFoundException("Astroloq tapılmadı.");

            if (request.ScheduledAt < DateTime.UtcNow)
                throw new BadRequestException("Görüş üçün gələcək bir tarix və saat seçin.");

            var sessionType = request.SessionType?.ToLower() == "written" ? SessionType.Written : SessionType.Live;

            var booking = new Booking
            {
                UserId = userId,
                AstrologerId = request.AstrologerId,
                SessionType = sessionType,
                ScheduledAt = request.ScheduledAt,
                Status = BookingStatus.Pending,
                Note = request.Note
            };

            await _bookingRepo.AddAsync(booking, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return new BookingDto(
                booking.Id,
                booking.UserId,
                booking.AstrologerId,
                astrologer.DisplayName,
                astrologer.Title,
                booking.SessionType.ToString().ToLower(),
                booking.ScheduledAt,
                booking.Status.ToString().ToLower(),
                booking.Note,
                booking.CreatedAt
            );
        }

        public async Task<IReadOnlyList<BookingDto>> GetMyBookingsAsync(Guid userId, CancellationToken ct = default)
        {
            var bookings = _bookingRepo.Query()
                .Where(b => b.UserId == userId)
                .OrderBy(b => b.ScheduledAt)
                .ToList();

            var astrologerIds = bookings.Select(b => b.AstrologerId).Distinct().ToList();
            var astrologers = (await _astrologerRepo.FindAsync(a => astrologerIds.Contains(a.Id), ct))
                .ToDictionary(a => a.Id, a => a);

            return bookings.Select(b =>
            {
                astrologers.TryGetValue(b.AstrologerId, out var astro);
                return new BookingDto(
                    b.Id,
                    b.UserId,
                    b.AstrologerId,
                    astro?.DisplayName ?? "Astroloq",
                    astro?.Title,
                    b.SessionType.ToString().ToLower(),
                    b.ScheduledAt,
                    b.Status.ToString().ToLower(),
                    b.Note,
                    b.CreatedAt
                );
            }).ToList();
        }

        public async Task<IReadOnlyList<BookingDto>> GetAstrologerBookingsAsync(Guid astrologerUserId, CancellationToken ct = default)
        {
            var astro = await _astrologerRepo.FirstOrDefaultAsync(a => a.UserId == astrologerUserId, ct);
            if (astro == null)
                return [];

            var bookings = _bookingRepo.Query()
                .Where(b => b.AstrologerId == astro.Id)
                .OrderBy(b => b.ScheduledAt)
                .ToList();

            return bookings.Select(b => new BookingDto(
                b.Id,
                b.UserId,
                b.AstrologerId,
                astro.DisplayName,
                astro.Title,
                b.SessionType.ToString().ToLower(),
                b.ScheduledAt,
                b.Status.ToString().ToLower(),
                b.Note,
                b.CreatedAt
            )).ToList();
        }

        public async Task<IReadOnlyList<BookingDto>> GetAllBookingsAsync(CancellationToken ct = default)
        {
            var bookings = _bookingRepo.Query().OrderByDescending(b => b.CreatedAt).ToList();
            var astrologerIds = bookings.Select(b => b.AstrologerId).Distinct().ToList();
            var astrologers = (await _astrologerRepo.FindAsync(a => astrologerIds.Contains(a.Id), ct))
                .ToDictionary(a => a.Id, a => a);

            return bookings.Select(b =>
            {
                astrologers.TryGetValue(b.AstrologerId, out var astro);
                return new BookingDto(
                    b.Id,
                    b.UserId,
                    b.AstrologerId,
                    astro?.DisplayName ?? "Astroloq",
                    astro?.Title,
                    b.SessionType.ToString().ToLower(),
                    b.ScheduledAt,
                    b.Status.ToString().ToLower(),
                    b.Note,
                    b.CreatedAt
                );
            }).ToList();
        }

        public async Task<BookingDto> UpdateStatusAsync(Guid bookingId, Guid currentUserId, bool isAdmin, string status, CancellationToken ct = default)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId, ct);
            if (booking == null)
                throw new NotFoundException("Rezervasiya tapılmadı.");

            if (!Enum.TryParse<BookingStatus>(status, true, out var newStatus))
                throw new BadRequestException("Status yalnışdır.");

            // Users can cancel their own booking
            if (!isAdmin && booking.UserId != currentUserId)
            {
                var astro = await _astrologerRepo.GetByIdAsync(booking.AstrologerId, ct);
                if (astro == null || astro.UserId != currentUserId)
                    throw new ForbiddenException("Bu rezervasiyanı dəyişməyə icazəniz yoxdur.");
            }

            booking.Status = newStatus;
            booking.UpdatedAt = DateTime.UtcNow;
            _bookingRepo.Update(booking);
            await _unitOfWork.SaveChangesAsync(ct);

            var astrologer = await _astrologerRepo.GetByIdAsync(booking.AstrologerId, ct);
            return new BookingDto(
                booking.Id,
                booking.UserId,
                booking.AstrologerId,
                astrologer?.DisplayName ?? "Astroloq",
                astrologer?.Title,
                booking.SessionType.ToString().ToLower(),
                booking.ScheduledAt,
                booking.Status.ToString().ToLower(),
                booking.Note,
                booking.CreatedAt
            );
        }
    }

}
