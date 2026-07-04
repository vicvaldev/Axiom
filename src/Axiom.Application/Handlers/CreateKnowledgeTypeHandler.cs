using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Manejador del comando <see cref="CreateKnowledgeTypeCommand"/>.
/// Crea una nueva entidad <see cref="KnowledgeType"/> a partir del código y nombre
/// proporcionados en el comando y la persiste a través del repositorio
/// <see cref="IKnowledgeTypeRepository"/>.
/// </summary>
public class CreateKnowledgeTypeHandler : IRequestHandler<CreateKnowledgeTypeCommand, KnowledgeType>
{
    private readonly IKnowledgeTypeRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CreateKnowledgeTypeHandler"/>.
    /// </summary>
    /// <param name="repository">Repositorio de tipos de conocimiento utilizado para persistir la entidad <see cref="KnowledgeType"/>.</param>
    public CreateKnowledgeTypeHandler(IKnowledgeTypeRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Maneja la creación de un nuevo tipo de conocimiento.
    /// Construye una entidad <see cref="KnowledgeType"/> con el código y nombre
    /// especificados en el comando, la guarda en el repositorio y la retorna.
    /// </summary>
    /// <param name="request">Comando que contiene el código y nombre del tipo de conocimiento a crear (<see cref="CreateKnowledgeTypeCommand"/>).</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="KnowledgeType"/> recién creada y persistida.</returns>
    public async Task<KnowledgeType> Handle(CreateKnowledgeTypeCommand request, CancellationToken cancellationToken)
    {
        var entry = new KnowledgeType(request.Code, request.Name);
        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
