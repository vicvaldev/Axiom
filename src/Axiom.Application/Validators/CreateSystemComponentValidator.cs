using Axiom.Application.Commands;
using FluentValidation;

namespace Axiom.Application.Validators;

public class CreateSystemComponentValidator : AbstractValidator<CreateSystemComponentCommand>
{
    public CreateSystemComponentValidator()
    {
        RuleFor(x => x.SystemId).GreaterThan(0);
        RuleFor(x => x.ComponentId).NotEmpty();
    }
}
