using Axiom.Domain.Enums;

namespace Axiom.Domain.Entities;

public class ComponentDependency
{
    public Guid DependencyId { get; private set; }
    public Guid SourceComponentId { get; private set; }
    public Guid TargetComponentId { get; private set; }
    public DependencyType DependencyType { get; private set; }
    public string? Description { get; private set; }
    public Criticality Criticality { get; private set; }
    public DependencyStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public TechnicalComponent Source { get; private set; } = null!;
    public TechnicalComponent Target { get; private set; } = null!;
    public ICollection<DependencyTraceEvent> TraceEvents { get; private set; } = new HashSet<DependencyTraceEvent>();

    private ComponentDependency() { }

    public ComponentDependency(
        Guid sourceComponentId,
        Guid targetComponentId,
        DependencyType dependencyType,
        Criticality criticality,
        DependencyStatus status,
        string? description = null,
        Guid? dependencyId = null)
    {
        if (sourceComponentId == targetComponentId)
            throw new ArgumentException("Source and target component cannot be the same.", nameof(sourceComponentId));

        DependencyId = dependencyId ?? Guid.NewGuid();
        SourceComponentId = sourceComponentId;
        TargetComponentId = targetComponentId;
        DependencyType = dependencyType;
        Criticality = criticality;
        Status = status;
        Description = description ?? string.Empty;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        DependencyType dependencyType,
        Criticality criticality,
        DependencyStatus status,
        string? description = null)
    {
        DependencyType = dependencyType;
        Criticality = criticality;
        Status = status;
        Description = description ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }
}
