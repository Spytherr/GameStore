using FluentValidation;

namespace GameStore.api;

public class CreateGameOfferDtoValidator : AbstractValidator<CreateGameOfferDto>
{
    public CreateGameOfferDtoValidator()
    {
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .LessThanOrEqualTo(9999.99m).WithMessage("Price cannot exceed 9999.99.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative.")
            .LessThanOrEqualTo(10000).WithMessage("Stock cannot exceed 10000.");

        RuleFor(x => x.PlatformId)
            .GreaterThan(0).WithMessage("PlatformId must be greater than 0.");
    }
}
