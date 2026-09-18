using AstroBackend.Domain.Common;

namespace AstroBackend.Domain.Entities;

public class Astrologer : BaseEntity
{
    public Guid? UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public string SpecialtiesJson { get; set; } = "[]";
    public string LanguagesJson { get; set; } = "[]";
    public int PriceAzn { get; set; } = 50;
    public decimal Rating { get; set; } = 5.0m;
    public string? AvatarUrl { get; set; }
    public bool Verified { get; set; } = false;

    // Navigation
    public virtual User? User { get; set; }
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
