using Axiom.Application.Commands;
using FluentValidation;

namespace Axiom.Application.Validators;

public class CreateComponentDependencyValidator : AbstractValidator<CreateComponentDependencyCommand>
{
    public CreateComponentDependencyValidator()
    {
        RuleFor(x => x.SourceComponentId).NotEmpty();
        RuleFor(x => x.TargetComponentId).NotEmpty();
        RuleFor(x => x.SourceComponentId).NotEqual(x => x.TargetComponentId)
            .WithMessage("Source and target component cannot be the same.");
    }
}
