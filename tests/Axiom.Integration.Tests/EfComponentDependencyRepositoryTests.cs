using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using Axiom.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Integration.Tests;

public class EfComponentDependencyRepositoryTests : IDisposable
{
    private readonly AxiomDbContext _context;
    private readonly IComponentDependencyRepository _repo;
    private readonly User _testUser;
    private readonly AxiomSystem _testSystem;
    private readonly TechnicalComponent _source;
    private readonly TechnicalComponent _target;

    public EfComponentDependencyRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AxiomDbContext>()
            .UseInMemoryDatabase($"axiom_test_{Guid.NewGuid()}")
            .Options;

        _context = new AxiomDbContext(options);
        _repo = new EfComponentDependencyRepository(_context);

        _testUser = new User("test@test.com", "Test User");
        _testSystem = new AxiomSystem("EAI001", "Test System", _testUser.UserId);
        _source = new TechnicalComponent("Source", "src", ComponentType.Table, TargetEnvironment.PROD, Criticality.Medium, _testSystem.SystemId);
        _target = new TechnicalComponent("Target", "tgt", ComponentType.Api, TargetEnvironment.PROD, Criticality.High, _testSystem.SystemId);

        _context.Users.Add(_testUser);
        _context.Systems.Add(_testSystem);
        _context.TechnicalComponents.AddRange(_source, _target);
        _context.SaveChanges();
    }

    [Fact]
    public async Task ShouldRoundTripDependency()
    {
        var entry = new ComponentDependency(
            _source.ComponentId,
            _target.ComponentId,
            DependencyType.Calls,
            Criticality.High,
            DependencyStatus.Active,
            "API calls table");

        await _repo.SaveAsync(entry);

        var loaded = await _repo.GetByIdAsync(entry.DependencyId);

        loaded.Should().NotBeNull();
        loaded!.SourceComponentId.Should().Be(_source.ComponentId);
        loaded.TargetComponentId.Should().Be(_target.ComponentId);
        loaded.DependencyType.Should().Be(DependencyType.Calls);
        loaded.Status.Should().Be(DependencyStatus.Active);
        loaded.Description.Should().Be("API calls table");
    }

    [Fact]
    public async Task ShouldListDependenciesByComponent()
    {
        var dep1 = new ComponentDependency(
            _source.ComponentId, _target.ComponentId, DependencyType.Calls, Criticality.High, DependencyStatus.Active);

        var third = new TechnicalComponent("Third", "t3", ComponentType.Queue, TargetEnvironment.PROD, Criticality.Low, _testSystem.SystemId);
        _context.TechnicalComponents.Add(third);
        await _context.SaveChangesAsync();

        var dep2 = new ComponentDependency(
            _source.ComponentId, third.ComponentId, DependencyType.Triggers, Criticality.Medium, DependencyStatus.Active);

        await _repo.SaveAsync(dep1);
        await _repo.SaveAsync(dep2);

        var results = await _repo.GetByComponentIdAsync(_source.ComponentId);

        results.Should().HaveCount(2);
        results.Should().Contain(d => d.TargetComponentId == _target.ComponentId);
        results.Should().Contain(d => d.TargetComponentId == third.ComponentId);
    }

    [Fact]
    public async Task ShouldListImpactedByComponent()
    {
        var source2 = new TechnicalComponent("Source2", "s2", ComponentType.Api, TargetEnvironment.PROD, Criticality.Low, _testSystem.SystemId);
        _context.TechnicalComponents.Add(source2);
        await _context.SaveChangesAsync();

        var dep = new ComponentDependency(
            source2.ComponentId, _target.ComponentId, DependencyType.Calls, Criticality.High, DependencyStatus.Active);

        await _repo.SaveAsync(dep);

        var results = await _repo.GetImpactedByComponentAsync(_target.ComponentId);

        results.Should().HaveCount(1);
        results.First().SourceComponentId.Should().Be(source2.ComponentId);
    }

    [Fact]
    public async Task ShouldUpdateExistingDependency()
    {
        var entry = new ComponentDependency(
            _source.ComponentId, _target.ComponentId, DependencyType.ReadsFrom, Criticality.Low, DependencyStatus.Active);

        await _repo.SaveAsync(entry);

        entry.Update(DependencyType.WritesTo, Criticality.Critical, DependencyStatus.Deprecated, "Updated");
        await _repo.SaveAsync(entry);

        var loaded = await _repo.GetByIdAsync(entry.DependencyId);
        loaded.Should().NotBeNull();
        loaded!.DependencyType.Should().Be(DependencyType.WritesTo);
        loaded.Status.Should().Be(DependencyStatus.Deprecated);
    }

    [Fact]
    public async Task ShouldDeleteDependency()
    {
        var entry = new ComponentDependency(
            _source.ComponentId, _target.ComponentId, DependencyType.Calls, Criticality.Medium, DependencyStatus.Active);

        await _repo.SaveAsync(entry);
        await _repo.DeleteAsync(entry.DependencyId);

        var loaded = await _repo.GetByIdAsync(entry.DependencyId);
        loaded.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
