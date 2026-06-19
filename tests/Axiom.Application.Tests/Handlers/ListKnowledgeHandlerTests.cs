using Axiom.Application.Handlers;
using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace Axiom.Application.Tests.Handlers;

public class ListKnowledgeHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnAllEntries()
    {
        var repository = Substitute.For<IKnowledgeRepository>();
        var entries = new List<Knowledge>
        {
            new("Title1", "summary", "content", 1, Guid.NewGuid(), 1, 1),
            new("Title2", "summary", "content", 2, Guid.NewGuid(), 2, 2)
        };
        repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(entries);

        var handler = new ListKnowledgeHandler(repository);
        var result = await handler.Handle(new ListKnowledgeQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }
}
