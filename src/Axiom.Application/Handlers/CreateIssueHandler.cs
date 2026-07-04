using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Manejador del comando <see cref="CreateIssueCommand"/>.
/// Crea una nueva entidad <see cref="Issue"/> a partir de los datos proporcionados
/// en el comando y la persiste a través del repositorio <see cref="IIssueRepository"/>.
/// </summary>
public class CreateIssueHandler : IRequestHandler<CreateIssueCommand, Issue>
{
    private readonly IIssueRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CreateIssueHandler"/>.
    /// </summary>
    /// <param name="repository">Repositorio de incidencias utilizado para persistir la entidad <see cref="Issue"/>.</param>
    public CreateIssueHandler(IIssueRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Maneja la creación de una nueva incidencia.
    /// Construye una entidad <see cref="Issue"/> con todos los datos especificados
    /// en el comando (resumen, sistema, problema, estado, análisis, resolución,
    /// números de RITM e incidencia, etc.), la guarda en el repositorio y la retorna.
    /// </summary>
    /// <param name="request">Comando que contiene los datos de la incidencia a crear (<see cref="CreateIssueCommand"/>).</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="Issue"/> recién creada y persistida.</returns>
    public async Task<Issue> Handle(CreateIssueCommand request, CancellationToken cancellationToken)
    {
        var issue = new Issue(
            request.Summary,
            request.SystemId,
            request.Problem,
            request.StateId,
            request.CreatedByUserId,
            request.Analysis,
            request.Resolution,
            request.RitmNumber,
            request.IncidentNumber,
            request.IssueId);

        await _repository.SaveAsync(issue, cancellationToken);
        return issue;
    }
}
