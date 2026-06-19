namespace Axiom.Domain.Entities;

public class IssueState
{
    public int StateId { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;

    public ICollection<Issue> Issues { get; private set; } = new HashSet<Issue>();

    private IssueState() { }

    public IssueState(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Code = code;
        Name = name;
    }
}
