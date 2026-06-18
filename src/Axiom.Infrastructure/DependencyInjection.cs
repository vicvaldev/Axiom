using Axiom.Application.Interfaces;
using Axiom.Infrastructure.Data;
using Axiom.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Axiom.Infrastructure;

/// <summary>
/// Provides extension methods for registering infrastructure-layer services into the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers JSON-based repository implementations for knowledge entries and case records.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The same service collection so that calls can be chained.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IKnowledgeRepository, JsonKnowledgeRepository>();
        services.AddScoped<ICaseRepository, JsonCaseRepository>();

        return services;
    }

    /// <summary>
    /// Registers EF Core SQL Server and repository implementations for knowledge entries and case records.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <returns>The same service collection so that calls can be chained.</returns>
    public static IServiceCollection AddInfrastructureEF(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AxiomDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IKnowledgeRepository, EfKnowledgeRepository>();
        services.AddScoped<ICaseRepository, EfCaseRepository>();

        return services;
    }
}
