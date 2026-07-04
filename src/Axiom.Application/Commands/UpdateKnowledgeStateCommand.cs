using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
///     Comando para actualizar los datos de un estado de conocimiento existente en el catálogo de estados.
///     Implementa <see cref="IRequest{TResponse}" /> con <see cref="KnowledgeState" /> como respuesta,
///     devolviendo la entidad actualizada o <c>null</c> si el estado no se encuentra.
/// </summary>
/// <param name="Id">Identificador único del estado de conocimiento que se desea actualizar.</param>
/// <param name="Code">Nuevo código único que identifica al estado de conocimiento.</param>
/// <param name="Name">Nuevo nombre descriptivo del estado de conocimiento.</param>
/// <returns>La entidad <see cref="KnowledgeState" /> actualizada, o <c>null</c> si no existe ningún estado de conocimiento con el <paramref name="Id" /> especificado.</returns>
public record UpdateKnowledgeStateCommand(
    int Id,
    string Code,
    string Name) : IRequest<KnowledgeState?>;
