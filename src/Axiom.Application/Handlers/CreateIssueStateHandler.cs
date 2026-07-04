using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Manejador del comando <see cref="CreateIssueStateCommand"/>.
/// Crea una nueva entidad <see cref="IssueState"/> a partir del código y nombre
/// proporcionados en el comando y la persiste a través del repositorio
/// <see cref="IIssueStateRepository"/>.
/// </summary>
public class CreateIssueStateHandler : IRequestHandler<CreateIssueStateCommand, IssueState>
{
    private readonly IIssueStateRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CreateIssueStateHandler"/>.
    /// </summary>
    /// <param name="repository">Repositorio de estados de incidencia utilizado para persistir la entidad <see cref="IssueState"/>.</param>
    public CreateIssueStateHandler(IIssueStateRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Maneja la creación de un nuevo estado de incidencia.
    /// Construye una entidad <see cref="IssueState"/> con el código y nombre
    /// especificados en el comando, la guarda en el repositorio y la retorna.
    /// </summary>
    /// <param name="request">Comando que contiene el código y nombre del estado de incidencia a crear (<see cref="CreateIssueStateCommand"/>).</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="IssueState"/> recién creada y persistida.</returns>
    public async Task<IssueState> Handle(CreateIssueStateCommand request, CancellationToken cancellationToken)
    {
        var entry = new IssueState(request.Code, request.Name);
        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
