using Axiom.Application.Commands;
using FluentValidation;

namespace Axiom.Application.Validators;

/// <summary>
/// Validador para el comando <see cref="CreateIssueCommand"/>.
/// Asegura que los datos obligatorios para registrar una nueva incidencia
/// cumplan con las reglas de negocio antes de ser procesados por el manejador.
/// </summary>
public class CreateIssueValidator : AbstractValidator<CreateIssueCommand>
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CreateIssueValidator"/>
    /// y define las reglas de validación para cada propiedad del comando.
    /// </summary>
    public CreateIssueValidator()
    {
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Problem).NotEmpty();
        RuleFor(x => x.SystemId).GreaterThan(0);
        RuleFor(x => x.CreatedByUserId).NotEmpty();
        RuleFor(x => x.StateId).GreaterThan(0);
    }
}
