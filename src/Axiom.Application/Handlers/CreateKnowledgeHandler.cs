using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Manejador del comando <see cref="CreateKnowledgeCommand"/>.
/// Crea una nueva entidad <see cref="Knowledge"/> a partir de los datos proporcionados
/// en el comando, incluyendo la gestión de etiquetas asociadas a través del repositorio
/// <see cref="ITagRepository"/>, y persiste el resultado mediante <see cref="IKnowledgeRepository"/>.
/// </summary>
public class CreateKnowledgeHandler : IRequestHandler<CreateKnowledgeCommand, Knowledge>
{
    private readonly IKnowledgeRepository _repository;
    private readonly ITagRepository _tagRepository;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CreateKnowledgeHandler"/>.
    /// </summary>
    /// <param name="repository">Repositorio de conocimientos utilizado para persistir la entidad <see cref="Knowledge"/>.</param>
    /// <param name="tagRepository">Repositorio de etiquetas utilizado para buscar o crear las etiquetas asociadas al conocimiento.</param>
    public CreateKnowledgeHandler(IKnowledgeRepository repository, ITagRepository tagRepository)
    {
        _repository = repository;
        _tagRepository = tagRepository;
    }

    /// <summary>
    /// Maneja la creación de un nuevo conocimiento.
    /// Construye una entidad <see cref="Knowledge"/> con los datos del comando,
    /// procesa las etiquetas opcionales (<see cref="KnowledgeKnowledgeTag"/>) buscándolas
    /// o creándolas según sea necesario, la guarda en el repositorio y la retorna.
    /// </summary>
    /// <param name="request">Comando que contiene los datos del conocimiento a crear (<see cref="CreateKnowledgeCommand"/>).</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="Knowledge"/> recién creada y persistida, incluyendo sus relaciones con etiquetas.</returns>
    public async Task<Knowledge> Handle(CreateKnowledgeCommand request, CancellationToken cancellationToken)
    {
        var entry = new Knowledge(
            request.Title,
            request.Summary,
            request.Content,
            request.SystemId,
            request.CreatedByUserId,
            request.KnowledgeTypeId,
            request.KnowledgeStateId,
            request.IssueId,
            request.KnowledgeId);

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
