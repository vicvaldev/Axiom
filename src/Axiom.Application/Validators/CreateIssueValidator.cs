using Axiom.Application.Commands;
using FluentValidation;

namespace Axiom.Application.Validators;

public class CreateIssueValidator : AbstractValidator<CreateIssueCommand>
{
    public CreateIssueValidator()
    {
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Problem).NotEmpty();
        RuleFor(x => x.SystemId).GreaterThan(0);
        RuleFor(x => x.CreatedByUserId).NotEmpty();
        RuleFor(x => x.StateId).GreaterThan(0);
    }
}
