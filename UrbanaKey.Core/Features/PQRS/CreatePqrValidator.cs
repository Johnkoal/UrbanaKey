using FluentValidation;

namespace UrbanaKey.Core.Features.PQRS;

public class CreatePqrValidator : AbstractValidator<CreatePqrRequest>
{
    public CreatePqrValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.UnitId).NotEmpty();
    }
}
