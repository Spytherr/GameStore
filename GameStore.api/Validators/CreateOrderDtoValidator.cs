using FluentValidation;

namespace GameStore.api;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.")
            .Must(items => items.Count <= 10).WithMessage("Order cannot contain more than 10 items.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.GameOfferId)
                .GreaterThan(0).WithMessage("GameOfferId must be greater than 0.");

            item.RuleFor(x => x.Quantity)
                .InclusiveBetween(1, 100).WithMessage("Quantity must be between 1 and 100.");
        });
    }
}
