using Axiom.Application.Commands;
using FluentValidation;

namespace Axiom.Application.Validators;

public class CreateDependencyTraceEventValidator : AbstractValidator<CreateDependencyTraceEventCommand>
{
    public CreateDependencyTraceEventValidator()
    {
        RuleFor(x => x.DependencyId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
    }
}
