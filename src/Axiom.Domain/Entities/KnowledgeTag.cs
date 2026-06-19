namespace Axiom.Domain.Entities;

public class KnowledgeTag
{
    public long KnowledgeTagId { get; private set; }
    public string TagName { get; private set; } = null!;

    public ICollection<KnowledgeKnowledgeTag> KnowledgeKnowledgeTags { get; private set; } = new HashSet<KnowledgeKnowledgeTag>();

    private KnowledgeTag() { }

    public KnowledgeTag(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            throw new ArgumentException("Tag name cannot be empty.", nameof(tagName));

        TagName = tagName;
    }
}
