using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Axiom.Domain.ValueObjects;
using MediatR;

namespace Axiom.Application.Handlers;

/// <summary>
/// Handles the <see cref="CreateKnowledgeCommand"/> by creating a new <see cref="KnowledgeEntry"/> and persisting it.
/// </summary>
public class CreateKnowledgeHandler : IRequestHandler<CreateKnowledgeCommand, KnowledgeEntry>
{
    private readonly IKnowledgeRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateKnowledgeHandler"/> class.
    /// </summary>
    /// <param name="repository">The knowledge repository used for persistence.</param>
    public CreateKnowledgeHandler(IKnowledgeRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the command by constructing a <see cref="KnowledgeEntry"/> from the request data and saving it.
    /// </summary>
    /// <param name="request">The create knowledge command.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The newly created <see cref="KnowledgeEntry"/>.</returns>
    public async Task<KnowledgeEntry> Handle(CreateKnowledgeCommand request, CancellationToken cancellationToken)
    {
        var entry = new KnowledgeEntry(
            request.Title,
            request.Description,
            request.Content,
            new SystemName(request.System),
            request.Tags,
            request.Author,
            request.Type,
            request.Status);

        await _repository.SaveAsync(entry, cancellationToken);
        return entry;
    }
}
