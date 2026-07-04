using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
///     Manejador del comando <see cref="UpdateSystemCommand" />.
///     Actualiza los datos de un sistema existente en el repositorio.
///     Si el sistema no se encuentra, retorna <c>null</c> sin realizar cambios.
/// </summary>
public class UpdateSystemHandler : IRequestHandler<UpdateSystemCommand, AxiomSystem?>
{
    private readonly ISystemRepository _repository;

    /// <summary>
    ///     Inicializa una nueva instancia de la clase <see cref="UpdateSystemHandler" />.
    /// </summary>
    /// <param name="repository">Repositorio de sistemas (<see cref="ISystemRepository" />).</param>
    public UpdateSystemHandler(ISystemRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    ///     Procesa el comando de actualización de sistema.
    ///     Busca la entidad por identificador, aplica los cambios con <see cref="AxiomSystem.Update" />
    ///     y persiste la entidad modificada en el repositorio.
    /// </summary>
    /// <param name="request">Comando que contiene el identificador del sistema y los nuevos valores
    /// (<see cref="UpdateSystemCommand.Id" />, <see cref="UpdateSystemCommand.EAI" />,
    /// <see cref="UpdateSystemCommand.Name" />, <see cref="UpdateSystemCommand.OwnerUserId" />).</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="AxiomSystem" /> actualizada, o <c>null</c> si no se encuentra el sistema.</returns>
    public async Task<AxiomSystem?> Handle(UpdateSystemCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return null;

        entry.Update(request.EAI, request.Name, request.OwnerUserId);
        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
