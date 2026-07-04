using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
///     Manejador del comando <see cref="UpdateKnowledgeTypeCommand" />.
///     Actualiza los datos de un tipo de conocimiento existente.
///     Si el tipo no se encuentra, retorna <c>null</c> sin realizar cambios.
/// </summary>
public class UpdateKnowledgeTypeHandler : IRequestHandler<UpdateKnowledgeTypeCommand, KnowledgeType?>
{
    private readonly IKnowledgeTypeRepository _repository;

    /// <summary>
    ///     Inicializa una nueva instancia de la clase <see cref="UpdateKnowledgeTypeHandler" />.
    /// </summary>
    /// <param name="repository">Repositorio de tipos de conocimiento
    /// (<see cref="IKnowledgeTypeRepository" />).</param>
    public UpdateKnowledgeTypeHandler(IKnowledgeTypeRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    ///     Procesa el comando de actualización de tipo de conocimiento.
    ///     Busca la entidad por identificador, aplica los cambios con <see cref="KnowledgeType.Update" />
    ///     y persiste la entidad modificada en el repositorio.
    /// </summary>
    /// <param name="request">Comando que contiene el identificador del tipo y los nuevos valores
    /// (<see cref="UpdateKnowledgeTypeCommand.Id" />, <see cref="UpdateKnowledgeTypeCommand.Code" />,
    /// <see cref="UpdateKnowledgeTypeCommand.Name" />).</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="KnowledgeType" /> actualizada, o <c>null</c> si no se encuentra el tipo.</returns>
    public async Task<KnowledgeType?> Handle(UpdateKnowledgeTypeCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return null;

        entry.Update(request.Code, request.Name);
        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
