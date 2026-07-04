using Axiom.Application.Interfaces;
using Axiom.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Axiom.Infrastructure;

/// <summary>
/// Proporciona métodos de extensión para registrar los servicios de la capa de infraestructura
/// en el contenedor de inyección de dependencias.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de infraestructura en el contenedor de DI, incluyendo
    /// el <see cref="DbContext"/> de EF Core, los repositorios (scoped) y el almacén
    /// JSON en memoria (singleton).
    /// </summary>
    /// <param name="services">Colección de descriptores de servicios donde se registrarán las dependencias.</param>
    /// <param name="connectionString">Cadena de conexión a la base de datos SQL Server.</param>
    /// <returns>La misma colección <paramref name="services"/> para permitir el encadenamiento.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AxiomDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IKnowledgeRepository, EfKnowledgeRepository>();
        services.AddScoped<IIssueRepository, EfIssueRepository>();
        services.AddScoped<ITagRepository, EfTagRepository>();
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<ISystemRepository, EfSystemRepository>();
        services.AddScoped<IKnowledgeTypeRepository, EfKnowledgeTypeRepository>();
        services.AddScoped<IKnowledgeStateRepository, EfKnowledgeStateRepository>();
        services.AddScoped<IIssueStateRepository, EfIssueStateRepository>();
        services.AddScoped<IKnowledgeTagRepository, EfKnowledgeTagRepository>();
        services.AddScoped<IStartupService, EfStartupService>();
        services.AddScoped<IReferenceDataService, EfReferenceDataService>();
        services.AddSingleton<IJsonStore>(new JsonStore());

        return services;
    }
}
