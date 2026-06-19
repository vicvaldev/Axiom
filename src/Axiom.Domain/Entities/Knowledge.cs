namespace Axiom.Domain.Entities;

public class Knowledge
{
    public Guid KnowledgeId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Summary { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public long SystemId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public long KnowledgeTypeId { get; private set; }
    public int KnowledgeStateId { get; private set; }
    public Guid? IssueId { get; private set; }
    public int VersionNumber { get; private set; }

    public AxiomSystem System { get; private set; } = null!;
    public User CreatedBy { get; private set; } = null!;
    public KnowledgeType Type { get; private set; } = null!;
    public KnowledgeState State { get; private set; } = null!;
    public Issue? Issue { get; private set; }
    public ICollection<KnowledgeKnowledgeTag> KnowledgeKnowledgeTags { get; private set; } = new HashSet<KnowledgeKnowledgeTag>();

    private Knowledge() { }

    public Knowledge(
        string title,
        string summary,
        string content,
        long systemId,
        Guid createdByUserId,
        long knowledgeTypeId,
        int knowledgeStateId,
        Guid? issueId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));

        KnowledgeId = Guid.NewGuid();
        Title = title;
        Summary = summary ?? string.Empty;
        Content = content;
        SystemId = systemId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
        KnowledgeTypeId = knowledgeTypeId;
        KnowledgeStateId = knowledgeStateId;
        IssueId = issueId;
        VersionNumber = 1;
    }

    public void Update(
        string title,
        string summary,
        string content,
        long systemId,
        long knowledgeTypeId,
        int knowledgeStateId,
        Guid? issueId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));

        Title = title;
        Summary = summary ?? string.Empty;
        Content = content;
        SystemId = systemId;
        KnowledgeTypeId = knowledgeTypeId;
        KnowledgeStateId = knowledgeStateId;
        IssueId = issueId;
        UpdatedAt = DateTime.UtcNow;
        VersionNumber++;
    }
}
