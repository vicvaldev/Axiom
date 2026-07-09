using Axiom.Api.Dtos.Requests;
using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using MediatR;

namespace Axiom.Api.ApiEndpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUsers(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IReferenceDataService refs) =>
            Results.Ok(await refs.ListUsersAsync()));

        group.MapPost("/", async (CreateUserRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new CreateUserCommand(req.Email, req.Name));
            return Results.Created($"/api/users/{result.UserId}", result);
        });

        group.MapPut("/{id}", async (Guid id, UpdateUserRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateUserCommand(id, req.Email, req.Name));
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapDelete("/{id}", async (Guid id, IMediator mediator) =>
        {
            var ok = await mediator.Send(new DeleteUserCommand(id));
            return ok ? Results.Ok(new { deleted = true, id }) : Results.NotFound();
        });

        return group;
    }
}
