using FluentValidation;

namespace GameStore.api;

public class GamesQueryDtoValidator : AbstractValidator<GamesQueryDto>
{
    private static readonly string[] ValidSortFields = ["title", "releasedate", "price"];
    private static readonly string[] ValidSortOrders = ["asc", "desc"];

    public GamesQueryDtoValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("PageSize must be between 1 and 50.");

        RuleFor(x => x.SortBy)
            .Must(s => ValidSortFields.Contains(s.ToLower()))
            .WithMessage("SortBy must be one of: title, releasedate, price.");

        RuleFor(x => x.SortOrder)
            .Must(s => ValidSortOrders.Contains(s.ToLower()))
            .WithMessage("SortOrder must be 'asc' or 'desc'.");
    }
}
