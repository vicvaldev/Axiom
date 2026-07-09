using Axiom.Api.Dtos.Requests;
using Axiom.Application.Commands;
using Axiom.Application.Queries;
using Axiom.Domain.Enums;
using MediatR;

namespace Axiom.Api.ApiEndpoints;

public static class ComponentEndpoints
{
    public static RouteGroupBuilder MapComponents(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateComponentRequest req, IMediator mediator) =>
        {
            if (!Enum.TryParse<ComponentType>(req.ComponentType, true, out var componentType))
                return Results.BadRequest($"Invalid component type. Valid values: {string.Join(", ", Enum.GetNames<ComponentType>())}");

            if (!Enum.TryParse<TargetEnvironment>(req.Environment, true, out var env))
                return Results.BadRequest($"Invalid environment. Valid values: {string.Join(", ", Enum.GetNames<TargetEnvironment>())}");

            if (!Enum.TryParse<Criticality>(req.Criticality, true, out var criticality))
                return Results.BadRequest($"Invalid criticality. Valid values: {string.Join(", ", Enum.GetNames<Criticality>())}");

            var cmd = new CreateTechnicalComponentCommand(
                req.Name, req.TechnicalName, componentType, env, criticality, req.SystemId, req.Description);
            var result = await mediator.Send(cmd);
            return Results.Created($"/api/components/{result.ComponentId}", result);
        });

        group.MapGet("/", async (long systemId, IMediator mediator) =>
            Results.Ok(await mediator.Send(new ListComponentsBySystemQuery(systemId))));

        group.MapGet("/{id}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetComponentByIdQuery(id));
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        return group;
    }
}
