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

internal static class SystemCommands
{
    public static Command Create(IHost host)
    {
        var cmd = new Command("system", "Manage systems");

        cmd.Subcommands.Add(CreateList(host));
        cmd.Subcommands.Add(CreateCreate(host));
        cmd.Subcommands.Add(CreateUpdate(host));
        cmd.Subcommands.Add(CreateDelete(host));

        return cmd;
    }

    private static Command CreateList(IHost host)
    {
        var systemListCmd = new Command("list", "List systems");
        var systemListJsonOpt = CliOutput.NewJsonOption();
        systemListCmd.Options.Add(systemListJsonOpt);

        systemListCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
            var json = result.GetValue(systemListJsonOpt);

            try
            {
                var systems = references.ListSystemsAsync().Result;
                if (json)
                {
                    CliOutput.WriteJson(systems);
                    return;
                }

                var table = new Table();
                table.AddColumns("Id", "EAI", "Name", "Owner");
                foreach (var system in systems)
                {
                    table.AddRow(system.SystemId.ToString(), system.EAI, system.Name, system.OwnerName);
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

                var entries = store.ReadAllAsync<JsonSystemEntry>("systems").Result;
                if (json)
                {
                    CliOutput.WriteJson(entries);
                    return;
                }

                AnsiConsole.MarkupLine("[yellow]DB unavailable - showing data from local store[/]");
                var table = new Table();
                table.AddColumns("Id", "EAI", "Name", "OwnerUserId");
                foreach (var entry in entries)
                {
                    table.AddRow(entry.SystemId.ToString(), entry.EAI, entry.Name, entry.OwnerUserId.ToString()[..8]);
                }

                AnsiConsole.Write(table);
            }
        });

        return systemListCmd;
    }

    private static Command CreateCreate(IHost host)
    {
        var systemCreateCmd = new Command("create", "Create a new system");
        var systemCreateEaiOpt = new Option<string>("--eai") { Required = true };
        var systemCreateNameOpt = new Option<string>("--name") { Required = true };
        var systemCreateOwnerIdOpt = new Option<Guid>("--owner-id");
        var systemCreateOwnerEmailOpt = new Option<string>("--owner-email");
        var systemCreateJsonOpt = CliOutput.NewJsonOption();
        systemCreateCmd.Options.Add(systemCreateEaiOpt);
        systemCreateCmd.Options.Add(systemCreateNameOpt);
        systemCreateCmd.Options.Add(systemCreateOwnerIdOpt);
        systemCreateCmd.Options.Add(systemCreateOwnerEmailOpt);
        systemCreateCmd.Options.Add(systemCreateJsonOpt);

        systemCreateCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
            var json = result.GetValue(systemCreateJsonOpt);

            var eai = result.GetValue(systemCreateEaiOpt)!;
            var name = result.GetValue(systemCreateNameOpt)!;
            var ownerUserId = Resolvers.ResolveUserId(result.GetValue(systemCreateOwnerIdOpt), result.GetValue(systemCreateOwnerEmailOpt), references, json);
            if (ownerUserId is null)
                return;

            try
            {
                var entry = mediator.Send(new CreateSystemCommand(eai, name, ownerUserId.Value)).Result;

                if (json)
                {
                    CliOutput.WriteJson(new { entry.SystemId, entry.EAI, entry.Name, entry.OwnerUserId });
                    return;
                }

                AnsiConsole.MarkupLine($"[green]System created:[/] {entry.SystemId}");
                AnsiConsole.MarkupLine($"  [bold]EAI:[/] {entry.EAI}");
                AnsiConsole.MarkupLine($"  [bold]Name:[/] {entry.Name}");
                AnsiConsole.MarkupLine($"  [bold]Owner:[/] {entry.OwnerUserId}");
            }
            catch
            {
                var store = CliOutput.CreateJsonStore();
                if (store is null)
                {
                    CliOutput.WriteError("Database is not available and JSON store could not be created.", json);
                    return;
                }

                var existingSystems = store.ReadAllAsync<JsonSystemEntry>("systems").Result;
                var nextNegativeId = -1 - existingSystems.Count;
                var jsonEntry = new JsonSystemEntry
                {
                    SystemId = nextNegativeId,
                    EAI = eai,
                    Name = name,
                    OwnerUserId = ownerUserId.Value
                };

                try
                {
                    store.AppendAsync("systems", jsonEntry).Wait();

                    if (json)
                    {
                        CliOutput.WriteJson(jsonEntry);
                        return;
                    }

                    AnsiConsole.MarkupLine($"[yellow]DB unavailable - System saved to local store:[/] {eai}");
                    AnsiConsole.MarkupLine($"  [bold]Name:[/] {name}");
                    AnsiConsole.MarkupLine($"  [bold]Owner:[/] {ownerUserId}");
                }
                catch (Exception storeEx)
                {
                    CliOutput.WriteError($"Failed to save to local store: {storeEx.Message}", json);
                }
            }
        });

        return systemCreateCmd;
    }

    private static Command CreateUpdate(IHost host)
    {
        var systemUpdateCmd = new Command("update", "Update a system");
        var systemUpdateIdArg = new Argument<long>("id");
        var systemUpdateEaiOpt = new Option<string>("--eai") { Required = true };
        var systemUpdateNameOpt = new Option<string>("--name") { Required = true };
        var systemUpdateOwnerIdOpt = new Option<Guid>("--owner-id");
        var systemUpdateOwnerEmailOpt = new Option<string>("--owner-email");
        var systemUpdateJsonOpt = CliOutput.NewJsonOption();
        systemUpdateCmd.Arguments.Add(systemUpdateIdArg);
        systemUpdateCmd.Options.Add(systemUpdateEaiOpt);
        systemUpdateCmd.Options.Add(systemUpdateNameOpt);
        systemUpdateCmd.Options.Add(systemUpdateOwnerIdOpt);
        systemUpdateCmd.Options.Add(systemUpdateOwnerEmailOpt);
        systemUpdateCmd.Options.Add(systemUpdateJsonOpt);

        systemUpdateCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
            var json = result.GetValue(systemUpdateJsonOpt);

            var id = result.GetValue(systemUpdateIdArg);
            var eai = result.GetValue(systemUpdateEaiOpt)!;
            var name = result.GetValue(systemUpdateNameOpt)!;
            var ownerUserId = Resolvers.ResolveUserId(result.GetValue(systemUpdateOwnerIdOpt), result.GetValue(systemUpdateOwnerEmailOpt), references, json);
            if (ownerUserId is null)
            {
                return;
            }

            try
            {
                var command = new UpdateSystemCommand(id, eai, name, ownerUserId.Value);
                var entry = mediator.Send(command).Result;

                if (entry is null)
                {
                    CliOutput.WriteError("System not found.", json);
                    return;
                }

                if (json)
                {
                    CliOutput.WriteJson(new
                    {
                        entry.SystemId,
                        entry.EAI,
                        entry.Name,
                        entry.OwnerUserId
                    });
                    return;
                }

                AnsiConsole.MarkupLine($"[green]System updated:[/] {entry.SystemId}");
                AnsiConsole.MarkupLine($"  [bold]EAI:[/] {entry.EAI}");
                AnsiConsole.MarkupLine($"  [bold]Name:[/] {entry.Name}");
                AnsiConsole.MarkupLine($"  [bold]Owner:[/] {entry.OwnerUserId}");
            }
            catch
            {
                var store = CliOutput.CreateJsonStore();
                if (store is null)
                {
                    CliOutput.WriteError("Database is not available and JSON store could not be created.", json);
                    return;
                }

                var existingEntry = store.FindByIdAsync<JsonSystemEntry>("systems", e => e.SystemId == id).Result;
                if (existingEntry is null)
                {
                    CliOutput.WriteError("System not found.", json);
                    return;
                }

                var updatedEntry = new JsonSystemEntry
                {
                    SystemId = id,
                    EAI = eai,
                    Name = name,
                    OwnerUserId = ownerUserId.Value
                };

                try
                {
                    store.UpdateAsync("systems", e => e.SystemId == id, updatedEntry).Wait();

                    if (json)
                    {
                        CliOutput.WriteJson(updatedEntry);
                        return;
                    }

                    AnsiConsole.MarkupLine($"[yellow]DB unavailable - System updated in local store:[/] {id}");
                    AnsiConsole.MarkupLine($"  [bold]EAI:[/] {eai}");
                    AnsiConsole.MarkupLine($"  [bold]Name:[/] {name}");
                }
                catch (Exception storeEx)
                {
                    CliOutput.WriteError($"Failed to update in local store: {storeEx.Message}", json);
                }
            }
        });

        return systemUpdateCmd;
    }

    private static Command CreateDelete(IHost host)
    {
        var systemDeleteCmd = new Command("delete", "Delete a system");
        var systemDeleteIdArg = new Argument<long>("id");
        var systemDeleteJsonOpt = CliOutput.NewJsonOption();
        systemDeleteCmd.Arguments.Add(systemDeleteIdArg);
        systemDeleteCmd.Options.Add(systemDeleteJsonOpt);

        systemDeleteCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var json = result.GetValue(systemDeleteJsonOpt);

            var id = result.GetValue(systemDeleteIdArg);

            try
            {
                var ok = mediator.Send(new DeleteSystemCommand(id)).Result;
                if (!ok)
                {
                    CliOutput.WriteError("System not found.", json);
                    return;
                }

                if (json)
                {
                    CliOutput.WriteJson(new { deleted = true, id });
                    return;
                }

                AnsiConsole.MarkupLine($"[green]System deleted:[/] {id}");
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
                    var ok = store.DeleteAsync<JsonSystemEntry>("systems", e => e.SystemId == id).Result;
                    if (!ok)
                    {
                        CliOutput.WriteError("System not found.", json);
                        return;
                    }

                    if (json)
                    {
                        CliOutput.WriteJson(new { deleted = true, id });
                        return;
                    }

                    AnsiConsole.MarkupLine($"[yellow]DB unavailable - System deleted from local store:[/] {id}");
                }
                catch (Exception storeEx)
                {
                    CliOutput.WriteError($"Failed to delete in local store: {storeEx.Message}", json);
                }
            }
        });

        return systemDeleteCmd;
    }
}
