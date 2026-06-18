using Axiom.Application.Commands;
using FluentValidation;

namespace Axiom.Application.Validators;

/// <summary>
/// Validates the <see cref="CreateCaseCommand"/> before it is handled by the application.
/// Ensures required fields are provided and within length limits.
/// </summary>
public class CreateCaseValidator : AbstractValidator<CreateCaseCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCaseValidator"/> class with the validation rules.
    /// </summary>
    public CreateCaseValidator()
    {
        RuleFor(x => x.System).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Problem).NotEmpty();
    }
}
