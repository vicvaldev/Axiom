using Axiom.Application.Commands;
using FluentValidation;

namespace Axiom.Application.Validators;

public class CreateKnowledgeValidator : AbstractValidator<CreateKnowledgeCommand>
{
    public CreateKnowledgeValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.SystemId).GreaterThan(0);
        RuleFor(x => x.CreatedByUserId).NotEmpty();
        RuleFor(x => x.KnowledgeTypeId).GreaterThan(0);
        RuleFor(x => x.KnowledgeStateId).GreaterThan(0);
    }
}
