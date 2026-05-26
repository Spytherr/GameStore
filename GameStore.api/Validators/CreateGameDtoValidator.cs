using FluentValidation;

namespace GameStore.api;

public class CreateGameDtoValidator : AbstractValidator<CreateGameDto>
{
    public CreateGameDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(50).WithMessage("Title cannot exceed 50 characters.");

        RuleFor(x => x.GenreId)
            .GreaterThan(0).WithMessage("GenreId must be greater than 0.");

        RuleFor(x => x.ReleaseDate)
            .NotEmpty().WithMessage("Release date is required.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).When(x => x.ImageUrl is not null);

        RuleFor(x => x.Description)
            .MaximumLength(1000).When(x => x.Description is not null);
    }
}
