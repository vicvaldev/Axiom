using Axiom.Application.Handlers;
using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using Axiom.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace Axiom.Application.Tests.Handlers;

/// <summary>
/// Unit tests for the <see cref="ListKnowledgeHandler"/> class.
/// </summary>
public class ListKnowledgeHandlerTests
{
    /// <summary>
    /// Verifies that the handler returns all entries from the repository.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnAllEntries()
    {
        var repository = Substitute.For<IKnowledgeRepository>();
        var entries = new List<KnowledgeEntry>
        {
            new("Title1", "desc", "content", new SystemName("Sys1"), [], "author", KnowledgeType.Documentation, KnowledgeStatusValue.Draft),
            new("Title2", "desc", "content", new SystemName("Sys2"), [], "author", KnowledgeType.Runbook, KnowledgeStatusValue.Published)
        };
        repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(entries);

        var handler = new ListKnowledgeHandler(repository);
        var result = await handler.Handle(new ListKnowledgeQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }
}
