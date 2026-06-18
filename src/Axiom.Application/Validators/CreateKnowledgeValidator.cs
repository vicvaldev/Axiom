using Axiom.Application.Commands;
using FluentValidation;

namespace Axiom.Application.Validators;

/// <summary>
/// Validates the <see cref="CreateKnowledgeCommand"/> before it is handled by the application.
/// Ensures required fields are provided and within length limits.
/// </summary>
public class CreateKnowledgeValidator : AbstractValidator<CreateKnowledgeCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateKnowledgeValidator"/> class with the validation rules.
    /// </summary>
    public CreateKnowledgeValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.System).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Author).NotEmpty().MaximumLength(200);
    }
}
