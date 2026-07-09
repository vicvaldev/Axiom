using Axiom.Api.Dtos.Requests;
using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;

namespace Axiom.Api.ApiEndpoints;

public static class SystemEndpoints
{
    public static RouteGroupBuilder MapSystems(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IReferenceDataService refs) =>
            Results.Ok(await refs.ListSystemsAsync()));

        group.MapPost("/", async (CreateSystemRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new CreateSystemCommand(req.EAI, req.Name, req.OwnerUserId));
            return Results.Created($"/api/systems/{result.SystemId}", result);
        });

        group.MapPut("/{id:long}", async (long id, UpdateSystemRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateSystemCommand(id, req.EAI, req.Name, req.OwnerUserId));
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapDelete("/{id:long}", async (long id, IMediator mediator) =>
        {
            var ok = await mediator.Send(new DeleteSystemCommand(id));
            return ok ? Results.Ok(new { deleted = true, id }) : Results.NotFound();
        });

        return group;
    }
}
