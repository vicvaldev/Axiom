using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
///     Comando para actualizar los datos de un estado de issue existente en el catálogo de estados.
///     Implementa <see cref="IRequest{TResponse}" /> con <see cref="IssueState" /> como respuesta,
///     devolviendo la entidad actualizada o <c>null</c> si el estado no se encuentra.
/// </summary>
/// <param name="Id">Identificador único del estado de issue que se desea actualizar.</param>
/// <param name="Code">Nuevo código único que identifica al estado de issue.</param>
/// <param name="Name">Nuevo nombre descriptivo del estado de issue.</param>
/// <returns>La entidad <see cref="IssueState" /> actualizada, o <c>null</c> si no existe ningún estado de issue con el <paramref name="Id" /> especificado.</returns>
public record UpdateIssueStateCommand(
    int Id,
    string Code,
    string Name) : IRequest<IssueState?>;
