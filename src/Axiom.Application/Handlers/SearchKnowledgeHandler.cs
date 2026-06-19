using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using MediatR;

namespace Axiom.Application.Handlers;

public class SearchKnowledgeHandler : IRequestHandler<SearchKnowledgeQuery, IEnumerable<KnowledgeDto>>
{
    private readonly IKnowledgeRepository _repository;

    public SearchKnowledgeHandler(IKnowledgeRepository repository)
    {
        _repository = repository;
    }

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
