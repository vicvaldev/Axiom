using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
///     Manejador del comando <see cref="UpdateKnowledgeStateCommand" />.
///     Actualiza los datos de un estado de conocimiento existente.
///     Si el estado no se encuentra, retorna <c>null</c> sin realizar cambios.
/// </summary>
public class UpdateKnowledgeStateHandler : IRequestHandler<UpdateKnowledgeStateCommand, KnowledgeState?>
{
    private readonly IKnowledgeStateRepository _repository;

    /// <summary>
    ///     Inicializa una nueva instancia de la clase <see cref="UpdateKnowledgeStateHandler" />.
    /// </summary>
    /// <param name="repository">Repositorio de estados de conocimiento
    /// (<see cref="IKnowledgeStateRepository" />).</param>
    public UpdateKnowledgeStateHandler(IKnowledgeStateRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    ///     Procesa el comando de actualización de estado de conocimiento.
    ///     Busca la entidad por identificador, aplica los cambios con <see cref="KnowledgeState.Update" />
    ///     y persiste la entidad modificada en el repositorio.
    /// </summary>
    /// <param name="request">Comando que contiene el identificador del estado y los nuevos valores
    /// (<see cref="UpdateKnowledgeStateCommand.Id" />, <see cref="UpdateKnowledgeStateCommand.Code" />,
    /// <see cref="UpdateKnowledgeStateCommand.Name" />).</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="KnowledgeState" /> actualizada, o <c>null</c> si no se encuentra el estado.</returns>
    public async Task<KnowledgeState?> Handle(UpdateKnowledgeStateCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return null;

        entry.Update(request.Code, request.Name);
        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
