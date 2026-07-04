using Axiom.Application.Commands;
using Axiom.Application.Handlers;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Axiom.Application.Tests.Handlers;

public class CreateTechnicalComponentHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateAndSaveEntry()
    {
        var repository = Substitute.For<ITechnicalComponentRepository>();
        var handler = new CreateTechnicalComponentHandler(repository);

        var command = new CreateTechnicalComponentCommand(
            "Customer DB",
            "crm_db",
            ComponentType.Database,
            TargetEnvironment.PROD,
            Criticality.High,
            1,
            "Production database");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Customer DB");
        result.TechnicalName.Should().Be("crm_db");
        result.ComponentType.Should().Be(ComponentType.Database);
        result.Environment.Should().Be(TargetEnvironment.PROD);
        result.Criticality.Should().Be(Criticality.High);
        result.SystemId.Should().Be(1);

        await repository.Received(1).SaveAsync(Arg.Any<TechnicalComponent>(), Arg.Any<CancellationToken>());
    }
}
