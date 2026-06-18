using Axiom.Application.Handlers;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Axiom.Application;

/// <summary>
/// Provides extension methods for registering application-layer services into the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers MediatR handlers and FluentValidation validators from the Application assembly.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The same service collection so that calls can be chained.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
