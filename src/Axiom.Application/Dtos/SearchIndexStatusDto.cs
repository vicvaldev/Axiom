namespace Axiom.Application.Dtos;

public class SearchIndexStatusDto
{
    public string IndexName { get; init; } = null!;
    public long DocumentCount { get; init; }
}
