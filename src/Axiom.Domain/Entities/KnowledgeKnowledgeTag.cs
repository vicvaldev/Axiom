namespace Axiom.Domain.Entities;

public class KnowledgeKnowledgeTag
{
    public Guid KnowledgeId { get; private set; }
    public long KnowledgeTagId { get; private set; }

    public Knowledge? Knowledge { get; private set; }
    public KnowledgeTag? Tag { get; private set; }

    private KnowledgeKnowledgeTag() { }

    public KnowledgeKnowledgeTag(Guid knowledgeId, long knowledgeTagId)
    {
        KnowledgeId = knowledgeId;
        KnowledgeTagId = knowledgeTagId;
    }
}
