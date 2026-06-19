using Axiom.Application.Dtos;

namespace Axiom.Application.Interfaces;

public interface ISearchService
{
    Task<IEnumerable<SearchResultDto>> AskAsync(string question, CancellationToken ct = default);
}
