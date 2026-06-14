using Axiom.Application.Commands;
using FluentValidation;

namespace Axiom.Application.Validators;

public class CreateKnowledgeValidator : AbstractValidator<CreateKnowledgeCommand>
{
    public CreateKnowledgeValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.System).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Author).NotEmpty().MaximumLength(200);
    }
}
