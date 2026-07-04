namespace Axiom.Application.Dtos;

public class TechnicalComponentDto
{
    public Guid ComponentId { get; init; }
    public string Name { get; init; } = null!;
    public string TechnicalName { get; init; } = null!;
    public string ComponentType { get; init; } = null!;
    public string? Description { get; init; }
    public string Environment { get; init; } = null!;
    public string Criticality { get; init; } = null!;
    public long SystemId { get; init; }
    public string SystemName { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
