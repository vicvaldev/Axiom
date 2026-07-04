using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
///     Comando CQRS para crear un nuevo estado de conocimiento.
///     Los estados definen el ciclo de vida de un artículo de conocimiento (ej: borrador, publicado, archivado).
/// </summary>
/// <param name="Code">Código único del estado de conocimiento. Ejemplo: "DRAFT", "PUBLISHED", "ARCHIVED".</param>
/// <param name="Name">Nombre descriptivo del estado de conocimiento (máximo 200 caracteres).</param>
/// <returns>La entidad <see cref="KnowledgeState"/> creada, incluyendo su identificador numérico asignado.</returns>
public record CreateKnowledgeStateCommand(
    string Code,
    string Name) : IRequest<KnowledgeState>;
