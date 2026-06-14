using Axiom.Domain.ValueObjects;
using FluentAssertions;

namespace Axiom.Domain.Tests.ValueObjects;

public class SystemNameTests
{
    [Fact]
    public void Constructor_WithValidValue_ShouldCreate()
    {
        var name = new SystemName("MySystem");
        name.Value.Should().Be("MySystem");
        name.ToString().Should().Be("MySystem");
    }

    [Fact]
    public void Constructor_WithEmptyValue_ShouldThrow()
    {
        Action act = () => new SystemName("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithWhitespaceValue_ShouldThrow()
    {
        Action act = () => new SystemName("   ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        var name1 = new SystemName("Sys");
        var name2 = new SystemName("Sys");

        name1.Should().Be(name2);
    }

    [Fact]
    public void Equality_DifferentValue_ShouldNotBeEqual()
    {
        var name1 = new SystemName("Sys1");
        var name2 = new SystemName("Sys2");

        name1.Should().NotBe(name2);
    }
}
