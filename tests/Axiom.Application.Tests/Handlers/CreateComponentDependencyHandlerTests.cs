using Axiom.Application.Commands;
using Axiom.Application.Handlers;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Axiom.Application.Tests.Handlers;

public class CreateComponentDependencyHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateAndSaveDependency()
    {
        var repository = Substitute.For<IComponentDependencyRepository>();
        var componentRepository = Substitute.For<ITechnicalComponentRepository>();
        var sourceId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        componentRepository.GetByIdAsync(sourceId, Arg.Any<CancellationToken>())
            .Returns(new TechnicalComponent("Source", "src", ComponentType.Table, TargetEnvironment.PROD, Criticality.Medium, 1, componentId: sourceId));
        componentRepository.GetByIdAsync(targetId, Arg.Any<CancellationToken>())
            .Returns(new TechnicalComponent("Target", "tgt", ComponentType.Api, TargetEnvironment.PROD, Criticality.High, 1, componentId: targetId));

        var handler = new CreateComponentDependencyHandler(repository, componentRepository);

        var command = new CreateComponentDependencyCommand(
            sourceId,
            targetId,
            DependencyType.Calls,
            Criticality.High,
            DependencyStatus.Active,
            "API calls database");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.SourceComponentId.Should().Be(sourceId);
        result.TargetComponentId.Should().Be(targetId);
        result.DependencyType.Should().Be(DependencyType.Calls);
        result.Status.Should().Be(DependencyStatus.Active);

        await repository.Received(1).SaveAsync(Arg.Any<ComponentDependency>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidSource_ShouldThrow()
    {
        var repository = Substitute.For<IComponentDependencyRepository>();
        var componentRepository = Substitute.For<ITechnicalComponentRepository>();
        var handler = new CreateComponentDependencyHandler(repository, componentRepository);

        var command = new CreateComponentDependencyCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DependencyType.Calls,
            Criticality.Medium,
            DependencyStatus.Active);

        await FluentActions.Awaiting(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>();
    }
}
