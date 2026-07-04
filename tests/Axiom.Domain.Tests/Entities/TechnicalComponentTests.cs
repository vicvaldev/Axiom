using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using FluentAssertions;

namespace Axiom.Domain.Tests.Entities;

public class TechnicalComponentTests
{
    private readonly Guid _componentId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidData_ShouldCreate()
    {
        var component = new TechnicalComponent(
            "Customer DB",
            "crm_db",
            ComponentType.Database,
            TargetEnvironment.PROD,
            Criticality.High,
            1,
            "Production database",
            _componentId);

        component.ComponentId.Should().Be(_componentId);
        component.Name.Should().Be("Customer DB");
        component.TechnicalName.Should().Be("crm_db");
        component.ComponentType.Should().Be(ComponentType.Database);
        component.Description.Should().Be("Production database");
        component.Environment.Should().Be(TargetEnvironment.PROD);
        component.Criticality.Should().Be(Criticality.High);
        component.SystemId.Should().Be(1);
        component.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        component.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrow()
    {
        Action act = () => new TechnicalComponent("", "tech_name", ComponentType.Api, TargetEnvironment.DEV, Criticality.Low, 1);
        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void Constructor_WithEmptyTechnicalName_ShouldThrow()
    {
        Action act = () => new TechnicalComponent("Name", "", ComponentType.Api, TargetEnvironment.DEV, Criticality.Low, 1);
        act.Should().Throw<ArgumentException>().WithParameterName("technicalName");
    }

    [Fact]
    public void Constructor_WithoutComponentId_ShouldGenerateGuid()
    {
        var component = new TechnicalComponent("Name", "tech", ComponentType.Api, TargetEnvironment.DEV, Criticality.Low, 1);
        component.ComponentId.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WithoutDescription_ShouldDefaultToEmpty()
    {
        var component = new TechnicalComponent("Name", "tech", ComponentType.Api, TargetEnvironment.DEV, Criticality.Low, 1);
        component.Description.Should().BeEmpty();
    }

    [Fact]
    public void Update_ShouldModifyProperties()
    {
        var component = new TechnicalComponent("Old", "old_tech", ComponentType.Api, TargetEnvironment.DEV, Criticality.Low, 1);

        component.Update("New Name", "new_tech", ComponentType.Database, TargetEnvironment.PROD, Criticality.Critical, 2, "New description");

        component.Name.Should().Be("New Name");
        component.TechnicalName.Should().Be("new_tech");
        component.ComponentType.Should().Be(ComponentType.Database);
        component.Description.Should().Be("New description");
        component.Environment.Should().Be(TargetEnvironment.PROD);
        component.Criticality.Should().Be(Criticality.Critical);
        component.SystemId.Should().Be(2);
        component.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_WithEmptyName_ShouldThrow()
    {
        var component = new TechnicalComponent("Name", "tech", ComponentType.Api, TargetEnvironment.DEV, Criticality.Low, 1);

        Action act = () => component.Update("", "tech", ComponentType.Api, TargetEnvironment.DEV, Criticality.Low, 1);
        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void Update_WithEmptyTechnicalName_ShouldThrow()
    {
        var component = new TechnicalComponent("Name", "tech", ComponentType.Api, TargetEnvironment.DEV, Criticality.Low, 1);

        Action act = () => component.Update("Name", "", ComponentType.Api, TargetEnvironment.DEV, Criticality.Low, 1);
        act.Should().Throw<ArgumentException>().WithParameterName("technicalName");
    }

    [Fact]
    public void Constructor_WithUnknownDefaults_ShouldCreate()
    {
        var component = new TechnicalComponent("Name", "tech", ComponentType.Unknown, TargetEnvironment.Unknown, Criticality.Unknown, 1);

        component.ComponentType.Should().Be(ComponentType.Unknown);
        component.Environment.Should().Be(TargetEnvironment.Unknown);
        component.Criticality.Should().Be(Criticality.Unknown);
    }
}
