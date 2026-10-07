using AstroBackend.Application.Astrology;
using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Exceptions;

namespace AstroBackend.Application.Services
{
    public class NumerologyService : INumerologyService
    {
        public NumerologyResponse Calculate(NumerologyRequest request)
        {
            // NumerologyEngine.Compute null/boş FullName/BirthDate ilə çağırıldıqda
            // ArgumentNullException/NullReferenceException atır — client tərəfindən
            // göndərilən inputu burada doğrulayırıq ki, 500 əvəzinə aydın 400 qaytarılsın.
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new BadRequestException("Ad və soyad daxil edilməlidir.");
            if (string.IsNullOrWhiteSpace(request.BirthDate))
                throw new BadRequestException("Doğum tarixi daxil edilməlidir.");

            return NumerologyEngine.Compute(request.FullName, request.BirthDate, request.Lang, request.ForYear);
        }
    }

}
