using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using Axiom.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace Axiom.Integration.Tests;

public class SqlServerTests : IDisposable
{
    private readonly AxiomDbContext _context;
    private readonly ITechnicalComponentRepository _componentRepo;
    private readonly ISystemComponentRepository _systemComponentRepo;
    private readonly IComponentDependencyRepository _dependencyRepo;
    private readonly IDependencyTraceEventRepository _traceRepo;
    private readonly User _testUser;
    private readonly AxiomSystem _testSystem;
    private readonly IDbContextTransaction _tx;

    public SqlServerTests()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var cs = configuration.GetConnectionString("Axiom")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión 'Axiom' en appsettings.json. " +
                "Agrega la sección ConnectionStrings:Axiom.");

        var options = new DbContextOptionsBuilder<AxiomDbContext>()
            .UseSqlServer(cs)
            .Options;

        _context = new AxiomDbContext(options);
        _tx = _context.Database.BeginTransaction();

        _componentRepo = new EfTechnicalComponentRepository(_context);
        _systemComponentRepo = new EfSystemComponentRepository(_context);
        _dependencyRepo = new EfComponentDependencyRepository(_context);
        _traceRepo = new EfDependencyTraceEventRepository(_context);

        _testUser = new User("sql.test@axiom.local", "SQL Test User");
        _testSystem = new AxiomSystem("EAI_SQL", "SQL Test System", _testUser.UserId);

        _context.Users.Add(_testUser);
        _context.Systems.Add(_testSystem);
        _context.SaveChanges();
    }

    [Fact]
    public async Task ShouldCreateAndReadComponent()
    {
        var component = new TechnicalComponent(
            "SQL Database", "sql_db", ComponentType.Database, TargetEnvironment.PROD, Criticality.High, _testSystem.SystemId, "SQL Server DB");

        await _componentRepo.SaveAsync(component);

        var loaded = await _componentRepo.GetByIdAsync(component.ComponentId);

        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("SQL Database");
        loaded.TechnicalName.Should().Be("sql_db");
        loaded.ComponentType.Should().Be(ComponentType.Database);
        loaded.Environment.Should().Be(TargetEnvironment.PROD);
        loaded.Criticality.Should().Be(Criticality.High);
        loaded.SystemId.Should().Be(_testSystem.SystemId);
    }

    [Fact]
    public async Task ShouldUpdateExistingComponent()
    {
        var component = new TechnicalComponent(
            "Old", "old_name", ComponentType.Api, TargetEnvironment.DEV, Criticality.Low, _testSystem.SystemId);

        await _componentRepo.SaveAsync(component);

        component.Update("Updated", "new_name", ComponentType.Queue, TargetEnvironment.PROD, Criticality.Critical, _testSystem.SystemId, "New desc");
        await _componentRepo.SaveAsync(component);

        var loaded = await _componentRepo.GetByIdAsync(component.ComponentId);
        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("Updated");
        loaded.ComponentType.Should().Be(ComponentType.Queue);
        loaded.Criticality.Should().Be(Criticality.Critical);
    }

    [Fact]
    public async Task ShouldListComponentsBySystem()
    {
        var c1 = new TechnicalComponent("C1", "c1", ComponentType.Database, TargetEnvironment.PROD, Criticality.High, _testSystem.SystemId);
        var c2 = new TechnicalComponent("C2", "c2", ComponentType.Api, TargetEnvironment.PROD, Criticality.Medium, _testSystem.SystemId);

        await _componentRepo.SaveAsync(c1);
        await _componentRepo.SaveAsync(c2);

        var results = await _componentRepo.GetBySystemIdAsync(_testSystem.SystemId);

        results.Should().HaveCount(2);
        results.Should().Contain(c => c.TechnicalName == "c1");
        results.Should().Contain(c => c.TechnicalName == "c2");
    }

    [Fact]
    public async Task ShouldDeleteComponent()
    {
        var component = new TechnicalComponent(
            "Delete Me", "del", ComponentType.File, TargetEnvironment.DEV, Criticality.Medium, _testSystem.SystemId);

        await _componentRepo.SaveAsync(component);
        await _componentRepo.DeleteAsync(component.ComponentId);

        var loaded = await _componentRepo.GetByIdAsync(component.ComponentId);
        loaded.Should().BeNull();
    }

    [Fact]
    public async Task ShouldCreateSystemComponentAssociation()
    {
        var component = new TechnicalComponent(
            "Assoc", "assoc", ComponentType.Service, TargetEnvironment.PROD, Criticality.Low, _testSystem.SystemId);
        await _componentRepo.SaveAsync(component);

        var sc = new SystemComponent(_testSystem.SystemId, component.ComponentId, isOwner: false);
        await _systemComponentRepo.SaveAsync(sc);

        var fromDb = await _context.SystemComponents
            .FirstOrDefaultAsync(s => s.SystemId == _testSystem.SystemId && s.ComponentId == component.ComponentId);

        fromDb.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldCreateAndReadDependency()
    {
        var source = new TechnicalComponent("Source", "src", ComponentType.Table, TargetEnvironment.PROD, Criticality.High, _testSystem.SystemId);
        var target = new TechnicalComponent("Target", "tgt", ComponentType.Api, TargetEnvironment.PROD, Criticality.Medium, _testSystem.SystemId);
        _context.TechnicalComponents.AddRange(source, target);
        await _context.SaveChangesAsync();

        var dep = new ComponentDependency(
            source.ComponentId, target.ComponentId, DependencyType.Calls, Criticality.High, DependencyStatus.Active, "Source calls target");

        await _dependencyRepo.SaveAsync(dep);

        var loaded = await _dependencyRepo.GetByIdAsync(dep.DependencyId);

        loaded.Should().NotBeNull();
        loaded!.SourceComponentId.Should().Be(source.ComponentId);
        loaded.TargetComponentId.Should().Be(target.ComponentId);
        loaded.DependencyType.Should().Be(DependencyType.Calls);
        loaded.Status.Should().Be(DependencyStatus.Active);
    }

    [Fact]
    public async Task ShouldListDependenciesByComponent()
    {
        var source = new TechnicalComponent("S", "s", ComponentType.Api, TargetEnvironment.PROD, Criticality.Low, _testSystem.SystemId);
        var t1 = new TechnicalComponent("T1", "t1", ComponentType.Database, TargetEnvironment.PROD, Criticality.High, _testSystem.SystemId);
        var t2 = new TechnicalComponent("T2", "t2", ComponentType.Queue, TargetEnvironment.PROD, Criticality.Medium, _testSystem.SystemId);
        _context.TechnicalComponents.AddRange(source, t1, t2);
        await _context.SaveChangesAsync();

        var d1 = new ComponentDependency(source.ComponentId, t1.ComponentId, DependencyType.Calls, Criticality.High, DependencyStatus.Active);
        var d2 = new ComponentDependency(source.ComponentId, t2.ComponentId, DependencyType.Triggers, Criticality.Medium, DependencyStatus.Active);
        await _dependencyRepo.SaveAsync(d1);
        await _dependencyRepo.SaveAsync(d2);

        var results = await _dependencyRepo.GetByComponentIdAsync(source.ComponentId);

        results.Should().HaveCount(2);
        results.Should().Contain(d => d.TargetComponentId == t1.ComponentId);
        results.Should().Contain(d => d.TargetComponentId == t2.ComponentId);
    }

    [Fact]
    public async Task ShouldListImpactedByComponent()
    {
        var target = new TechnicalComponent("Target", "tgt", ComponentType.Api, TargetEnvironment.PROD, Criticality.High, _testSystem.SystemId);
        var s1 = new TechnicalComponent("S1", "s1", ComponentType.Service, TargetEnvironment.PROD, Criticality.Critical, _testSystem.SystemId);
        _context.TechnicalComponents.AddRange(target, s1);
        await _context.SaveChangesAsync();

        var dep = new ComponentDependency(s1.ComponentId, target.ComponentId, DependencyType.Calls, Criticality.Critical, DependencyStatus.Active);
        await _dependencyRepo.SaveAsync(dep);

        var results = await _dependencyRepo.GetImpactedByComponentAsync(target.ComponentId);

        results.Should().HaveCount(1);
        results.First().SourceComponentId.Should().Be(s1.ComponentId);
    }

    [Fact]
    public async Task ShouldUpdateDependency()
    {
        var source = new TechnicalComponent("S", "s", ComponentType.Api, TargetEnvironment.PROD, Criticality.Low, _testSystem.SystemId);
        var target = new TechnicalComponent("T", "t", ComponentType.File, TargetEnvironment.PROD, Criticality.Low, _testSystem.SystemId);
        _context.TechnicalComponents.AddRange(source, target);
        await _context.SaveChangesAsync();

        var dep = new ComponentDependency(source.ComponentId, target.ComponentId, DependencyType.ReadsFrom, Criticality.Medium, DependencyStatus.Active);
        await _dependencyRepo.SaveAsync(dep);

        dep.Update(DependencyType.WritesTo, Criticality.Critical, DependencyStatus.Deprecated, "Updated dep");
        await _dependencyRepo.SaveAsync(dep);

        var loaded = await _dependencyRepo.GetByIdAsync(dep.DependencyId);
        loaded!.DependencyType.Should().Be(DependencyType.WritesTo);
        loaded.Status.Should().Be(DependencyStatus.Deprecated);
    }

    [Fact]
    public async Task ShouldDeleteDependency()
    {
        var source = new TechnicalComponent("S", "s", ComponentType.Api, TargetEnvironment.PROD, Criticality.Low, _testSystem.SystemId);
        var target = new TechnicalComponent("T", "t", ComponentType.File, TargetEnvironment.PROD, Criticality.Low, _testSystem.SystemId);
        _context.TechnicalComponents.AddRange(source, target);
        await _context.SaveChangesAsync();

        var dep = new ComponentDependency(source.ComponentId, target.ComponentId, DependencyType.Calls, Criticality.Medium, DependencyStatus.Active);
        await _dependencyRepo.SaveAsync(dep);
        await _dependencyRepo.DeleteAsync(dep.DependencyId);

        var loaded = await _dependencyRepo.GetByIdAsync(dep.DependencyId);
        loaded.Should().BeNull();
    }

    [Fact]
    public async Task ShouldAddAndListTraceEvents()
    {
        var source = new TechnicalComponent("S", "s", ComponentType.Api, TargetEnvironment.PROD, Criticality.Low, _testSystem.SystemId);
        var target = new TechnicalComponent("T", "t", ComponentType.Database, TargetEnvironment.PROD, Criticality.High, _testSystem.SystemId);
        _context.TechnicalComponents.AddRange(source, target);
        await _context.SaveChangesAsync();

        var dep = new ComponentDependency(source.ComponentId, target.ComponentId, DependencyType.Calls, Criticality.High, DependencyStatus.Active);
        await _dependencyRepo.SaveAsync(dep);

        var ev1 = new DependencyTraceEvent(dep.DependencyId, DependencyTraceEventType.Created, "Dependency created");
        var ev2 = new DependencyTraceEvent(dep.DependencyId, DependencyTraceEventType.Validated, "Validated in PROD", createdByUserId: Guid.NewGuid());

        await _traceRepo.AddAsync(ev1);
        await _traceRepo.AddAsync(ev2);

        var events = await _traceRepo.GetByDependencyIdAsync(dep.DependencyId);

        events.Should().HaveCount(2);

        var list = events.ToList();
        list.Should().Contain(e => e.EventType == DependencyTraceEventType.Created);
        list.Should().Contain(e => e.EventType == DependencyTraceEventType.Validated);
        list[0].CreatedAt.Should().BeOnOrAfter(list[1].CreatedAt);
    }

    [Fact]
    public async Task ShouldReturnEmptyTraceEventsForUnknownDependency()
    {
        var results = await _traceRepo.GetByDependencyIdAsync(Guid.NewGuid());
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldCascadeDeleteTraceEvents()
    {
        var source = new TechnicalComponent("S", "s", ComponentType.Api, TargetEnvironment.PROD, Criticality.Low, _testSystem.SystemId);
        var target = new TechnicalComponent("T", "t", ComponentType.Database, TargetEnvironment.PROD, Criticality.High, _testSystem.SystemId);
        _context.TechnicalComponents.AddRange(source, target);
        await _context.SaveChangesAsync();

        var dep = new ComponentDependency(source.ComponentId, target.ComponentId, DependencyType.Calls, Criticality.High, DependencyStatus.Active);
        await _dependencyRepo.SaveAsync(dep);

        var ev = new DependencyTraceEvent(dep.DependencyId, DependencyTraceEventType.Created, "Created");
        await _traceRepo.AddAsync(ev);

        await _dependencyRepo.DeleteAsync(dep.DependencyId);

        var events = await _traceRepo.GetByDependencyIdAsync(dep.DependencyId);
        events.Should().BeEmpty();
    }

    public void Dispose()
    {
        _tx.Rollback();
        _tx.Dispose();
        _context.Dispose();
    }
}
