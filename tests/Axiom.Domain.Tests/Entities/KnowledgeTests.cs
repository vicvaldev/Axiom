using Axiom.Domain.Entities;
using FluentAssertions;

namespace Axiom.Domain.Tests.Entities;

public class KnowledgeTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreate()
    {
        var entry = new Knowledge(
            "Test Title",
            "Test Summary",
            "Test Content",
            1,
            Guid.NewGuid(),
            1,
            1);

        entry.KnowledgeId.Should().NotBeEmpty();
        entry.Title.Should().Be("Test Title");
        entry.Summary.Should().Be("Test Summary");
        entry.Content.Should().Be("Test Content");
        entry.SystemId.Should().Be(1);
        entry.KnowledgeTypeId.Should().Be(1);
        entry.KnowledgeStateId.Should().Be(1);
        entry.VersionNumber.Should().Be(1);
        entry.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entry.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_WithEmptyTitle_ShouldThrow()
    {
        Action act = () => new Knowledge("", "summary", "content", 1, Guid.NewGuid(), 1, 1);
        act.Should().Throw<ArgumentException>().WithParameterName("title");
    }

    [Fact]
    public void Constructor_WithEmptyContent_ShouldThrow()
    {
        Action act = () => new Knowledge("Title", "summary", "", 1, Guid.NewGuid(), 1, 1);
        act.Should().Throw<ArgumentException>().WithParameterName("content");
    }

    [Fact]
    public void Update_ShouldModifyProperties()
    {
        var entry = new Knowledge("Old", "summary", "content", 1, Guid.NewGuid(), 1, 1);

        entry.Update("New Title", "New summary", "New content", 2, 2, 2);

        entry.Title.Should().Be("New Title");
        entry.Summary.Should().Be("New summary");
        entry.Content.Should().Be("New content");
        entry.SystemId.Should().Be(2);
        entry.KnowledgeTypeId.Should().Be(2);
        entry.KnowledgeStateId.Should().Be(2);
        entry.VersionNumber.Should().Be(2);
        entry.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_WithEmptyTitle_ShouldThrow()
    {
        var entry = new Knowledge("Title", "summary", "content", 1, Guid.NewGuid(), 1, 1);

        Action act = () => entry.Update("", "summary", "content", 1, 1, 1);
        act.Should().Throw<ArgumentException>().WithParameterName("title");
    }
}
