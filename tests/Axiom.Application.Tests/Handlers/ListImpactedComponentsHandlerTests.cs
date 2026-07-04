using Axiom.Application.Handlers;
using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Axiom.Application.Tests.Handlers;

public class ListImpactedComponentsHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnImpactedDependencies()
    {
        var repository = Substitute.For<IComponentDependencyRepository>();
        var handler = new ListImpactedComponentsHandler(repository);
        var componentId = Guid.NewGuid();
        var sourceId = Guid.NewGuid();

        var dependencies = new[]
        {
            new ComponentDependency(sourceId, componentId, DependencyType.Calls, Criticality.High, DependencyStatus.Active)
        };

        repository.GetImpactedByComponentAsync(componentId, Arg.Any<CancellationToken>()).Returns(dependencies);

        var result = await handler.Handle(new ListImpactedComponentsQuery(componentId), CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().TargetComponentId.Should().Be(componentId);
    }
}
