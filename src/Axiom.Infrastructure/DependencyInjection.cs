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

        return services;
    }
}
