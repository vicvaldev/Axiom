using Axiom.Application.Commands;
using Axiom.Application.Handlers;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using Axiom.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace Axiom.Application.Tests.Handlers;

public class CreateKnowledgeHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateAndSaveEntry()
    {
        var repository = Substitute.For<IKnowledgeRepository>();
        var handler = new CreateKnowledgeHandler(repository);

        var command = new CreateKnowledgeCommand(
            "Test Title",
            "Test Description",
            "Test Content",
            "TestSys",
            ["tag1"],
            "TestAuthor",
            KnowledgeType.Documentation,
            KnowledgeStatusValue.Draft);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("Test Title");
        result.Content.Should().Be("Test Content");
        result.System.ToString().Should().Be("TestSys");

        await repository.Received(1).SaveAsync(Arg.Any<KnowledgeEntry>(), Arg.Any<CancellationToken>());
    }
}
