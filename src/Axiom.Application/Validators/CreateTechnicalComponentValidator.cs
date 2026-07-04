using Axiom.Application.Commands;
using FluentValidation;

namespace Axiom.Application.Validators;

public class CreateTechnicalComponentValidator : AbstractValidator<CreateTechnicalComponentCommand>
{
    public CreateTechnicalComponentValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TechnicalName).NotEmpty().MaximumLength(500);
        RuleFor(x => x.SystemId).GreaterThan(0);
    }
}
