using Axiom.Application.Dtos;
using MediatR;

namespace Axiom.Application.Queries;

/// <summary>
///     Consulta para obtener una lista de incidencias, con la posibilidad de filtrar
///     opcionalmente por el número EAI del sistema asociado.
///     Si no se especifica el filtro, devuelve todas las incidencias registradas.
/// </summary>
/// <param name="Eai">
///     (Opcional) Número EAI del sistema para filtrar las incidencias.
///     Si es <c>null</c> o se omite, se devuelven todas las incidencias sin filtrar.
/// </param>
/// <returns>
///     Colección de objetos <see cref="IssueDto"/> que representan las incidencias
///     encontradas según el filtro aplicado.
/// </returns>
public record ListIssuesQuery(string? Eai = null) : IRequest<IEnumerable<IssueDto>>;
