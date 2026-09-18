using AstroBackend.Domain.Common;


namespace AstroBackend.Domain.Entities
{
    public class NatalChart : BaseEntity
    {
        public Guid UserId { get; set; }
        public string ChartJson { get; set; } = "{}";

        // Navigation
        public virtual User User { get; set; } = null!;
    }
}
