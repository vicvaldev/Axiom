using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using Axiom.Domain.ValueObjects;
using FluentAssertions;

namespace Axiom.Domain.Tests.Entities;

public class KnowledgeEntryTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateEntry()
    {
        var entry = new KnowledgeEntry(
            "Test Title",
            "Test Description",
            "Test Content",
            new SystemName("TestSys"),
            ["tag1", "tag2"],
            "TestAuthor",
            KnowledgeType.Runbook,
            KnowledgeStatusValue.Draft);

        entry.Id.Should().NotBeEmpty();
        entry.Title.Should().Be("Test Title");
        entry.Content.Should().Be("Test Content");
        entry.System.ToString().Should().Be("TestSys");
        entry.Tags.Should().Contain(["tag1", "tag2"]);
        entry.Author.Should().Be("TestAuthor");
        entry.Type.Should().Be(KnowledgeType.Runbook);
        entry.Status.Should().Be(KnowledgeStatusValue.Draft);
        entry.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entry.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_WithEmptyTitle_ShouldThrow()
    {
        Action act = () => new KnowledgeEntry("", "desc", "content", new SystemName("Sys"), [], "author", KnowledgeType.Documentation, KnowledgeStatusValue.Draft);

        act.Should().Throw<ArgumentException>().WithParameterName("title");
    }

    [Fact]
    public void Constructor_WithEmptyContent_ShouldThrow()
    {
        Action act = () => new KnowledgeEntry("Title", "desc", "", new SystemName("Sys"), [], "author", KnowledgeType.Documentation, KnowledgeStatusValue.Draft);

        act.Should().Throw<ArgumentException>().WithParameterName("content");
    }

    [Fact]
    public void Constructor_WithNullTags_ShouldDefaultToEmpty()
    {
        var entry = new KnowledgeEntry("Title", "desc", "content", new SystemName("Sys"), null!, "author", KnowledgeType.Documentation, KnowledgeStatusValue.Draft);

        entry.Tags.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullDescription_ShouldDefaultToEmpty()
    {
        var entry = new KnowledgeEntry("Title", null!, "content", new SystemName("Sys"), [], "author", KnowledgeType.Documentation, KnowledgeStatusValue.Draft);

        entry.Description.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullAuthor_ShouldDefaultToUnknown()
    {
        var entry = new KnowledgeEntry("Title", "desc", "content", new SystemName("Sys"), [], null!, KnowledgeType.Documentation, KnowledgeStatusValue.Draft);

        entry.Author.Should().Be("unknown");
    }

    [Fact]
    public void Update_ShouldModifyProperties()
    {
        var entry = new KnowledgeEntry("Old Title", "desc", "content", new SystemName("Sys"), [], "author", KnowledgeType.Documentation, KnowledgeStatusValue.Draft);

        entry.Update("New Title", "New desc", "New content", new SystemName("NewSys"), ["new"], KnowledgeType.Troubleshooting, KnowledgeStatusValue.Published);

        entry.Title.Should().Be("New Title");
        entry.Content.Should().Be("New content");
        entry.System.ToString().Should().Be("NewSys");
        entry.Type.Should().Be(KnowledgeType.Troubleshooting);
        entry.Status.Should().Be(KnowledgeStatusValue.Published);
        entry.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_WithEmptyTitle_ShouldThrow()
    {
        var entry = new KnowledgeEntry("Title", "desc", "content", new SystemName("Sys"), [], "author", KnowledgeType.Documentation, KnowledgeStatusValue.Draft);

        Action act = () => entry.Update("", "desc", "content", new SystemName("Sys"), [], KnowledgeType.Documentation, KnowledgeStatusValue.Draft);

        act.Should().Throw<ArgumentException>().WithParameterName("title");
    }
}
