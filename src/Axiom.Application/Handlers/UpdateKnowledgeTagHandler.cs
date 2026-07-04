using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
///     Manejador del comando <see cref="UpdateKnowledgeTagCommand" />.
///     Actualiza el nombre de una etiqueta de conocimiento existente.
///     Si la etiqueta no se encuentra, retorna <c>null</c> sin realizar cambios.
/// </summary>
public class UpdateKnowledgeTagHandler : IRequestHandler<UpdateKnowledgeTagCommand, KnowledgeTag?>
{
    private readonly IKnowledgeTagRepository _repository;

    /// <summary>
    ///     Inicializa una nueva instancia de la clase <see cref="UpdateKnowledgeTagHandler" />.
    /// </summary>
    /// <param name="repository">Repositorio de etiquetas de conocimiento
    /// (<see cref="IKnowledgeTagRepository" />).</param>
    public UpdateKnowledgeTagHandler(IKnowledgeTagRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    ///     Procesa el comando de actualización de etiqueta de conocimiento.
    ///     Busca la entidad por identificador, aplica el cambio con <see cref="KnowledgeTag.Update" />
    ///     y persiste la entidad modificada en el repositorio.
    /// </summary>
    /// <param name="request">Comando que contiene el identificador de la etiqueta y el nuevo nombre
    /// (<see cref="UpdateKnowledgeTagCommand.Id" />, <see cref="UpdateKnowledgeTagCommand.TagName" />).</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="KnowledgeTag" /> actualizada, o <c>null</c> si no se encuentra la etiqueta.</returns>
    public async Task<KnowledgeTag?> Handle(UpdateKnowledgeTagCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return null;

        entry.Update(request.TagName);
        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
