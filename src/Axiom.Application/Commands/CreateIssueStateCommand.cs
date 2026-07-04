using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
///     Comando CQRS para crear un nuevo estado de issue.
///     Los estados definen el ciclo de vida de un issue (ej: abierto, en análisis, resuelto, cerrado).
/// </summary>
/// <param name="Code">Código único del estado de issue. Ejemplo: "OPEN", "ANALYSIS", "RESOLVED", "CLOSED".</param>
/// <param name="Name">Nombre descriptivo del estado de issue (máximo 200 caracteres).</param>
/// <returns>La entidad <see cref="IssueState"/> creada, incluyendo su identificador numérico asignado.</returns>
public record CreateIssueStateCommand(
    string Code,
    string Name) : IRequest<IssueState>;
