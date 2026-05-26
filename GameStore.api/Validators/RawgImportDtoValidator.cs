using FluentValidation;

namespace GameStore.api;

public class RawgImportDtoValidator : AbstractValidator<RawgImportDto>
{
    public RawgImportDtoValidator()
    {
        RuleFor(x => x.RawgId)
            .GreaterThan(0).WithMessage("RawgId must be greater than 0.");
    }
}
