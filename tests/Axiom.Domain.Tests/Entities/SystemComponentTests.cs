using Axiom.Domain.Entities;
using FluentAssertions;

namespace Axiom.Domain.Tests.Entities;

public class SystemComponentTests
{
    private readonly Guid _componentId = Guid.NewGuid();
    private readonly Guid _systemComponentId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidData_ShouldCreate()
    {
        var sc = new SystemComponent(1, _componentId, true, "Primary connection", _systemComponentId);

        sc.SystemComponentId.Should().Be(_systemComponentId);
        sc.SystemId.Should().Be(1);
        sc.ComponentId.Should().Be(_componentId);
        sc.IsOwner.Should().BeTrue();
        sc.RoleDescription.Should().Be("Primary connection");
        sc.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        sc.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_WithoutSystemComponentId_ShouldGenerateGuid()
    {
        var sc = new SystemComponent(1, _componentId, false);
        sc.SystemComponentId.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WithoutRoleDescription_ShouldDefaultToEmpty()
    {
        var sc = new SystemComponent(1, _componentId, false);
        sc.RoleDescription.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithIsOwnerFalse_ShouldCreate()
    {
        var sc = new SystemComponent(1, _componentId, false);
        sc.IsOwner.Should().BeFalse();
    }

    [Fact]
    public void Update_ShouldModifyProperties()
    {
        var sc = new SystemComponent(1, _componentId, false, "Old role");

        sc.Update("New role", true);

        sc.RoleDescription.Should().Be("New role");
        sc.IsOwner.Should().BeTrue();
        sc.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_WithNullRoleDescription_ShouldDefaultToEmpty()
    {
        var sc = new SystemComponent(1, _componentId, true, "Old role");

        sc.Update(null, false);

        sc.RoleDescription.Should().BeEmpty();
        sc.IsOwner.Should().BeFalse();
    }
}
