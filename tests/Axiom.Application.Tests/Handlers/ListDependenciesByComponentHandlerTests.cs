using Axiom.Application.Handlers;
using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Axiom.Application.Tests.Handlers;

public class ListDependenciesByComponentHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnDependenciesForComponent()
    {
        var repository = Substitute.For<IComponentDependencyRepository>();
        var handler = new ListDependenciesByComponentHandler(repository);
        var componentId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        var dependencies = new[]
        {
            new ComponentDependency(componentId, targetId, DependencyType.Calls, Criticality.High, DependencyStatus.Active)
        };

        repository.GetByComponentIdAsync(componentId, Arg.Any<CancellationToken>()).Returns(dependencies);

        var result = await handler.Handle(new ListDependenciesByComponentQuery(componentId), CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().SourceComponentId.Should().Be(componentId);
    }
}
