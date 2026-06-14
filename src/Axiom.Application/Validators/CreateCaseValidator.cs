using Axiom.Application.Commands;
using FluentValidation;

namespace Axiom.Application.Validators;

public class CreateCaseValidator : AbstractValidator<CreateCaseCommand>
{
    public CreateCaseValidator()
    {
        RuleFor(x => x.System).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Problem).NotEmpty();
    }
}
