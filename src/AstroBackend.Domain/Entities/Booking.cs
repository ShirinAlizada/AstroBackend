using AstroBackend.Domain.Common;
using AstroBackend.Domain.Enums;

namespace AstroBackend.Domain.Entities
{
    public class Booking : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid AstrologerId { get; set; }
        public SessionType SessionType { get; set; } = SessionType.Live;
        public DateTime ScheduledAt { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public string? Note { get; set; }

        // Navigation
        public virtual User User { get; set; } = null!;
        public virtual Astrologer Astrologer { get; set; } = null!;
    }

}
