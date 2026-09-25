using AstroBackend.Application.Astrology;
using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;

namespace AstroBackend.Application.Services
{
    public class NumerologyService : INumerologyService
    {
        public NumerologyResponse Calculate(NumerologyRequest request)
        {
            return NumerologyEngine.Compute(request.FullName, request.BirthDate);
        }
    }

}
