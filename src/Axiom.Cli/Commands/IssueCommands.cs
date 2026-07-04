using System.CommandLine;
using System.CommandLine.Parsing;
using Axiom.Application.Commands;
using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Cli.Helpers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace Axiom.Cli.Commands;

internal static class IssueCommands
{
    public static Command Create(IHost host)
    {
        var cmd = new Command("issue", "Manage issue records");

        cmd.Subcommands.Add(CreateCreate(host));
        cmd.Subcommands.Add(CreateUpdate(host));
        cmd.Subcommands.Add(CreateList(host));
        cmd.Subcommands.Add(CreateShow(host));
        cmd.Subcommands.Add(CreateDelete(host));

        return cmd;
    }

    private static Command CreateCreate(IHost host)
    {
        var issueCreateCmd = new Command("create", "Create a new issue record");
        var issueSummaryOpt = new Option<string>("--summary") { Required = true };
        var issueSystemIdOpt = new Option<long>("--system-id");
        var issueSystemEaiOpt = new Option<string>("--system-eai");
        var problemOpt = new Option<string>("--problem") { Required = true };
        var analysisOpt = new Option<string>("--analysis");
        var resolutionOpt = new Option<string>("--resolution");
        var issueStateIdOpt = new Option<int>("--state-id");
        var issueStateCodeOpt = new Option<string>("--state-code");
        var issueUserIdOpt = new Option<Guid>("--created-by");
        var issueCreatedByEmailOpt = new Option<string>("--created-by-email");
        var ritmOpt = new Option<string>("--ritm-number");
        var incidentOpt = new Option<string>("--incident-number");
        var issueCreateJsonOpt = CliOutput.NewJsonOption();
        var issueCreateWizardOpt = CliOutput.NewWizardOption();
        issueCreateCmd.Options.Add(issueSummaryOpt);
        issueCreateCmd.Options.Add(issueSystemIdOpt);
        issueCreateCmd.Options.Add(issueSystemEaiOpt);
        issueCreateCmd.Options.Add(problemOpt);
        issueCreateCmd.Options.Add(analysisOpt);
        issueCreateCmd.Options.Add(resolutionOpt);
        issueCreateCmd.Options.Add(issueStateIdOpt);
        issueCreateCmd.Options.Add(issueStateCodeOpt);
        issueCreateCmd.Options.Add(issueUserIdOpt);
        issueCreateCmd.Options.Add(issueCreatedByEmailOpt);
        issueCreateCmd.Options.Add(ritmOpt);
        issueCreateCmd.Options.Add(incidentOpt);
        issueCreateCmd.Options.Add(issueCreateJsonOpt);
        issueCreateCmd.Options.Add(issueCreateWizardOpt);

        issueCreateCmd.SetAction((ParseResult result) =>
        {
            var wizard = result.GetValue(issueCreateWizardOpt);
            var json = result.GetValue(issueCreateJsonOpt);

            if (wizard)
            {
                using var scope = host.Services.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();

                AnsiConsole.Write(new FigletText("Issue Wizard").Color(Color.Yellow));
                AnsiConsole.Write(new Rule("[yellow]Create a new issue[/]"));

                var users = references.ListUsersAsync().Result;
                var selectedUser = AnsiConsole.Prompt(
                    new SelectionPrompt<UserDto>()
                        .Title("[cyan]Select user:[/]")
                        .PageSize(10)
                        .AddChoices(users)
                        .UseConverter(u => $"{u.Name} ({u.Email})"));

                var systems = references.ListSystemsAsync().Result;
                var selectedSystem = AnsiConsole.Prompt(
                    new SelectionPrompt<SystemDto>()
                        .Title("[cyan]Select system:[/]")
                        .PageSize(10)
                        .AddChoices(systems)
                        .UseConverter(s => $"{s.EAI} - {s.Name}"));

                var states = references.ListIssueStatesAsync().Result.Select(s => new ReferenceCodeDto { Id = s.Id, Code = s.Code, Name = s.Name }).ToList();
                var selectedState = AnsiConsole.Prompt(
                    new SelectionPrompt<ReferenceCodeDto>()
                        .Title("[cyan]Select issue state:[/]")
                        .PageSize(10)
                        .AddChoices(states)
                        .UseConverter(s => $"{s.Code} - {s.Name}"));

                var summary = AnsiConsole.Prompt(
                    new TextPrompt<string>("[cyan]Summary:[/]")
                        .Validate(v => string.IsNullOrWhiteSpace(v)
                            ? ValidationResult.Error("[red]Summary cannot be empty[/]")
                            : ValidationResult.Success()));

                var problem = AnsiConsole.Prompt(
                    new TextPrompt<string>("[cyan]Problem description:[/]")
                        .Validate(v => string.IsNullOrWhiteSpace(v)
                            ? ValidationResult.Error("[red]Problem cannot be empty[/]")
                            : ValidationResult.Success()));

                var analysis = AnsiConsole.Prompt(
                    new TextPrompt<string>("[cyan]Analysis (optional):[/]")
                        .AllowEmpty());

                var resolution = AnsiConsole.Prompt(
                    new TextPrompt<string>("[cyan]Resolution (optional):[/]")
                        .AllowEmpty());

                var ritmNumber = AnsiConsole.Prompt(
                    new TextPrompt<string>("[cyan]RITM number (optional):[/]")
                        .AllowEmpty());

                var ritmValue = string.IsNullOrWhiteSpace(ritmNumber) ? null : ritmNumber;

                var incidentNumber = AnsiConsole.Prompt(
                    new TextPrompt<string>("[cyan]Incident number (optional):[/]")
                        .AllowEmpty());

                var incidentValue = string.IsNullOrWhiteSpace(incidentNumber) ? null : incidentNumber;

                var issueId = Guid.NewGuid();

                var summaryTable = new Table();
                summaryTable.AddColumns("Field", "Value");
                summaryTable.AddRow("User", $"{selectedUser.Name} ({selectedUser.Email})");
                summaryTable.AddRow("System", $"{selectedSystem.EAI} - {selectedSystem.Name}");
                summaryTable.AddRow("State", $"{selectedState.Code} - {selectedState.Name}");
                summaryTable.AddRow("Summary", summary);
                summaryTable.AddRow("RITM", ritmValue ?? "-");
                summaryTable.AddRow("Incident", incidentValue ?? "-");
                AnsiConsole.Write(summaryTable);

                if (!AnsiConsole.Confirm("Create this issue?"))
                {
                    AnsiConsole.MarkupLine("[red]Cancelled.[/]");
                    return;
                }

                try
                {
                    var command = new CreateIssueCommand(
                        summary, selectedSystem.SystemId, problem, analysis, resolution,
                        (int)selectedState.Id, selectedUser.UserId, ritmValue, incidentValue, issueId);

                    var issue = mediator.Send(command).Result;

                    if (json)
                    {
                        CliOutput.WriteJson(Mappers.ToIssueCreateResult(issue));
                        return;
                    }

                    AnsiConsole.MarkupLine($"[green]Issue created:[/] {issue.IssueId}");
                    AnsiConsole.MarkupLine($"  [bold]Summary:[/] {issue.Summary}");
                    AnsiConsole.MarkupLine($"  [bold]System:[/] {selectedSystem.Name}");
                    AnsiConsole.MarkupLine($"  [bold]State:[/] {selectedState.Name}");
                }
                catch
                {
                    var store = CliOutput.CreateJsonStore();
                    if (store is null)
                    {
                        CliOutput.WriteError("Database is not available and JSON store could not be created.", json);
                        return;
                    }

                    var jsonEntry = new JsonIssueEntry
                    {
                        IssueId = issueId, Summary = summary, SystemId = selectedSystem.SystemId,
                        Problem = problem, Analysis = analysis, Resolution = resolution,
                        StateId = (int)selectedState.Id, CreatedByUserId = selectedUser.UserId,
                        RitmNumber = ritmValue, IncidentNumber = incidentValue,
                        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                    };

                    try
                    {
                        store.AppendAsync("issues", jsonEntry).Wait();

                        if (json)
                        {
                            CliOutput.WriteJson(jsonEntry);
                            return;
                        }

                        AnsiConsole.MarkupLine($"[yellow]DB unavailable - Issue saved to local store:[/] {issueId}");
                        AnsiConsole.MarkupLine($"  [bold]Summary:[/] {summary}");
                        AnsiConsole.MarkupLine($"  [bold]System ID:[/] {selectedSystem.SystemId}");
                    }
                    catch (Exception storeEx)
                    {
                        CliOutput.WriteError($"Failed to save to local store: {storeEx.Message}", json);
                    }
                }

                return;
            }

            {
                using var scope = host.Services.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
                json = result.GetValue(issueCreateJsonOpt);

                var systemId = Resolvers.ResolveSystemId(result.GetValue(issueSystemIdOpt), result.GetValue(issueSystemEaiOpt), references, json);
                var stateId = Resolvers.ResolveIssueStateId(result.GetValue(issueStateIdOpt), result.GetValue(issueStateCodeOpt), references, json);
                var createdByUserId = Resolvers.ResolveUserId(result.GetValue(issueUserIdOpt), result.GetValue(issueCreatedByEmailOpt), references, json);
                if (systemId is null || stateId is null || createdByUserId is null)
                {
                    return;
                }

                var summary = result.GetValue(issueSummaryOpt)!;
                var problem = result.GetValue(problemOpt)!;
                var analysis = result.GetValue(analysisOpt) ?? string.Empty;
                var resolution = result.GetValue(resolutionOpt) ?? string.Empty;
                var ritmNumber = result.GetValue(ritmOpt);
                var incidentNumber = result.GetValue(incidentOpt);
                var issueId = Guid.NewGuid();

                try
                {
                    var command = new CreateIssueCommand(
                        summary, systemId.Value, problem, analysis, resolution,
                        stateId.Value, createdByUserId.Value, ritmNumber, incidentNumber, issueId);

                    var issue = mediator.Send(command).Result;

                    if (json)
                    {
                        CliOutput.WriteJson(Mappers.ToIssueCreateResult(issue));
                        return;
                    }

                    AnsiConsole.MarkupLine($"[green]Issue created:[/] {issue.IssueId}");
                    AnsiConsole.MarkupLine($"  [bold]Summary:[/] {issue.Summary}");
                    AnsiConsole.MarkupLine($"  [bold]System ID:[/] {issue.SystemId}");
                    AnsiConsole.MarkupLine($"  [bold]State ID:[/] {issue.StateId}");
                }
                catch
                {
                    var store = CliOutput.CreateJsonStore();
                    if (store is null)
                    {
                        CliOutput.WriteError("Database is not available and JSON store could not be created.", json);
                        return;
                    }

                    var jsonEntry = new JsonIssueEntry
                    {
                        IssueId = issueId, Summary = summary, SystemId = systemId.Value,
                        Problem = problem, Analysis = analysis, Resolution = resolution,
                        StateId = stateId.Value, CreatedByUserId = createdByUserId.Value,
                        RitmNumber = ritmNumber, IncidentNumber = incidentNumber,
                        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                    };

                    try
                    {
                        store.AppendAsync("issues", jsonEntry).Wait();

                        if (json)
                        {
                            CliOutput.WriteJson(jsonEntry);
                            return;
                        }

                        AnsiConsole.MarkupLine($"[yellow]DB unavailable - Issue saved to local store:[/] {issueId}");
                        AnsiConsole.MarkupLine($"  [bold]Summary:[/] {summary}");
                        AnsiConsole.MarkupLine($"  [bold]System ID:[/] {systemId}");
                        AnsiConsole.MarkupLine($"  [bold]State ID:[/] {stateId}");
                    }
                    catch (Exception storeEx)
                    {
                        CliOutput.WriteError($"Failed to save to local store: {storeEx.Message}", json);
                    }
                }
            }
        });

        return issueCreateCmd;
    }

    private static Command CreateUpdate(IHost host)
    {
        var issueUpdateCmd = new Command("update", "Update an issue record");
        var issueUpdateIdArg = new Argument<Guid>("id");
        var issueUpdateSummaryOpt = new Option<string>("--summary") { Required = true };
        var issueUpdateSystemIdOpt = new Option<long>("--system-id");
        var issueUpdateSystemEaiOpt = new Option<string>("--system-eai");
        var issueUpdateProblemOpt = new Option<string>("--problem") { Required = true };
        var issueUpdateAnalysisOpt = new Option<string>("--analysis");
        var issueUpdateResolutionOpt = new Option<string>("--resolution");
        var issueUpdateStateIdOpt = new Option<int>("--state-id");
        var issueUpdateStateCodeOpt = new Option<string>("--state-code");
        var issueUpdateRitmOpt = new Option<string>("--ritm-number");
        var issueUpdateIncidentOpt = new Option<string>("--incident-number");
        var issueUpdateJsonOpt = CliOutput.NewJsonOption();
        issueUpdateCmd.Arguments.Add(issueUpdateIdArg);
        issueUpdateCmd.Options.Add(issueUpdateSummaryOpt);
        issueUpdateCmd.Options.Add(issueUpdateSystemIdOpt);
        issueUpdateCmd.Options.Add(issueUpdateSystemEaiOpt);
        issueUpdateCmd.Options.Add(issueUpdateProblemOpt);
        issueUpdateCmd.Options.Add(issueUpdateAnalysisOpt);
        issueUpdateCmd.Options.Add(issueUpdateResolutionOpt);
        issueUpdateCmd.Options.Add(issueUpdateStateIdOpt);
        issueUpdateCmd.Options.Add(issueUpdateStateCodeOpt);
        issueUpdateCmd.Options.Add(issueUpdateRitmOpt);
        issueUpdateCmd.Options.Add(issueUpdateIncidentOpt);
        issueUpdateCmd.Options.Add(issueUpdateJsonOpt);

        issueUpdateCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
            var json = result.GetValue(issueUpdateJsonOpt);

            var id = result.GetValue(issueUpdateIdArg);
            var systemId = Resolvers.ResolveSystemId(result.GetValue(issueUpdateSystemIdOpt), result.GetValue(issueUpdateSystemEaiOpt), references, json);
            var stateId = Resolvers.ResolveIssueStateId(result.GetValue(issueUpdateStateIdOpt), result.GetValue(issueUpdateStateCodeOpt), references, json);
            if (systemId is null || stateId is null)
            {
                return;
            }

            var summary = result.GetValue(issueUpdateSummaryOpt)!;
            var problem = result.GetValue(issueUpdateProblemOpt)!;
            var analysis = result.GetValue(issueUpdateAnalysisOpt) ?? string.Empty;
            var resolution = result.GetValue(issueUpdateResolutionOpt) ?? string.Empty;
            var ritmNumber = result.GetValue(issueUpdateRitmOpt);
            var incidentNumber = result.GetValue(issueUpdateIncidentOpt);

            try
            {
                var command = new UpdateIssueCommand(
                    id, summary, problem, analysis, resolution,
                    systemId.Value, stateId.Value, ritmNumber, incidentNumber);

                var issue = mediator.Send(command).Result;

                if (issue is null)
                {
                    CliOutput.WriteError("Issue not found.", json);
                    return;
                }

                if (json)
                {
                    CliOutput.WriteJson(new
                    {
                        issue.IssueId,
                        issue.Summary,
                        issue.SystemId,
                        issue.StateId,
                        issue.RitmNumber,
                        issue.IncidentNumber,
                        issue.Problem,
                        issue.Analysis,
                        issue.Resolution,
                        issue.UpdatedAt
                    });
                    return;
                }

                AnsiConsole.MarkupLine($"[green]Issue updated:[/] {issue.IssueId}");
                AnsiConsole.MarkupLine($"  [bold]Summary:[/] {issue.Summary}");
                AnsiConsole.MarkupLine($"  [bold]System ID:[/] {issue.SystemId}");
                AnsiConsole.MarkupLine($"  [bold]State ID:[/] {issue.StateId}");
            }
            catch
            {
                var store = CliOutput.CreateJsonStore();
                if (store is null)
                {
                    CliOutput.WriteError("Database is not available and JSON store could not be created.", json);
                    return;
                }

                var existingEntry = store.FindByIdAsync<JsonIssueEntry>("issues", e => e.IssueId == id).Result;
                if (existingEntry is null)
                {
                    CliOutput.WriteError("Issue not found.", json);
                    return;
                }

                var updatedEntry = new JsonIssueEntry
                {
                    IssueId = id, Summary = summary, SystemId = systemId.Value,
                    Problem = problem, Analysis = analysis, Resolution = resolution,
                    StateId = stateId.Value, CreatedByUserId = existingEntry.CreatedByUserId,
                    RitmNumber = ritmNumber, IncidentNumber = incidentNumber,
                    CreatedAt = existingEntry.CreatedAt, UpdatedAt = DateTime.UtcNow,
                    ResolvedAt = existingEntry.ResolvedAt
                };

                try
                {
                    store.UpdateAsync("issues", e => e.IssueId == id, updatedEntry).Wait();

                    if (json)
                    {
                        CliOutput.WriteJson(updatedEntry);
                        return;
                    }

                    AnsiConsole.MarkupLine($"[yellow]DB unavailable - Issue updated in local store:[/] {id}");
                    AnsiConsole.MarkupLine($"  [bold]Summary:[/] {summary}");
                    AnsiConsole.MarkupLine($"  [bold]System ID:[/] {systemId}");
                }
                catch (Exception storeEx)
                {
                    CliOutput.WriteError($"Failed to update in local store: {storeEx.Message}", json);
                }
            }
        });

        return issueUpdateCmd;
    }

    private static Command CreateList(IHost host)
    {
        var eaiOpt = new Option<string>("--eai")
        {
            Description = "Filter by system EAI code"
        };
        var issueListCmd = new Command("list", "List all issues");
        var issueListJsonOpt = CliOutput.NewJsonOption();
        issueListCmd.Options.Add(eaiOpt);
        issueListCmd.Options.Add(issueListJsonOpt);

        issueListCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var json = result.GetValue(issueListJsonOpt);

            var eai = result.GetValue(eaiOpt);

            try
            {
                var issues = mediator.Send(new ListIssuesQuery(eai)).Result;
                if (json)
                {
                    CliOutput.WriteJson(issues);
                    return;
                }

                var table = new Table();
                table.AddColumns("Id", "Summary", "System", "State", "RITM", "Incident", "Created");

                foreach (var issue in issues)
                {
                    table.AddRow(
                        issue.IssueId.ToString()[..8],
                        issue.Summary,
                        issue.SystemName,
                        issue.StateName,
                        issue.RitmNumber ?? "-",
                        issue.IncidentNumber ?? "-",
                        issue.CreatedAt.ToString("yyyy-MM-dd"));
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

                var entries = store.ReadAllAsync<JsonIssueEntry>("issues").Result;

                if (json)
                {
                    CliOutput.WriteJson(entries);
                    return;
                }

                AnsiConsole.MarkupLine("[yellow]DB unavailable - showing data from local store[/]");
                var table = new Table();
                table.AddColumns("Id", "Summary", "SystemId", "StateId", "RITM", "Incident", "Created");

                foreach (var entry in entries)
                {
                    table.AddRow(
                        entry.IssueId.ToString()[..8],
                        entry.Summary,
                        entry.SystemId.ToString(),
                        entry.StateId.ToString(),
                        entry.RitmNumber ?? "-",
                        entry.IncidentNumber ?? "-",
                        entry.CreatedAt.ToString("yyyy-MM-dd"));
                }

                AnsiConsole.Write(table);
            }
        });

        return issueListCmd;
    }

    private static Command CreateShow(IHost host)
    {
        var issueShowCmd = new Command("show", "Show issue record details");
        var issueIdArg = new Argument<Guid>("id");
        var issueShowJsonOpt = CliOutput.NewJsonOption();
        issueShowCmd.Arguments.Add(issueIdArg);
        issueShowCmd.Options.Add(issueShowJsonOpt);

        issueShowCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var id = result.GetValue(issueIdArg);
            var json = result.GetValue(issueShowJsonOpt);

            try
            {
                var issue = mediator.Send(new GetIssueByIdQuery(id)).Result;

                if (issue is null)
                {
                    CliOutput.WriteError("Issue not found.", json);
                    return;
                }

                if (json)
                {
                    CliOutput.WriteJson(Mappers.ToIssueDetails(issue));
                    return;
                }

                var panel = new Panel(
                    new Markup(
                        $"[bold]Summary:[/] {issue.Summary}\n" +
                        $"[bold]System:[/] {issue.System?.Name ?? issue.SystemId.ToString()}\n" +
                        $"[bold]State:[/] {issue.State?.Name ?? issue.StateId.ToString()}\n" +
                        $"[bold]RITM:[/] {issue.RitmNumber ?? "-"}\n" +
                        $"[bold]Incident:[/] {issue.IncidentNumber ?? "-"}\n" +
                        $"[bold]Created By:[/] {issue.CreatedBy?.Name ?? issue.CreatedByUserId.ToString()}\n" +
                        $"[bold]Created:[/] {issue.CreatedAt:yyyy-MM-dd HH:mm:ss}\n" +
                        $"[bold]Resolved:[/] {(issue.ResolvedAt.HasValue ? issue.ResolvedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "-")}\n" +
                        $"[bold]Problem:[/]\n{issue.Problem}\n\n" +
                        $"[bold]Analysis:[/]\n{issue.Analysis}\n\n" +
                        $"[bold]Resolution:[/]\n{issue.Resolution}"))
                {
                    Header = new PanelHeader($"Issue - {issue.IssueId}")
                };

                AnsiConsole.Write(panel);
            }
            catch
            {
                var store = CliOutput.CreateJsonStore();
                if (store is null)
                {
                    CliOutput.WriteError("Database is not available.", json);
                    return;
                }

                var entry = store.FindByIdAsync<JsonIssueEntry>("issues", e => e.IssueId == id).Result;
                if (entry is null)
                {
                    CliOutput.WriteError("Issue not found.", json);
                    return;
                }

                if (json)
                {
                    CliOutput.WriteJson(entry);
                    return;
                }

                AnsiConsole.MarkupLine("[yellow]DB unavailable - showing data from local store[/]");
                var panel = new Panel(
                    new Markup(
                        $"[bold]Summary:[/] {entry.Summary}\n" +
                        $"[bold]System ID:[/] {entry.SystemId}\n" +
                        $"[bold]State ID:[/] {entry.StateId}\n" +
                        $"[bold]RITM:[/] {entry.RitmNumber ?? "-"}\n" +
                        $"[bold]Incident:[/] {entry.IncidentNumber ?? "-"}\n" +
                        $"[bold]Created By User ID:[/] {entry.CreatedByUserId}\n" +
                        $"[bold]Created:[/] {entry.CreatedAt:yyyy-MM-dd HH:mm:ss}\n" +
                        $"[bold]Resolved:[/] {(entry.ResolvedAt.HasValue ? entry.ResolvedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "-")}\n" +
                        $"[bold]Problem:[/]\n{entry.Problem}\n\n" +
                        $"[bold]Analysis:[/]\n{entry.Analysis}\n\n" +
                        $"[bold]Resolution:[/]\n{entry.Resolution}"))
                {
                    Header = new PanelHeader($"Issue - {entry.IssueId} (local store)")
                };

                AnsiConsole.Write(panel);
            }
        });

        return issueShowCmd;
    }

    private static Command CreateDelete(IHost host)
    {
        var issueDeleteCmd = new Command("delete", "Delete an issue record");
        var issueDeleteIdArg = new Argument<Guid>("id");
        var issueDeleteJsonOpt = CliOutput.NewJsonOption();
        issueDeleteCmd.Arguments.Add(issueDeleteIdArg);
        issueDeleteCmd.Options.Add(issueDeleteJsonOpt);

        issueDeleteCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var json = result.GetValue(issueDeleteJsonOpt);

            var id = result.GetValue(issueDeleteIdArg);

            try
            {
                var ok = mediator.Send(new DeleteIssueCommand(id)).Result;
                if (!ok)
                {
                    CliOutput.WriteError("Issue not found.", json);
                    return;
                }

                if (json)
                {
                    CliOutput.WriteJson(new { deleted = true, id });
                    return;
                }

                AnsiConsole.MarkupLine($"[green]Issue deleted:[/] {id}");
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
                    var ok = store.DeleteAsync<JsonIssueEntry>("issues", e => e.IssueId == id).Result;
                    if (!ok)
                    {
                        CliOutput.WriteError("Issue not found.", json);
                        return;
                    }

                    if (json)
                    {
                        CliOutput.WriteJson(new { deleted = true, id });
                        return;
                    }

                    AnsiConsole.MarkupLine($"[yellow]DB unavailable - Issue deleted from local store:[/] {id}");
                }
                catch (Exception storeEx)
                {
                    CliOutput.WriteError($"Failed to delete in local store: {storeEx.Message}", json);
                }
            }
        });

        return issueDeleteCmd;
    }
}
