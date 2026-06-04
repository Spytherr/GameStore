using FluentValidation;

namespace GameStore.api;

public class CreateGameDtoValidator : AbstractValidator<CreateGameDto>
{
    public CreateGameDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(50).WithMessage("Title cannot exceed 50 characters.");

        RuleFor(x => x.GenreIds)
            .NotEmpty().WithMessage("At least one GenreId must be provided.")
            .Must(ids => ids.Count <= 10).WithMessage("A game cannot have more than 10 genres.");

        RuleForEach(x => x.GenreIds)
            .GreaterThan(0).WithMessage("GenreId must be greater than 0.");

        RuleFor(x => x.PlatformIds)
            .NotEmpty().WithMessage("At least one PlatformId must be provided.")
            .Must(ids => ids.Count <= 20).WithMessage("A game cannot have more than 20 platforms.");

        RuleForEach(x => x.PlatformIds)
            .GreaterThan(0).WithMessage("PlatformId must be greater than 0.");

        RuleFor(x => x.ReleaseDate)
            .NotEmpty().WithMessage("Release date is required.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).When(x => x.ImageUrl is not null);

        RuleFor(x => x.Creators)
            .MaximumLength(500).When(x => x.Creators is not null);

        RuleFor(x => x.Publishers)
            .MaximumLength(500).When(x => x.Publishers is not null);
    }
}
