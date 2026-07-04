using Axiom.Domain.Enums;

namespace Axiom.Domain.Entities;

public class TechnicalComponent
{
    public Guid ComponentId { get; private set; }
    public string Name { get; private set; } = null!;
    public string TechnicalName { get; private set; } = null!;
    public ComponentType ComponentType { get; private set; }
    public string? Description { get; private set; }
    public TargetEnvironment Environment { get; private set; }
    public Criticality Criticality { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public long SystemId { get; private set; }

    public AxiomSystem System { get; private set; } = null!;
    public ICollection<SystemComponent> SystemComponents { get; private set; } = new HashSet<SystemComponent>();
    public ICollection<ComponentDependency> OutgoingDependencies { get; private set; } = new HashSet<ComponentDependency>();
    public ICollection<ComponentDependency> IncomingDependencies { get; private set; } = new HashSet<ComponentDependency>();

    private TechnicalComponent() { }

    public TechnicalComponent(
        string name,
        string technicalName,
        ComponentType componentType,
        TargetEnvironment environment,
        Criticality criticality,
        long systemId,
        string? description = null,
        Guid? componentId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(technicalName))
            throw new ArgumentException("TechnicalName cannot be empty.", nameof(technicalName));

        ComponentId = componentId ?? Guid.NewGuid();
        Name = name;
        TechnicalName = technicalName;
        ComponentType = componentType;
        Description = description ?? string.Empty;
        Environment = environment;
        Criticality = criticality;
        SystemId = systemId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        string name,
        string technicalName,
        ComponentType componentType,
        TargetEnvironment environment,
        Criticality criticality,
        long systemId,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(technicalName))
            throw new ArgumentException("TechnicalName cannot be empty.", nameof(technicalName));

        Name = name;
        TechnicalName = technicalName;
        ComponentType = componentType;
        Description = description ?? string.Empty;
        Environment = environment;
        Criticality = criticality;
        SystemId = systemId;
        UpdatedAt = DateTime.UtcNow;
    }
}
