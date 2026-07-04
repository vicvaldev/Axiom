using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
///     Manejador del comando <see cref="UpdateIssueStateCommand" />.
///     Actualiza los datos de un estado de incidencia existente.
///     Si el estado no se encuentra, retorna <c>null</c> sin realizar cambios.
/// </summary>
public class UpdateIssueStateHandler : IRequestHandler<UpdateIssueStateCommand, IssueState?>
{
    private readonly IIssueStateRepository _repository;

    /// <summary>
    ///     Inicializa una nueva instancia de la clase <see cref="UpdateIssueStateHandler" />.
    /// </summary>
    /// <param name="repository">Repositorio de estados de incidencia
    /// (<see cref="IIssueStateRepository" />).</param>
    public UpdateIssueStateHandler(IIssueStateRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    ///     Procesa el comando de actualización de estado de incidencia.
    ///     Busca la entidad por identificador, aplica los cambios con <see cref="IssueState.Update" />
    ///     y persiste la entidad modificada en el repositorio.
    /// </summary>
    /// <param name="request">Comando que contiene el identificador del estado y los nuevos valores
    /// (<see cref="UpdateIssueStateCommand.Id" />, <see cref="UpdateIssueStateCommand.Code" />,
    /// <see cref="UpdateIssueStateCommand.Name" />).</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="IssueState" /> actualizada, o <c>null</c> si no se encuentra el estado.</returns>
    public async Task<IssueState?> Handle(UpdateIssueStateCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return null;

        entry.Update(request.Code, request.Name);
        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
