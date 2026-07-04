using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Queries;

/// <summary>
///     Consulta para recuperar un conocimiento específico a partir de su identificador único.
///     Si no existe ningún conocimiento con el <see cref="Id"/> proporcionado, devuelve <c>null</c>.
/// </summary>
/// <param name="Id">
///     Identificador único (GUID) del conocimiento que se desea obtener.
/// </param>
/// <returns>
///     El objeto <see cref="Knowledge"/> correspondiente al identificador solicitado,
///     o <c>null</c> si no se encuentra ningún conocimiento con ese Id.
/// </returns>
public record GetKnowledgeByIdQuery(Guid Id) : IRequest<Knowledge?>;
