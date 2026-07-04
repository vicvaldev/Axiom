using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;
using Axiom.Domain.Entities;

namespace Axiom.Application.Handlers;

/// <summary>
///     Manejador del comando <see cref="UpdateIssueCommand" />.
///     Actualiza los datos de una incidencia/ticket existente.
///     Si la incidencia no se encuentra, retorna <c>null</c> sin realizar cambios.
/// </summary>
public class UpdateIssueHandler : IRequestHandler<UpdateIssueCommand, Issue?>
{
    private readonly IIssueRepository _repository;

    /// <summary>
    ///     Inicializa una nueva instancia de la clase <see cref="UpdateIssueHandler" />.
    /// </summary>
    /// <param name="repository">Repositorio de incidencias (<see cref="IIssueRepository" />).</param>
    public UpdateIssueHandler(IIssueRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    ///     Procesa el comando de actualización de incidencia.
    ///     Busca la entidad por identificador, aplica los cambios con <see cref="Issue.Update" />
    ///     y persiste la entidad modificada en el repositorio.
    /// </summary>
    /// <param name="request">Comando que contiene el identificador de la incidencia y los nuevos valores
    /// (<see cref="UpdateIssueCommand.Id" />, <see cref="UpdateIssueCommand.Summary" />,
    /// <see cref="UpdateIssueCommand.Problem" />, <see cref="UpdateIssueCommand.Analysis" />,
    /// <see cref="UpdateIssueCommand.Resolution" />, <see cref="UpdateIssueCommand.SystemId" />,
    /// <see cref="UpdateIssueCommand.StateId" />, <see cref="UpdateIssueCommand.RitmNumber" />,
    /// <see cref="UpdateIssueCommand.IncidentNumber" />).</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="Issue" /> actualizada, o <c>null</c> si no se encuentra la incidencia.</returns>
    public async Task<Issue?> Handle(UpdateIssueCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return null;

        entry.Update(
            request.Summary,
            request.Problem,
            request.Analysis,
            request.Resolution,
            request.SystemId,
            request.StateId,
            request.RitmNumber,
            request.IncidentNumber);

        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
