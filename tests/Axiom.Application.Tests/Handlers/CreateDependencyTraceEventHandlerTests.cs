using Axiom.Application.Commands;
using Axiom.Application.Handlers;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Axiom.Application.Tests.Handlers;

public class CreateDependencyTraceEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateAndAddTraceEvent()
    {
        var repository = Substitute.For<IDependencyTraceEventRepository>();
        var handler = new CreateDependencyTraceEventHandler(repository);

        var command = new CreateDependencyTraceEventCommand(
            Guid.NewGuid(),
            DependencyTraceEventType.Created,
            "Dependency created",
            CreatedByUserId: Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.EventType.Should().Be(DependencyTraceEventType.Created);
        result.Description.Should().Be("Dependency created");

        await repository.Received(1).AddAsync(Arg.Any<DependencyTraceEvent>(), Arg.Any<CancellationToken>());
    }
}
