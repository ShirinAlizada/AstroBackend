using AstroBackend.Domain.Common;

namespace AstroBackend.Domain.Entities
{
    public class Article : BaseEntity
    {
        public Guid? AuthorId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string Body { get; set; } = string.Empty;
        public string Tag { get; set; } = "ümumi";
        public string? CoverUrl { get; set; }
        public bool Published { get; set; } = true;
        public DateTime? PublishedAt { get; set; } = DateTime.UtcNow;
        public int Views { get; set; } = 0;

        // Navigation
        public virtual User? Author { get; set; }
    }
}
