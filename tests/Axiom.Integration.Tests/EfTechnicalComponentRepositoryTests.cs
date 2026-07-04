using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using Axiom.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Integration.Tests;

public class EfTechnicalComponentRepositoryTests : IDisposable
{
    private readonly AxiomDbContext _context;
    private readonly ITechnicalComponentRepository _repo;
    private readonly User _testUser;
    private readonly AxiomSystem _testSystem;

    public EfTechnicalComponentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AxiomDbContext>()
            .UseInMemoryDatabase($"axiom_test_{Guid.NewGuid()}")
            .Options;

        _context = new AxiomDbContext(options);
        _repo = new EfTechnicalComponentRepository(_context);

        _testUser = new User("test@test.com", "Test User");
        _testSystem = new AxiomSystem("EAI001", "Test System", _testUser.UserId);

        _context.Users.Add(_testUser);
        _context.Systems.Add(_testSystem);
        _context.SaveChanges();
    }

    [Fact]
    public async Task ShouldRoundTripComponent()
    {
        var entry = new TechnicalComponent(
            "Customer DB",
            "crm_db",
            ComponentType.Database,
            TargetEnvironment.PROD,
            Criticality.High,
            _testSystem.SystemId,
            "Production database");

        await _repo.SaveAsync(entry);

        var loaded = await _repo.GetByIdAsync(entry.ComponentId);

        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("Customer DB");
        loaded.TechnicalName.Should().Be("crm_db");
        loaded.ComponentType.Should().Be(ComponentType.Database);
        loaded.Environment.Should().Be(TargetEnvironment.PROD);
        loaded.Criticality.Should().Be(Criticality.High);
        loaded.SystemId.Should().Be(_testSystem.SystemId);
    }

    [Fact]
    public async Task ShouldUpdateExistingComponent()
    {
        var entry = new TechnicalComponent(
            "Old Name", "old_tech", ComponentType.Api, TargetEnvironment.DEV, Criticality.Low, _testSystem.SystemId);

        await _repo.SaveAsync(entry);

        entry.Update("New Name", "new_tech", ComponentType.Database, TargetEnvironment.PROD, Criticality.Critical, _testSystem.SystemId, "Updated desc");
        await _repo.SaveAsync(entry);

        var loaded = await _repo.GetByIdAsync(entry.ComponentId);
        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("New Name");
        loaded.ComponentType.Should().Be(ComponentType.Database);
        loaded.Criticality.Should().Be(Criticality.Critical);
    }

    [Fact]
    public async Task ShouldListComponentsBySystem()
    {
        var entry1 = new TechnicalComponent("C1", "c1", ComponentType.Api, TargetEnvironment.PROD, Criticality.Low, _testSystem.SystemId);
        var entry2 = new TechnicalComponent("C2", "c2", ComponentType.Database, TargetEnvironment.PROD, Criticality.High, _testSystem.SystemId);

        await _repo.SaveAsync(entry1);
        await _repo.SaveAsync(entry2);

        var results = await _repo.GetBySystemIdAsync(_testSystem.SystemId);

        results.Should().HaveCount(2);
        results.Should().Contain(c => c.Name == "C1");
        results.Should().Contain(c => c.Name == "C2");
    }

    [Fact]
    public async Task ShouldDeleteComponent()
    {
        var entry = new TechnicalComponent("To Delete", "del", ComponentType.File, TargetEnvironment.DEV, Criticality.Medium, _testSystem.SystemId);

        await _repo.SaveAsync(entry);
        await _repo.DeleteAsync(entry.ComponentId);

        var loaded = await _repo.GetByIdAsync(entry.ComponentId);
        loaded.Should().BeNull();
    }

    [Fact]
    public async Task ShouldGetAllComponents()
    {
        var entry1 = new TechnicalComponent("A", "a", ComponentType.Api, TargetEnvironment.PROD, Criticality.Low, _testSystem.SystemId);
        var entry2 = new TechnicalComponent("B", "b", ComponentType.Queue, TargetEnvironment.PROD, Criticality.High, _testSystem.SystemId);

        await _repo.SaveAsync(entry1);
        await _repo.SaveAsync(entry2);

        var all = await _repo.GetAllAsync();

        all.Should().HaveCount(2);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
