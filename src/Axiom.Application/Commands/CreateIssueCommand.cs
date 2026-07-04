using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Commands;

/// <summary>
///     Comando CQRS para crear un nuevo issue (incidencia o solicitud) en el sistema.
///     Un issue representa un problema, solicitud de cambio o incidencia registrada.
/// </summary>
/// <param name="Summary">Resumen breve del issue.</param>
/// <param name="SystemId">Identificador del sistema al que pertenece el issue.</param>
/// <param name="Problem">Descripción detallada del problema o solicitud.</param>
/// <param name="Analysis">Análisis técnico del problema.</param>
/// <param name="Resolution">Resolución o solución aplicada al issue.</param>
/// <param name="StateId">Identificador del estado inicial del issue (abierto, en análisis, resuelto, etc.).</param>
/// <param name="CreatedByUserId">Identificador del usuario que crea el issue.</param>
/// <param name="RitmNumber">Número opcional de RITM (Request Item) asociado desde el sistema de ticketing. Debe ser único si se proporciona.</param>
/// <param name="IncidentNumber">Número opcional de incidencia asociado desde el sistema de ticketing. Debe ser único si se proporciona.</param>
/// <param name="IssueId">Identificador opcional para asignar manualmente al issue. Si se omite, se genera automáticamente.</param>
/// <returns>La entidad <see cref="Issue"/> creada, incluyendo su identificador único asignado.</returns>
public record CreateIssueCommand(
    string Summary,
    long SystemId,
    string Problem,
    string Analysis,
    string Resolution,
    int StateId,
    Guid CreatedByUserId,
    string? RitmNumber,
    string? IncidentNumber,
    Guid? IssueId = null) : IRequest<Issue>;
