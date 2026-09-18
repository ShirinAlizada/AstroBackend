using AstroBackend.Domain.Common;

namespace AstroBackend.Domain.Entities
{
    public class JournalEntry : BaseEntity
    {
        public Guid UserId { get; set; }
        public DateTime EntryDate { get; set; } = DateTime.UtcNow.Date;
        public int Mood { get; set; } = 3; // 1-5
        public string? Title { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? TransitNote { get; set; }

        // Navigation
        public virtual User User { get; set; } = null!;
    }
}
