using AstroBackend.Domain.Common;

namespace AstroBackend.Domain.Entities
{
    public class Profile : BaseEntity
    {
        public Guid UserId { get; set; }
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? BirthDate { get; set; } // YYYY-MM-DD
        public string? BirthTime { get; set; } // HH:mm
        public string? BirthPlace { get; set; }
        public double? BirthLat { get; set; }
        public double? BirthLon { get; set; }
        public double? TzOffset { get; set; }
        public string? SunSign { get; set; }
        public string? MoonSign { get; set; }
        public string? Ascendant { get; set; }
        public string? Bio { get; set; }

        // Navigation
        public virtual User User { get; set; } = null!;
    }
}
