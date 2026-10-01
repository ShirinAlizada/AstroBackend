using AstroBackend.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroBackend.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Body { get; set; }
        public string? Link { get; set; }
        public bool IsRead { get; set; } = false;

        // Navigation
        public virtual User User { get; set; } = null!;
    }
}
