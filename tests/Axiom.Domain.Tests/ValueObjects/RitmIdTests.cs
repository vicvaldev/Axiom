using Axiom.Domain.ValueObjects;
using FluentAssertions;

namespace Axiom.Domain.Tests.ValueObjects;

public class RitmIdTests
{
    [Fact]
    public void Constructor_WithValidValue_ShouldCreate()
    {
        var id = new RitmId("RITM001");
        id.Value.Should().Be("RITM001");
    }

    [Fact]
    public void Constructor_WithEmptyValue_ShouldThrow()
    {
        Action act = () => new RitmId("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        var id1 = new RitmId("RITM001");
        var id2 = new RitmId("RITM001");

        id1.Should().Be(id2);
    }
}
