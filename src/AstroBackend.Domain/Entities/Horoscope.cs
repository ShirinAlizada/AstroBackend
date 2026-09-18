using AstroBackend.Domain.Common;
using AstroBackend.Domain.Enums;


namespace AstroBackend.Domain.Entities
{
    public class Horoscope : BaseEntity
    {
        public string Sign { get; set; } = string.Empty; // Qoç, Buğa, etc.
        public HoroscopePeriod Period { get; set; } = HoroscopePeriod.Daily;
        public DateTime PeriodStart { get; set; } = DateTime.UtcNow.Date;
        public string Content { get; set; } = string.Empty;
        public int Love { get; set; } = 70;
        public int Career { get; set; } = 70;
        public int Finance { get; set; } = 70;
    }
}
