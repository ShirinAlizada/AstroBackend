using AstroBackend.Application.DTOs;
using FluentValidation;

namespace AstroBackend.Application.Validators
{
    public class CreateShopProductRequestValidator : AbstractValidator<CreateShopProductRequest>
    {
        public CreateShopProductRequestValidator()
        {
            RuleFor(x => x.Category).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
            RuleFor(x => x.PriceAzn).GreaterThan(0);
            RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        }
    }

    public class UpdateShopProductRequestValidator : AbstractValidator<UpdateShopProductRequest>
    {
        public UpdateShopProductRequestValidator()
        {
            RuleFor(x => x.Category).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
            RuleFor(x => x.PriceAzn).GreaterThan(0);
            RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        }
    }

    public class PlaceShopOrderRequestValidator : AbstractValidator<PlaceShopOrderRequest>
    {
        public PlaceShopOrderRequestValidator()
        {
            RuleFor(x => x.Items).NotEmpty().WithMessage("Səbət boşdur.");
            RuleForEach(x => x.Items).ChildRules(items =>
            {
                items.RuleFor(i => i.ProductId).NotEmpty();
                items.RuleFor(i => i.Quantity).GreaterThan(0);
            });
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Phone).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        }
    }

    public class UpsertReviewRequestValidator : AbstractValidator<UpsertReviewRequest>
    {
        public UpsertReviewRequestValidator()
        {
            RuleFor(x => x.Rating).InclusiveBetween(1, 5);
            RuleFor(x => x.Comment).MaximumLength(2000);
        }
    }
}
