using Axiom.Application.Commands;
using Axiom.Application.Handlers;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace Axiom.Application.Tests.Handlers;

public class CreateKnowledgeHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateAndSaveEntry()
    {
        var repository = Substitute.For<IKnowledgeRepository>();
        var tagRepository = Substitute.For<ITagRepository>();
        var handler = new CreateKnowledgeHandler(repository, tagRepository);

        var command = new CreateKnowledgeCommand(
            "Test Title",
            "Test Summary",
            "Test Content",
            1,
            Guid.NewGuid(),
            1,
            1,
            null,
            []);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("Test Title");
        result.Content.Should().Be("Test Content");
        result.SystemId.Should().Be(1);

        await repository.Received(1).SaveAsync(Arg.Any<Knowledge>(), Arg.Any<CancellationToken>());
    }
}
