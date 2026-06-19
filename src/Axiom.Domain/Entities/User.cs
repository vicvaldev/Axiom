namespace Axiom.Domain.Entities;

public class User
{
    public Guid UserId { get; private set; }
    public string Email { get; private set; } = null!;
    public string Name { get; private set; } = null!;

    public ICollection<AxiomSystem> OwnedSystems { get; private set; } = new HashSet<AxiomSystem>();
    public ICollection<Issue> CreatedIssues { get; private set; } = new HashSet<Issue>();
    public ICollection<Knowledge> CreatedKnowledges { get; private set; } = new HashSet<Knowledge>();

    private User() { }

    public User(string email, string name)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        UserId = Guid.NewGuid();
        Email = email;
        Name = name;
    }
}
