namespace Axiom.Application.Dtos;

public class ComponentDependencyDto
{
    public Guid DependencyId { get; init; }
    public Guid SourceComponentId { get; init; }
    public string SourceComponentName { get; init; } = null!;
    public string SourceTechnicalName { get; init; } = null!;
    public Guid TargetComponentId { get; init; }
    public string TargetComponentName { get; init; } = null!;
    public string TargetTechnicalName { get; init; } = null!;
    public string DependencyType { get; init; } = null!;
    public string? Description { get; init; }
    public string Criticality { get; init; } = null!;
    public string Status { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
