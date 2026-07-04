using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using FluentAssertions;

namespace Axiom.Domain.Tests.Entities;

public class ComponentDependencyTests
{
    private readonly Guid _sourceId = Guid.NewGuid();
    private readonly Guid _targetId = Guid.NewGuid();
    private readonly Guid _dependencyId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidData_ShouldCreate()
    {
        var dep = new ComponentDependency(
            _sourceId,
            _targetId,
            DependencyType.Calls,
            Criticality.High,
            DependencyStatus.Active,
            "API calls database",
            _dependencyId);

        dep.DependencyId.Should().Be(_dependencyId);
        dep.SourceComponentId.Should().Be(_sourceId);
        dep.TargetComponentId.Should().Be(_targetId);
        dep.DependencyType.Should().Be(DependencyType.Calls);
        dep.Criticality.Should().Be(Criticality.High);
        dep.Status.Should().Be(DependencyStatus.Active);
        dep.Description.Should().Be("API calls database");
        dep.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        dep.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_WithoutDependencyId_ShouldGenerateGuid()
    {
        var dep = new ComponentDependency(_sourceId, _targetId, DependencyType.ReadsFrom, Criticality.Low, DependencyStatus.Active);
        dep.DependencyId.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WithoutDescription_ShouldDefaultToEmpty()
    {
        var dep = new ComponentDependency(_sourceId, _targetId, DependencyType.ReadsFrom, Criticality.Low, DependencyStatus.Active);
        dep.Description.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithSameSourceAndTarget_ShouldThrow()
    {
        var id = Guid.NewGuid();
        Action act = () => new ComponentDependency(id, id, DependencyType.Calls, Criticality.High, DependencyStatus.Active);
        act.Should().Throw<ArgumentException>().WithParameterName("sourceComponentId");
    }

    [Fact]
    public void Update_ShouldModifyProperties()
    {
        var dep = new ComponentDependency(_sourceId, _targetId, DependencyType.ReadsFrom, Criticality.Low, DependencyStatus.Active);

        dep.Update(DependencyType.WritesTo, Criticality.Critical, DependencyStatus.Deprecated, "Updated description");

        dep.DependencyType.Should().Be(DependencyType.WritesTo);
        dep.Criticality.Should().Be(Criticality.Critical);
        dep.Status.Should().Be(DependencyStatus.Deprecated);
        dep.Description.Should().Be("Updated description");
        dep.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_WithNullDescription_ShouldDefaultToEmpty()
    {
        var dep = new ComponentDependency(_sourceId, _targetId, DependencyType.Calls, Criticality.Medium, DependencyStatus.Active);

        dep.Update(DependencyType.Exports, Criticality.Unknown, DependencyStatus.Disabled, null);

        dep.Description.Should().BeEmpty();
        dep.Status.Should().Be(DependencyStatus.Disabled);
    }
}
