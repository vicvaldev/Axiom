using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Manejador del comando <see cref="CreateKnowledgeStateCommand"/>.
/// Crea una nueva entidad <see cref="KnowledgeState"/> a partir del código y nombre
/// proporcionados en el comando y la persiste a través del repositorio
/// <see cref="IKnowledgeStateRepository"/>.
/// </summary>
public class CreateKnowledgeStateHandler : IRequestHandler<CreateKnowledgeStateCommand, KnowledgeState>
{
    private readonly IKnowledgeStateRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CreateKnowledgeStateHandler"/>.
    /// </summary>
    /// <param name="repository">Repositorio de estados de conocimiento utilizado para persistir la entidad <see cref="KnowledgeState"/>.</param>
    public CreateKnowledgeStateHandler(IKnowledgeStateRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Maneja la creación de un nuevo estado de conocimiento.
    /// Construye una entidad <see cref="KnowledgeState"/> con el código y nombre
    /// especificados en el comando, la guarda en el repositorio y la retorna.
    /// </summary>
    /// <param name="request">Comando que contiene el código y nombre del estado de conocimiento a crear (<see cref="CreateKnowledgeStateCommand"/>).</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La entidad <see cref="KnowledgeState"/> recién creada y persistida.</returns>
    public async Task<KnowledgeState> Handle(CreateKnowledgeStateCommand request, CancellationToken cancellationToken)
    {
        var entry = new KnowledgeState(request.Code, request.Name);
        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
