using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Persistence;

public class EfTagRepository : ITagRepository
{
    private readonly AxiomDbContext _context;

    public EfTagRepository(AxiomDbContext context)
    {
        _context = context;
    }

    public async Task<KnowledgeTag> FindOrCreateAsync(string tagName, CancellationToken cancellationToken = default)
    {
        var tag = await _context.KnowledgeTags
            .FirstOrDefaultAsync(t => t.TagName == tagName, cancellationToken);

        if (tag is not null)
            return tag;

        tag = new KnowledgeTag(tagName);
        _context.KnowledgeTags.Add(tag);
        await _context.SaveChangesAsync(cancellationToken);

        return tag;
    }
}
