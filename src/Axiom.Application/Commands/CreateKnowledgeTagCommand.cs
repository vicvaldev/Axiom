using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
///     Comando CQRS para crear una nueva etiqueta (tag) de conocimiento.
/// </summary>
/// <param name="TagName">Nombre de la etiqueta. Debe ser único en el sistema (máximo 100 caracteres).</param>
/// <returns>La entidad <see cref="KnowledgeTag"/> creada, incluyendo su identificador numérico asignado.</returns>
public record CreateKnowledgeTagCommand(string TagName) : IRequest<KnowledgeTag>;
