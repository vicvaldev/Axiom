using System.CommandLine;
using System.CommandLine.Parsing;
using Axiom.Application.Commands;
using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Axiom.Cli.Helpers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace Axiom.Cli.Commands;

internal static class UserCommands
{
    public static Command Create(IHost host)
    {
        var cmd = new Command("user", "Manage users");

        cmd.Subcommands.Add(CreateList(host));
        cmd.Subcommands.Add(CreateCreate(host));
        cmd.Subcommands.Add(CreateUpdate(host));
        cmd.Subcommands.Add(CreateDelete(host));

        return cmd;
    }

    private static Command CreateList(IHost host)
    {
        var userListCmd = new Command("list", "List users");
        var userListJsonOpt = CliOutput.NewJsonOption();
        userListCmd.Options.Add(userListJsonOpt);

        userListCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
            var json = result.GetValue(userListJsonOpt);

            try
            {
                var users = references.ListUsersAsync().Result;
                if (json)
                {
                    CliOutput.WriteJson(users);
                    return;
                }

                var table = new Table();
                table.AddColumns("Id", "Email", "Name");
                foreach (var user in users)
                {
                    table.AddRow(user.UserId.ToString()[..8], user.Email, user.Name);
                }

                AnsiConsole.Write(table);
            }
            catch
            {
                var store = CliOutput.CreateJsonStore();
                if (store is null)
                {
                    CliOutput.WriteError("Database is not available.", json);
                    return;
                }

                var entries = store.ReadAllAsync<JsonUserEntry>("users").Result;
                if (json)
                {
                    CliOutput.WriteJson(entries);
                    return;
                }

                AnsiConsole.MarkupLine("[yellow]DB unavailable - showing data from local store[/]");
                var table = new Table();
                table.AddColumns("Id", "Email", "Name");
                foreach (var entry in entries)
                {
                    table.AddRow(entry.UserId.ToString()[..8], entry.Email, entry.Name);
                }

                AnsiConsole.Write(table);
            }
        });

        return userListCmd;
    }

    private static Command CreateCreate(IHost host)
    {
        var userCreateCmd = new Command("create", "Create a new user");
        var userCreateEmailOpt = new Option<string>("--email") { Required = true };
        var userCreateNameOpt = new Option<string>("--name") { Required = true };
        var userCreateJsonOpt = CliOutput.NewJsonOption();
        userCreateCmd.Options.Add(userCreateEmailOpt);
        userCreateCmd.Options.Add(userCreateNameOpt);
        userCreateCmd.Options.Add(userCreateJsonOpt);

        userCreateCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var json = result.GetValue(userCreateJsonOpt);

            var email = result.GetValue(userCreateEmailOpt)!;
            var name = result.GetValue(userCreateNameOpt)!;

            try
            {
                var entry = mediator.Send(new CreateUserCommand(email, name)).Result;

                if (json)
                {
                    CliOutput.WriteJson(new { entry.UserId, entry.Email, entry.Name });
                    return;
                }

                AnsiConsole.MarkupLine($"[green]User created:[/] {entry.UserId}");
                AnsiConsole.MarkupLine($"  [bold]Email:[/] {entry.Email}");
                AnsiConsole.MarkupLine($"  [bold]Name:[/] {entry.Name}");
            }
            catch
            {
                var store = CliOutput.CreateJsonStore();
                if (store is null)
                {
                    CliOutput.WriteError("Database is not available and JSON store could not be created.", json);
                    return;
                }

                var userId = Guid.NewGuid();
                var jsonEntry = new JsonUserEntry
                {
                    UserId = userId,
                    Email = email,
                    Name = name
                };

                try
                {
                    store.AppendAsync("users", jsonEntry).Wait();

                    if (json)
                    {
                        CliOutput.WriteJson(jsonEntry);
                        return;
                    }

                    AnsiConsole.MarkupLine($"[yellow]DB unavailable - User saved to local store:[/] {userId}");
                    AnsiConsole.MarkupLine($"  [bold]Email:[/] {email}");
                    AnsiConsole.MarkupLine($"  [bold]Name:[/] {name}");
                }
                catch (Exception storeEx)
                {
                    CliOutput.WriteError($"Failed to save to local store: {storeEx.Message}", json);
                }
            }
        });

        return userCreateCmd;
    }

    private static Command CreateUpdate(IHost host)
    {
        var userUpdateCmd = new Command("update", "Update a user");
        var userUpdateIdArg = new Argument<Guid>("id");
        var userUpdateEmailOpt = new Option<string>("--email") { Required = true };
        var userUpdateNameOpt = new Option<string>("--name") { Required = true };
        var userUpdateJsonOpt = CliOutput.NewJsonOption();
        userUpdateCmd.Arguments.Add(userUpdateIdArg);
        userUpdateCmd.Options.Add(userUpdateEmailOpt);
        userUpdateCmd.Options.Add(userUpdateNameOpt);
        userUpdateCmd.Options.Add(userUpdateJsonOpt);

        userUpdateCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var json = result.GetValue(userUpdateJsonOpt);

            var id = result.GetValue(userUpdateIdArg);
            var email = result.GetValue(userUpdateEmailOpt)!;
            var name = result.GetValue(userUpdateNameOpt)!;

            try
            {
                var command = new UpdateUserCommand(id, email, name);
                var entry = mediator.Send(command).Result;

                if (entry is null)
                {
                    CliOutput.WriteError("User not found.", json);
                    return;
                }

                if (json)
                {
                    CliOutput.WriteJson(new
                    {
                        entry.UserId,
                        entry.Email,
                        entry.Name
                    });
                    return;
                }

                AnsiConsole.MarkupLine($"[green]User updated:[/] {entry.UserId}");
                AnsiConsole.MarkupLine($"  [bold]Email:[/] {entry.Email}");
                AnsiConsole.MarkupLine($"  [bold]Name:[/] {entry.Name}");
            }
            catch
            {
                var store = CliOutput.CreateJsonStore();
                if (store is null)
                {
                    CliOutput.WriteError("Database is not available and JSON store could not be created.", json);
                    return;
                }

                var existingEntry = store.FindByIdAsync<JsonUserEntry>("users", e => e.UserId == id).Result;
                if (existingEntry is null)
                {
                    CliOutput.WriteError("User not found.", json);
                    return;
                }

                var updatedEntry = new JsonUserEntry
                {
                    UserId = id,
                    Email = email,
                    Name = name
                };

                try
                {
                    store.UpdateAsync("users", e => e.UserId == id, updatedEntry).Wait();

                    if (json)
                    {
                        CliOutput.WriteJson(updatedEntry);
                        return;
                    }

                    AnsiConsole.MarkupLine($"[yellow]DB unavailable - User updated in local store:[/] {id}");
                    AnsiConsole.MarkupLine($"  [bold]Email:[/] {email}");
                    AnsiConsole.MarkupLine($"  [bold]Name:[/] {name}");
                }
                catch (Exception storeEx)
                {
                    CliOutput.WriteError($"Failed to update in local store: {storeEx.Message}", json);
                }
            }
        });

        return userUpdateCmd;
    }

    private static Command CreateDelete(IHost host)
    {
        var userDeleteCmd = new Command("delete", "Delete a user");
        var userDeleteIdArg = new Argument<Guid>("id");
        var userDeleteJsonOpt = CliOutput.NewJsonOption();
        userDeleteCmd.Arguments.Add(userDeleteIdArg);
        userDeleteCmd.Options.Add(userDeleteJsonOpt);

        userDeleteCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var json = result.GetValue(userDeleteJsonOpt);

            var id = result.GetValue(userDeleteIdArg);

            try
            {
                var ok = mediator.Send(new DeleteUserCommand(id)).Result;
                if (!ok)
                {
                    CliOutput.WriteError("User not found.", json);
                    return;
                }

                if (json)
                {
                    CliOutput.WriteJson(new { deleted = true, id });
                    return;
                }

                AnsiConsole.MarkupLine($"[green]User deleted:[/] {id}");
            }
            catch
            {
                var store = CliOutput.CreateJsonStore();
                if (store is null)
                {
                    CliOutput.WriteError("Database is not available and JSON store could not be created.", json);
                    return;
                }

                try
                {
                    var ok = store.DeleteAsync<JsonUserEntry>("users", e => e.UserId == id).Result;
                    if (!ok)
                    {
                        CliOutput.WriteError("User not found.", json);
                        return;
                    }

                    if (json)
                    {
                        CliOutput.WriteJson(new { deleted = true, id });
                        return;
                    }

                    AnsiConsole.MarkupLine($"[yellow]DB unavailable - User deleted from local store:[/] {id}");
                }
                catch (Exception storeEx)
                {
                    CliOutput.WriteError($"Failed to delete in local store: {storeEx.Message}", json);
                }
            }
        });

        return userDeleteCmd;
    }
}
