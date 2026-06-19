using Axiom.Application.Dtos;

namespace Axiom.Application.Interfaces;

public interface ISearchIndexStatusService
{
    Task<IEnumerable<SearchIndexStatusDto>> GetStatusAsync(CancellationToken ct = default);
}
