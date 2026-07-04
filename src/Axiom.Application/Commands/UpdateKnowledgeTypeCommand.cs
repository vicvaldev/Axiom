using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
///     Comando para actualizar los datos de un tipo de conocimiento existente en el catálogo de tipos.
///     Implementa <see cref="IRequest{TResponse}" /> con <see cref="KnowledgeType" /> como respuesta,
///     devolviendo la entidad actualizada o <c>null</c> si el tipo no se encuentra.
/// </summary>
/// <param name="Id">Identificador único del tipo de conocimiento que se desea actualizar.</param>
/// <param name="Code">Nuevo código único que identifica al tipo de conocimiento.</param>
/// <param name="Name">Nuevo nombre descriptivo del tipo de conocimiento.</param>
/// <returns>La entidad <see cref="KnowledgeType" /> actualizada, o <c>null</c> si no existe ningún tipo de conocimiento con el <paramref name="Id" /> especificado.</returns>
public record UpdateKnowledgeTypeCommand(
    long Id,
    string Code,
    string Name) : IRequest<KnowledgeType?>;
