using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using Axiom.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Integration.Tests;

public class EfDependencyTraceEventRepositoryTests : IDisposable
{
    private readonly AxiomDbContext _context;
    private readonly IDependencyTraceEventRepository _repo;
    private readonly IComponentDependencyRepository _depRepo;
    private readonly ComponentDependency _testDependency;

    public EfDependencyTraceEventRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AxiomDbContext>()
            .UseInMemoryDatabase($"axiom_test_{Guid.NewGuid()}")
            .Options;

        _context = new AxiomDbContext(options);
        _repo = new EfDependencyTraceEventRepository(_context);
        _depRepo = new EfComponentDependencyRepository(_context);

        var user = new User("test@test.com", "Test User");
        var system = new AxiomSystem("EAI001", "Test System", user.UserId);
        var source = new TechnicalComponent("Source", "src", ComponentType.Table, TargetEnvironment.PROD, Criticality.Medium, system.SystemId);
        var target = new TechnicalComponent("Target", "tgt", ComponentType.Api, TargetEnvironment.PROD, Criticality.High, system.SystemId);

        _context.Users.Add(user);
        _context.Systems.Add(system);
        _context.TechnicalComponents.AddRange(source, target);
        _context.SaveChanges();

        _testDependency = new ComponentDependency(
            source.ComponentId, target.ComponentId, DependencyType.Calls, Criticality.High, DependencyStatus.Active);

        _depRepo.SaveAsync(_testDependency).Wait();
    }

    [Fact]
    public async Task ShouldAddTraceEvent()
    {
        var ev = new DependencyTraceEvent(
            _testDependency.DependencyId,
            DependencyTraceEventType.Created,
            "Dependency created",
            createdByUserId: Guid.NewGuid());

        await _repo.AddAsync(ev);

        var loaded = await _repo.GetByDependencyIdAsync(_testDependency.DependencyId);

        loaded.Should().HaveCount(1);
        loaded.First().TraceEventId.Should().Be(ev.TraceEventId);
        loaded.First().EventType.Should().Be(DependencyTraceEventType.Created);
        loaded.First().Description.Should().Be("Dependency created");
    }

    [Fact]
    public async Task ShouldListMultipleEventsOrderedByDescending()
    {
        var ev1 = new DependencyTraceEvent(
            _testDependency.DependencyId, DependencyTraceEventType.Created, "Created");

        var ev2 = new DependencyTraceEvent(
            _testDependency.DependencyId, DependencyTraceEventType.Validated, "Validated successfully");

        await _repo.AddAsync(ev1);
        await _repo.AddAsync(ev2);

        var loaded = await _repo.GetByDependencyIdAsync(_testDependency.DependencyId);

        loaded.Should().HaveCount(2);

        var list = loaded.ToList();
        list[0].CreatedAt.Should().BeOnOrAfter(list[1].CreatedAt);
    }

    [Fact]
    public async Task ShouldReturnEmptyForUnknownDependency()
    {
        var results = await _repo.GetByDependencyIdAsync(Guid.NewGuid());
        results.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
