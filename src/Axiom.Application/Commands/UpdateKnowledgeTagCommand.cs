using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
///     Comando para actualizar el nombre de una etiqueta de conocimiento existente.
///     Implementa <see cref="IRequest{TResponse}" /> con <see cref="KnowledgeTag" /> como respuesta,
///     devolviendo la entidad actualizada o <c>null</c> si la etiqueta no se encuentra.
/// </summary>
/// <param name="Id">Identificador único de la etiqueta de conocimiento que se desea actualizar.</param>
/// <param name="TagName">Nuevo nombre para la etiqueta de conocimiento (hasta 100 caracteres). Debe ser único en el sistema.</param>
/// <returns>La entidad <see cref="KnowledgeTag" /> actualizada, o <c>null</c> si no existe ninguna etiqueta con el <paramref name="Id" /> especificado.</returns>
public record UpdateKnowledgeTagCommand(
    long Id,
    string TagName) : IRequest<KnowledgeTag?>;
