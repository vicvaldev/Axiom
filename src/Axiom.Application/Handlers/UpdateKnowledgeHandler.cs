using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;
using Axiom.Domain.Entities;

namespace Axiom.Application.Handlers;

/// <summary>
///     Manejador del comando <see cref="UpdateKnowledgeCommand" />.
///     Actualiza los datos de un artículo de conocimiento existente, incluyendo
///     la sincronización de sus etiquetas (<see cref="KnowledgeKnowledgeTag" />).
///     Si el conocimiento no se encuentra, retorna <c>null</c> sin realizar cambios.
/// </summary>
public class UpdateKnowledgeHandler : IRequestHandler<UpdateKnowledgeCommand, Knowledge?>
{
    private readonly IKnowledgeRepository _repository;
    private readonly ITagRepository _tagRepository;

    /// <summary>
    ///     Inicializa una nueva instancia de la clase <see cref="UpdateKnowledgeHandler" />.
    /// </summary>
    /// <param name="repository">Repositorio de conocimientos (<see cref="IKnowledgeRepository" />).</param>
    /// <param name="tagRepository">Repositorio de etiquetas (<see cref="ITagRepository" />)
    /// para buscar o crear etiquetas durante la actualización.</param>
    public UpdateKnowledgeHandler(IKnowledgeRepository repository, ITagRepository tagRepository)
    {
        _repository = repository;
        _tagRepository = tagRepository;
    }

    /// <summary>
    ///     Procesa el comando de actualización de conocimiento.
    ///     Busca la entidad por identificador, aplica los cambios con <see cref="Knowledge.Update" />,
    ///     reemplaza la colección de etiquetas (<see cref="Knowledge.KnowledgeKnowledgeTags" />)
    ///     usando <see cref="ITagRepository.FindOrCreateAsync" /> para cada etiqueta solicitada,
    ///     y persiste la entidad modificada en el repositorio.
    /// </summary>
    /// <param name="request">Comando que contiene el identificador del conocimiento y los nuevos valores
    /// (<see cref="UpdateKnowledgeCommand.Id" />, <see cref="UpdateKnowledgeCommand.Title" />,
    /// <see cref="UpdateKnowledgeCommand.Summary" />, <see cref="UpdateKnowledgeCommand.Content" />,
    /// <see cref="UpdateKnowledgeCommand.SystemId" />, <see cref="UpdateKnowledgeCommand.KnowledgeTypeId" />,
    /// <see cref="UpdateKnowledgeCommand.KnowledgeStateId" />, <see cref="UpdateKnowledgeCommand.IssueId" />,
    /// <see cref="UpdateKnowledgeCommand.Tags" />).</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="Knowledge" /> actualizada, o <c>null</c> si no se encuentra el conocimiento.</returns>
    public async Task<Knowledge?> Handle(UpdateKnowledgeCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entry is null)
            return null;

        entry.Update(
            request.Title,
            request.Summary,
            request.Content,
            request.SystemId,
            request.KnowledgeTypeId,
            request.KnowledgeStateId,
            request.IssueId);

        entry.KnowledgeKnowledgeTags.Clear();
        if (request.Tags?.Count > 0)
        {
            foreach (var tagName in request.Tags)
            {
                var tag = await _tagRepository.FindOrCreateAsync(tagName, cancellationToken);
                entry.KnowledgeKnowledgeTags.Add(
                    new KnowledgeKnowledgeTag(entry.KnowledgeId, tag.KnowledgeTagId));
            }
        }

        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
