using Axiom.Application.Commands;
using Axiom.Application.Handlers;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace Axiom.Application.Tests.Handlers;

public class CreateIssueHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateAndSaveIssue()
    {
        var repository = Substitute.For<IIssueRepository>();
        var handler = new CreateIssueHandler(repository);

        var command = new CreateIssueCommand(
            "Test summary",
            1,
            "Test problem",
            "Analysis text",
            "Resolution text",
            1,
            Guid.NewGuid(),
            "RITM001",
            "INC001");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Summary.Should().Be("Test summary");
        result.Problem.Should().Be("Test problem");
        result.SystemId.Should().Be(1);

        await repository.Received(1).SaveAsync(Arg.Any<Issue>(), Arg.Any<CancellationToken>());
    }
}
