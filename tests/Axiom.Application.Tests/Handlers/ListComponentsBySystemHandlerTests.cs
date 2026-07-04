using Axiom.Application.Handlers;
using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Axiom.Application.Tests.Handlers;

public class ListComponentsBySystemHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnComponentsForSystem()
    {
        var repository = Substitute.For<ITechnicalComponentRepository>();
        var handler = new ListComponentsBySystemHandler(repository);
        var systemId = 1L;

        var components = new[]
        {
            new TechnicalComponent("DB1", "db1", ComponentType.Database, TargetEnvironment.PROD, Criticality.High, systemId),
            new TechnicalComponent("API1", "api1", ComponentType.Api, TargetEnvironment.PROD, Criticality.Medium, systemId)
        };

        repository.GetBySystemIdAsync(systemId, Arg.Any<CancellationToken>()).Returns(components);

        var result = await handler.Handle(new ListComponentsBySystemQuery(systemId), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().Contain(d => d.Name == "DB1");
        result.Should().Contain(d => d.Name == "API1");
    }
}
