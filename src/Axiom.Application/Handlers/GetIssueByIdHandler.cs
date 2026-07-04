using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Controlador (handler) para la consulta <see cref="GetIssueByIdQuery"/>.
/// Obtiene una incidencia (<see cref="Issue"/>) a partir de su identificador único.
/// </summary>
public class GetIssueByIdHandler : IRequestHandler<GetIssueByIdQuery, Issue?>
{
    private readonly IIssueRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia del handler con el repositorio de incidencias especificado.
    /// </summary>
    /// <param name="repository">Repositorio de incidencias <see cref="IIssueRepository"/> utilizado para acceder a los datos.</param>
    public GetIssueByIdHandler(IIssueRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Maneja la consulta <see cref="GetIssueByIdQuery"/> y devuelve la incidencia correspondiente al identificador proporcionado.
    /// </summary>
    /// <param name="request">Consulta que contiene el identificador único (<see cref="GetIssueByIdQuery.Id"/>) de la incidencia a recuperar.</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asincrónica.</param>
    /// <returns>
    /// Una tarea que representa la operación asincrónica. El resultado contiene el objeto <see cref="Issue"/>
    /// si se encuentra; de lo contrario, <c>null</c>.
    /// </returns>
    public async Task<Issue?> Handle(GetIssueByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id, cancellationToken);
    }
}
