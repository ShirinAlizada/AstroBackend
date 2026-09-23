using AstroBackend.Domain.Common;

namespace AstroBackend.Domain.Entities
{
    public class ChatMessage : BaseEntity
    {
        public Guid ThreadId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = "user"; // user, assistant, system
        public string Content { get; set; } = string.Empty;

        // Navigation
        public virtual ChatThread Thread { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
