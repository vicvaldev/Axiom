using Axiom.Application.Interfaces;
using Axiom.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Axiom.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IKnowledgeRepository, JsonKnowledgeRepository>();
        services.AddScoped<ICaseRepository, JsonCaseRepository>();

        return services;
    }
}
