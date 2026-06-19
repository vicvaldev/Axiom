using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Axiom.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Integration.Tests;

public class EfIssueRepositoryTests : IDisposable
{
    private readonly AxiomDbContext _context;
    private readonly IIssueRepository _repo;
    private readonly User _testUser;
    private readonly AxiomSystem _testSystem;
    private readonly IssueState _testState;

    public EfIssueRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AxiomDbContext>()
            .UseInMemoryDatabase($"axiom_test_{Guid.NewGuid()}")
            .Options;

        _context = new AxiomDbContext(options);
        _repo = new EfIssueRepository(_context);

        _testUser = new User("test@test.com", "Test User");
        _testSystem = new AxiomSystem("EAI001", "Test System", _testUser.UserId);
        _testState = new IssueState("OPEN", "Open");

        _context.Users.Add(_testUser);
        _context.Systems.Add(_testSystem);
        _context.IssueStates.Add(_testState);
        _context.IssueStates.Add(new IssueState("RES", "Resolved"));
        _context.SaveChanges();
    }

    [Fact]
    public async Task ShouldRoundTripRecord()
    {
        var issue = new Issue(
            "Login issue",
            _testSystem.SystemId,
            "Token expired",
            _testState.StateId,
            _testUser.UserId,
            "Token analysis",
            "Reset password",
            "RITM999",
            "INC888");

        await _repo.SaveAsync(issue);

        var loaded = await _repo.GetByIdAsync(issue.IssueId);

        loaded.Should().NotBeNull();
        loaded!.Summary.Should().Be("Login issue");
        loaded.Problem.Should().Be("Token expired");
        loaded.RitmNumber.Should().Be("RITM999");
        loaded.IncidentNumber.Should().Be("INC888");
    }

    [Fact]
    public async Task ShouldUpdateExistingIssue()
    {
        var issue = new Issue(
            "Original", _testSystem.SystemId, "Problem", _testState.StateId, _testUser.UserId);

        await _repo.SaveAsync(issue);

        issue.Resolve(2, "Fixed");
        await _repo.SaveAsync(issue);

        var loaded = await _repo.GetByIdAsync(issue.IssueId);
        loaded.Should().NotBeNull();
        loaded!.StateId.Should().Be(2);
        loaded.Resolution.Should().Be("Fixed");
        loaded.ResolvedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
