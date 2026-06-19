using Axiom.Domain.Entities;

namespace Axiom.Application.Interfaces;

public interface ITagRepository
{
    Task<KnowledgeTag> FindOrCreateAsync(string tagName, CancellationToken cancellationToken = default);
}
