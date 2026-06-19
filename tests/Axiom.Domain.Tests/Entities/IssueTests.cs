using Axiom.Domain.Entities;
using FluentAssertions;

namespace Axiom.Domain.Tests.Entities;

public class IssueTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreate()
    {
        var issue = new Issue(
            "Test summary",
            1,
            "Test problem",
            1,
            Guid.NewGuid(),
            "Analysis",
            "Resolution",
            "RITM001",
            "INC001");

        issue.IssueId.Should().NotBeEmpty();
        issue.Summary.Should().Be("Test summary");
        issue.SystemId.Should().Be(1);
        issue.Problem.Should().Be("Test problem");
        issue.Analysis.Should().Be("Analysis");
        issue.Resolution.Should().Be("Resolution");
        issue.StateId.Should().Be(1);
        issue.RitmNumber.Should().Be("RITM001");
        issue.IncidentNumber.Should().Be("INC001");
        issue.ResolvedAt.Should().BeNull();
        issue.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_WithEmptySummary_ShouldThrow()
    {
        Action act = () => new Issue("", 1, "problem", 1, Guid.NewGuid());
        act.Should().Throw<ArgumentException>().WithParameterName("summary");
    }

    [Fact]
    public void Constructor_WithEmptyProblem_ShouldThrow()
    {
        Action act = () => new Issue("summary", 1, "", 1, Guid.NewGuid());
        act.Should().Throw<ArgumentException>().WithParameterName("problem");
    }

    [Fact]
    public void Resolve_ShouldSetStateAndTimestamp()
    {
        var issue = new Issue("summary", 1, "problem", 1, Guid.NewGuid());

        issue.Resolve(2, "Fix applied");

        issue.StateId.Should().Be(2);
        issue.Resolution.Should().Be("Fix applied");
        issue.ResolvedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UpdateState_ShouldChangeState()
    {
        var issue = new Issue("summary", 1, "problem", 1, Guid.NewGuid());

        issue.UpdateState(3);

        issue.StateId.Should().Be(3);
    }
}
