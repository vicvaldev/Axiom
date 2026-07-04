using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Manejador del comando <see cref="CreateKnowledgeTagCommand"/>.
/// Crea una nueva entidad <see cref="KnowledgeTag"/> a partir del nombre proporcionado
/// en el comando y la persiste a través del repositorio <see cref="IKnowledgeTagRepository"/>.
/// </summary>
public class CreateKnowledgeTagHandler : IRequestHandler<CreateKnowledgeTagCommand, KnowledgeTag>
{
    private readonly IKnowledgeTagRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CreateKnowledgeTagHandler"/>.
    /// </summary>
    /// <param name="repository">Repositorio de etiquetas de conocimiento utilizado para persistir la entidad <see cref="KnowledgeTag"/>.</param>
    public CreateKnowledgeTagHandler(IKnowledgeTagRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Maneja la creación de una nueva etiqueta de conocimiento.
    /// Construye una entidad <see cref="KnowledgeTag"/> con el nombre especificado
    /// en el comando, la guarda en el repositorio y la retorna.
    /// </summary>
    /// <param name="request">Comando que contiene el nombre de la etiqueta a crear (<see cref="CreateKnowledgeTagCommand"/>).</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="KnowledgeTag"/> recién creada y persistida.</returns>
    public async Task<KnowledgeTag> Handle(CreateKnowledgeTagCommand request, CancellationToken cancellationToken)
    {
        var tag = new KnowledgeTag(request.TagName);
        await _repository.SaveAsync(tag, cancellationToken);
        return tag;
    }
}
