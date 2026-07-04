using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Controlador (handler) para la consulta <see cref="ListIssuesQuery"/>.
/// Obtiene el listado de incidencias (<see cref="IssueDto"/>), opcionalmente filtrado por el EAI del sistema.
/// </summary>
public class ListIssuesHandler : IRequestHandler<ListIssuesQuery, IEnumerable<IssueDto>>
{
    private readonly IIssueRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia del handler con el repositorio de incidencias especificado.
    /// </summary>
    /// <param name="repository">Repositorio de incidencias <see cref="IIssueRepository"/> utilizado para acceder a los datos.</param>
    public ListIssuesHandler(IIssueRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Maneja la consulta <see cref="ListIssuesQuery"/> recuperando todas las incidencias o solo aquellas
    /// que pertenecen al sistema identificado por <see cref="ListIssuesQuery.Eai"/>,
    /// y las proyecta a objetos <see cref="IssueDto"/> para su presentación.
    /// </summary>
    /// <param name="request">
    /// Consulta que puede contener un valor de EAI opcional en <see cref="ListIssuesQuery.Eai"/>.
    /// Si se proporciona, solo se devuelven las incidencias del sistema con dicho EAI;
    /// de lo contrario, se devuelven todas las incidencias.
    /// </param>
    /// <param name="cancellationToken">Token de cancelación para la operación asincrónica.</param>
    /// <returns>
    /// Una tarea que representa la operación asincrónica. El resultado contiene una colección de objetos
    /// <see cref="IssueDto"/> con los datos principales de cada incidencia.
    /// </returns>
    public async Task<IEnumerable<IssueDto>> Handle(ListIssuesQuery request, CancellationToken cancellationToken)
    {
        var issues = string.IsNullOrWhiteSpace(request.Eai)
            ? await _repository.GetAllAsync(cancellationToken)
            : await _repository.GetByEaiAsync(request.Eai, cancellationToken);

        return issues.Select(i => new IssueDto
        {
            IssueId = i.IssueId,
            Summary = i.Summary,
            SystemName = i.System?.Name ?? string.Empty,
            StateName = i.State?.Name ?? string.Empty,
            RitmNumber = i.RitmNumber,
            IncidentNumber = i.IncidentNumber,
            CreatedAt = i.CreatedAt,
            ResolvedAt = i.ResolvedAt
        });
    }
}
