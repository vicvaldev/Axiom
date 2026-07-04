using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Controlador (handler) para la consulta <see cref="SearchKnowledgeQuery"/>.
/// Busca conocimientos (<see cref="KnowledgeDto"/>) cuyo contenido coincida con el texto de búsqueda proporcionado.
/// </summary>
public class SearchKnowledgeHandler : IRequestHandler<SearchKnowledgeQuery, IEnumerable<KnowledgeDto>>
{
    private readonly IKnowledgeRepository _repository;

    /// <summary>
    /// Inicializa una nueva instancia del handler con el repositorio de conocimientos especificado.
    /// </summary>
    /// <param name="repository">Repositorio de conocimientos <see cref="IKnowledgeRepository"/> utilizado para acceder a los datos.</param>
    public SearchKnowledgeHandler(IKnowledgeRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Maneja la consulta <see cref="SearchKnowledgeQuery"/> ejecutando la búsqueda de conocimientos
    /// a partir del texto especificado en <see cref="SearchKnowledgeQuery.Query"/>
    /// y proyectando los resultados a objetos <see cref="KnowledgeDto"/>.
    /// </summary>
    /// <param name="request">Consulta que contiene el término de búsqueda (<see cref="SearchKnowledgeQuery.Query"/>) a utilizar.</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asincrónica.</param>
    /// <returns>
    /// Una tarea que representa la operación asincrónica. El resultado contiene una colección de objetos
    /// <see cref="KnowledgeDto"/> que coinciden con el criterio de búsqueda.
    /// </returns>
    public async Task<IEnumerable<KnowledgeDto>> Handle(SearchKnowledgeQuery request, CancellationToken cancellationToken)
    {
        var results = await _repository.SearchAsync(request.Query, cancellationToken);

        return results.Select(k => new KnowledgeDto
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
