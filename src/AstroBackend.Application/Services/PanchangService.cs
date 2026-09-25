using AstroBackend.Application.Astrology;
using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;

namespace AstroBackend.Application.Services
{
    public class PanchangService : IPanchangService
    {
        public PanchangResponse GetPanchang(PanchangRequest request)
        {
            var dt = request.Date ?? DateTime.UtcNow;
            return PanchangEngine.Compute(dt);
        }
    }
}
