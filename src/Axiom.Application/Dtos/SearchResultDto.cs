namespace Axiom.Application.Dtos;

public class SearchResultDto
{
    public string Id { get; init; } = null!;
    public string EntityType { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string Summary { get; init; } = null!;
    public string SystemName { get; init; } = null!;
    public string StateName { get; init; } = null!;
    public double Score { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string? Highlight { get; init; }
}
