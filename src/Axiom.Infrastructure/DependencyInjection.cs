using Axiom.Application.Interfaces;
using Axiom.Infrastructure.Data;
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
}
