namespace Axiom.Domain.Entities;

public class KnowledgeState
{
    public int StateId { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;

    public ICollection<Knowledge> Knowledges { get; private set; } = new HashSet<Knowledge>();

    private KnowledgeState() { }

    public KnowledgeState(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Code = code;
        Name = name;
    }
}
