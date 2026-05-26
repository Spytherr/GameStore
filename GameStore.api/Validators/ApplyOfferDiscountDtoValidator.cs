using FluentValidation;

namespace GameStore.api;

public class ApplyOfferDiscountDtoValidator : AbstractValidator<ApplyOfferDiscountDto>
{
    public ApplyOfferDiscountDtoValidator()
    {
        RuleFor(x => x.DiscountPercentage)
            .GreaterThanOrEqualTo(1).WithMessage("Discount must be at least 1%.")
            .LessThanOrEqualTo(90).WithMessage("Discount cannot exceed 90%.");
    }
}
