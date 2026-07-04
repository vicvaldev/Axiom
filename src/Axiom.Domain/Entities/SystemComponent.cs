namespace Axiom.Domain.Entities;

public class SystemComponent
{
    public Guid SystemComponentId { get; private set; }
    public long SystemId { get; private set; }
    public Guid ComponentId { get; private set; }
    public string? RoleDescription { get; private set; }
    public bool IsOwner { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public AxiomSystem System { get; private set; } = null!;
    public TechnicalComponent Component { get; private set; } = null!;

    private SystemComponent() { }

    public SystemComponent(
        long systemId,
        Guid componentId,
        bool isOwner,
        string? roleDescription = null,
        Guid? systemComponentId = null)
    {
        SystemComponentId = systemComponentId ?? Guid.NewGuid();
        SystemId = systemId;
        ComponentId = componentId;
        IsOwner = isOwner;
        RoleDescription = roleDescription ?? string.Empty;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string? roleDescription, bool isOwner)
    {
        RoleDescription = roleDescription ?? string.Empty;
        IsOwner = isOwner;
        UpdatedAt = DateTime.UtcNow;
    }
}
