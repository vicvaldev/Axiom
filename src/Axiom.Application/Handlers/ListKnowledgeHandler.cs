using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Controlador (handler) para la consulta <see cref="ListKnowledgeQuery"/>.
/// Obtiene el listado completo de conocimientos (<see cref="KnowledgeDto"/>) sin aplicar filtros.
/// </summary>
public class ListKnowledgeHandler : IRequestHandler<ListKnowledgeQuery, IEnumerable<KnowledgeDto>>
{
    private readonly IKnowledgeRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia del handler con el repositorio de conocimientos especificado.
    /// </summary>
    /// <param name="repository">Repositorio de conocimientos <see cref="IKnowledgeRepository"/> utilizado para acceder a los datos.</param>
    public ListKnowledgeHandler(IKnowledgeRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Maneja la consulta <see cref="ListKnowledgeQuery"/> recuperando todos los conocimientos
    /// y proyectándolos a objetos <see cref="KnowledgeDto"/> para su presentación.
    /// </summary>
    /// <param name="request">Consulta de listado (sin parámetros de filtrado).</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asincrónica.</param>
    /// <returns>
    /// Una tarea que representa la operación asincrónica. El resultado contiene una colección de objetos
    /// <see cref="KnowledgeDto"/> con los datos principales de cada conocimiento.
    /// </returns>
    public async Task<IEnumerable<KnowledgeDto>> Handle(ListKnowledgeQuery request, CancellationToken cancellationToken)
    {
        var entries = await _repository.GetAllAsync(cancellationToken);

        return entries.Select(k => new KnowledgeDto
        {
            KnowledgeId = k.KnowledgeId,
            Title = k.Title,
            Summary = k.Summary,
            SystemName = k.System?.Name ?? string.Empty,
            Tags = k.KnowledgeKnowledgeTags?.Select(t => t.Tag?.TagName ?? string.Empty).ToList() ?? [],
            TypeName = k.Type?.Name ?? string.Empty,
            StateName = k.State?.Name ?? string.Empty,
            CreatedByName = k.CreatedBy?.Name ?? string.Empty,
            VersionNumber = k.VersionNumber,
            UpdatedAt = k.UpdatedAt
        });
    }
}
