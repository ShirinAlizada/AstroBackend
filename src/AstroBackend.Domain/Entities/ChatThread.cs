using AstroBackend.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroBackend.Domain.Entities
{
    public class ChatThread : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = "Yeni söhbət";

        // Navigation
        public virtual User User { get; set; } = null!;
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }

}
