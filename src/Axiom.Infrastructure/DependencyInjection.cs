using Axiom.Application.Interfaces;
using Axiom.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Axiom.Infrastructure;

public static class DependencyInjection
{
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
