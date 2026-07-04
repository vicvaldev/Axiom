using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Queries;

/// <summary>
///     Consulta para recuperar una incidencia específica a partir de su identificador único.
///     Si no existe ninguna incidencia con el <see cref="Id"/> proporcionado, devuelve <c>null</c>.
/// </summary>
/// <param name="Id">
///     Identificador único (GUID) de la incidencia que se desea obtener.
/// </param>
/// <returns>
///     El objeto <see cref="Issue"/> correspondiente al identificador solicitado,
///     o <c>null</c> si no se encuentra ninguna incidencia con ese Id.
/// </returns>
public record GetIssueByIdQuery(Guid Id) : IRequest<Issue?>;
