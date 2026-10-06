using AstroBackend.Application.DTOs;
using FluentValidation;

namespace AstroBackend.Application.Validators
{
    public class SynastryRequestValidator : AbstractValidator<SynastryRequest>
    {
        public SynastryRequestValidator()
        {
            RuleFor(x => x.SignA).NotEmpty();
            RuleFor(x => x.SignB).NotEmpty();
        }
    }

    /// <summary>
    /// CalculateChartRequest NatalChartsController-dəki "calculate" endpoint-ində də istifadə
    /// olunur — bu validator DI-da qeydiyyatdan keçdikdən sonra ValidationActionFilter bu tipi
    /// gördüyü HƏR yerdə (SynastryChartRequest-in daxilində də) avtomatik tətbiq edir.
    /// </summary>
    public class CalculateChartRequestValidator : AbstractValidator<CalculateChartRequest>
    {
        public CalculateChartRequestValidator()
        {
            RuleFor(x => x.Date).NotEmpty().WithMessage("Doğum tarixi mütləq daxil edilməlidir.");
            RuleFor(x => x.Time).NotEmpty().WithMessage("Doğum saatı mütləq daxil edilməlidir.");
            RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
            RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        }
    }

    public class SynastryChartRequestValidator : AbstractValidator<SynastryChartRequest>
    {
        public SynastryChartRequestValidator()
        {
            RuleFor(x => x.PersonB).NotNull().WithMessage("İkinci şəxsin doğum məlumatları mütləq daxil edilməlidir.");
            RuleFor(x => x.PersonB!).SetValidator(new CalculateChartRequestValidator()).When(x => x.PersonB != null);
            RuleFor(x => x.PersonA!).SetValidator(new CalculateChartRequestValidator()).When(x => x.PersonA != null);
        }
    }
}
