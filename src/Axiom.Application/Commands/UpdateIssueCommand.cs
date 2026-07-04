using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
///     Comando para actualizar los datos de un incidente o solicitud (issue) existente.
///     Implementa <see cref="IRequest{TResponse}" /> con <see cref="Issue" /> como respuesta,
///     devolviendo la entidad actualizada o <c>null</c> si el issue no se encuentra.
/// </summary>
/// <param name="Id">Identificador único del issue que se desea actualizar.</param>
/// <param name="Summary">Nuevo resumen o título breve del issue.</param>
/// <param name="Problem">Nueva descripción del problema o solicitud.</param>
/// <param name="Analysis">Nuevo análisis técnico o diagnóstico del issue. Puede ser <c>null</c> si no se ha realizado un análisis.</param>
/// <param name="Resolution">Nueva resolución o solución aplicada al issue. Puede ser <c>null</c> si aún no se ha resuelto.</param>
/// <param name="SystemId">Identificador del sistema al que pertenece el issue.</param>
/// <param name="StateId">Identificador del estado actual del issue (<see cref="IssueState" />).</param>
/// <param name="RitmNumber">Nuevo número de solicitud RITM asociado. Puede ser <c>null</c> si no aplica. Debe ser único si se proporciona.</param>
/// <param name="IncidentNumber">Nuevo número de incidente asociado. Puede ser <c>null</c> si no aplica. Debe ser único si se proporciona.</param>
/// <returns>La entidad <see cref="Issue" /> actualizada, o <c>null</c> si no existe ningún issue con el <paramref name="Id" /> especificado.</returns>
public record UpdateIssueCommand(
    Guid Id,
    string Summary,
    string Problem,
    string? Analysis,
    string? Resolution,
    long SystemId,
    int StateId,
    string? RitmNumber,
    string? IncidentNumber) : IRequest<Issue?>;
