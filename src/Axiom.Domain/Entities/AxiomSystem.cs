namespace Axiom.Domain.Entities;

public class AxiomSystem
{
    public long SystemId { get; private set; }
    public string EAI { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public Guid OwnerUserId { get; private set; }

    public User Owner { get; private set; } = null!;
    public ICollection<Issue> Issues { get; private set; } = new HashSet<Issue>();
    public ICollection<Knowledge> Knowledges { get; private set; } = new HashSet<Knowledge>();

    private AxiomSystem() { }

    public AxiomSystem(string eai, string name, Guid ownerUserId)
    {
        if (string.IsNullOrWhiteSpace(eai))
            throw new ArgumentException("EAI cannot be empty.", nameof(eai));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        EAI = eai;
        Name = name;
        OwnerUserId = ownerUserId;
    }
}
