using AstroBackend.Domain.Common;

namespace AstroBackend.Domain.Entities
{
    public class ForumReply : BaseEntity
    {
        public Guid TopicId { get; set; }
        public Guid UserId { get; set; }
        public string AuthorName { get; set; } = "İstifadəçi";
        public string Body { get; set; } = string.Empty;
        public bool IsHidden { get; set; } = false;

        // Navigation
        public virtual ForumTopic Topic { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
