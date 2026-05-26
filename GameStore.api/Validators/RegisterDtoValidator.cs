using FluentValidation;

namespace GameStore.api;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    private static readonly string[] ValidRoles = ["Buyer", "Seller"];

    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .Length(2, 50).WithMessage("Display name must be between 2 and 50 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => ValidRoles.Contains(role))
            .WithMessage("Role must be either 'Buyer' or 'Seller'.");
    }
}
