using Axiom.Application.Commands;
using Axiom.Application.Handlers;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace Axiom.Application.Tests.Handlers;

public class CreateCaseHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateAndSaveCase()
    {
        var repository = Substitute.For<ICaseRepository>();
        var handler = new CreateCaseHandler(repository);

        var command = new CreateCaseCommand(
            "CRM",
            "Login failure",
            "Analysis text",
            "Resolution text",
            "Lessons",
            "RITM001",
            "CHG001");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.System.ToString().Should().Be("CRM");
        result.Problem.Should().Be("Login failure");

        await repository.Received(1).SaveAsync(Arg.Any<CaseRecord>(), Arg.Any<CancellationToken>());
    }
}
