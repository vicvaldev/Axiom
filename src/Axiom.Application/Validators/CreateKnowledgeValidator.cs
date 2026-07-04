using Axiom.Application.Commands;
using FluentValidation;

namespace Axiom.Application.Validators;

/// <summary>
/// Validador para el comando <see cref="CreateKnowledgeCommand"/>.
/// Garantiza que los datos obligatorios para crear un nuevo conocimiento
/// cumplan con las reglas de negocio antes de ser procesados por el manejador.
/// </summary>
public class CreateKnowledgeValidator : AbstractValidator<CreateKnowledgeCommand>
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CreateKnowledgeValidator"/>
    /// y define las reglas de validación para cada propiedad del comando.
    /// </summary>
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
