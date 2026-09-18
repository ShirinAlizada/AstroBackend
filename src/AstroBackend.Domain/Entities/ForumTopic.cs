using AstroBackend.Domain.Common;


namespace AstroBackend.Domain.Entities
{
    public class ForumTopic : BaseEntity
    {
        public Guid UserId { get; set; }
        public string AuthorName { get; set; } = "İstifadəçi";
        public string Category { get; set; } = "ümumi";
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHidden { get; set; } = false;

        // Navigation
        public virtual User User { get; set; } = null!;
        public virtual ICollection<ForumReply> Replies { get; set; } = new List<ForumReply>();
    }
}
