using System.CommandLine;
using System.CommandLine.Parsing;
using Axiom.Application.Commands;
using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Cli.Helpers;
using Axiom.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace Axiom.Cli.Commands;

internal static class KnowledgeCommands
{
    public static Command Create(IHost host)
    {
        var cmd = new Command("knowledge", "Manage knowledge entries");

        cmd.Subcommands.Add(CreateCreate(host));
        cmd.Subcommands.Add(CreateUpdate(host));
        cmd.Subcommands.Add(CreateList(host));
        cmd.Subcommands.Add(CreateShow(host));
        cmd.Subcommands.Add(CreateSearch(host));

        return cmd;
    }

    private static Command CreateCreate(IHost host)
    {
        var createCmd = new Command("create", "Create a new knowledge entry");
        var titleOpt = new Option<string>("--title") { Required = true };
        var summaryOpt = new Option<string>("--summary");
        var contentOpt = new Option<string>("--content") { Required = true };
        var systemIdOpt = new Option<long>("--system-id");
        var systemEaiOpt = new Option<string>("--system-eai");
        var typeIdOpt = new Option<long>("--type-id");
        var typeCodeOpt = new Option<string>("--type-code");
        var stateIdOpt = new Option<int>("--state-id");
        var stateCodeOpt = new Option<string>("--state-code");
        var userIdOpt = new Option<Guid>("--created-by");
        var createdByEmailOpt = new Option<string>("--created-by-email");
        var tagsOpt = new Option<string>("--tags");
        var issueIdOpt = new Option<Guid?>("--issue-id");
        var knowledgeCreateJsonOpt = CliOutput.NewJsonOption();
        var knowledgeCreateWizardOpt = CliOutput.NewWizardOption();
        createCmd.Options.Add(titleOpt);
        createCmd.Options.Add(summaryOpt);
        createCmd.Options.Add(contentOpt);
        createCmd.Options.Add(systemIdOpt);
        createCmd.Options.Add(systemEaiOpt);
        createCmd.Options.Add(typeIdOpt);
        createCmd.Options.Add(typeCodeOpt);
        createCmd.Options.Add(stateIdOpt);
        createCmd.Options.Add(stateCodeOpt);
        createCmd.Options.Add(userIdOpt);
        createCmd.Options.Add(createdByEmailOpt);
        createCmd.Options.Add(tagsOpt);
        createCmd.Options.Add(issueIdOpt);
        createCmd.Options.Add(knowledgeCreateJsonOpt);
        createCmd.Options.Add(knowledgeCreateWizardOpt);

        createCmd.SetAction((ParseResult result) =>
        {
            var wizard = result.GetValue(knowledgeCreateWizardOpt);
            var json = result.GetValue(knowledgeCreateJsonOpt);

            if (wizard)
            {
                using var scope = host.Services.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();

                AnsiConsole.Write(new FigletText("Knowledge Wizard").Color(Color.Cyan));
                AnsiConsole.Write(new Rule("[cyan]Create a new knowledge entry[/]"));

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

                var types = references.ListKnowledgeTypesAsync().Result;
                var selectedType = AnsiConsole.Prompt(
                    new SelectionPrompt<ReferenceCodeDto>()
                        .Title("[cyan]Select knowledge type:[/]")
                        .PageSize(10)
                        .AddChoices(types)
                        .UseConverter(t => $"{t.Code} - {t.Name}"));

                var states = references.ListKnowledgeStatesAsync().Result.Select(s => new ReferenceCodeDto { Id = s.Id, Code = s.Code, Name = s.Name }).ToList();
                var selectedState = AnsiConsole.Prompt(
                    new SelectionPrompt<ReferenceCodeDto>()
                        .Title("[cyan]Select knowledge state:[/]")
                        .PageSize(10)
                        .AddChoices(states)
                        .UseConverter(s => $"{s.Code} - {s.Name}"));

                var title = AnsiConsole.Prompt(
                    new TextPrompt<string>("[cyan]Title:[/]")
                        .Validate(v => string.IsNullOrWhiteSpace(v)
                            ? ValidationResult.Error("[red]Title cannot be empty[/]")
                            : ValidationResult.Success()));

                var summary = AnsiConsole.Prompt(
                    new TextPrompt<string>("[cyan]Summary:[/]")
                        .AllowEmpty());

                var content = AnsiConsole.Prompt(
                    new TextPrompt<string>("[cyan]Content:[/]")
                        .Validate(v => string.IsNullOrWhiteSpace(v)
                            ? ValidationResult.Error("[red]Content cannot be empty[/]")
                            : ValidationResult.Success()));

                var tagsInput = AnsiConsole.Prompt(
                    new TextPrompt<string>("[cyan]Tags (comma-separated, optional):[/]")
                        .AllowEmpty());
                var tagList = string.IsNullOrWhiteSpace(tagsInput)
                    ? new List<string>()
                    : [.. tagsInput.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)];

                var issueIdInput = AnsiConsole.Prompt(
                    new TextPrompt<string>("[cyan]Issue ID (optional):[/]")
                        .AllowEmpty()
                        .Validate(v => string.IsNullOrWhiteSpace(v) || Guid.TryParse(v, out _)
                            ? ValidationResult.Success()
                            : ValidationResult.Error("[red]Must be a valid GUID or empty[/]")));
                var issueId = string.IsNullOrWhiteSpace(issueIdInput) ? (Guid?)null : Guid.Parse(issueIdInput);

                var knowledgeId = Guid.NewGuid();

                var summaryTable = new Table();
                summaryTable.AddColumns("Field", "Value");
                summaryTable.AddRow("User", $"{selectedUser.Name} ({selectedUser.Email})");
                summaryTable.AddRow("System", $"{selectedSystem.EAI} - {selectedSystem.Name}");
                summaryTable.AddRow("Type", $"{selectedType.Code} - {selectedType.Name}");
                summaryTable.AddRow("State", $"{selectedState.Code} - {selectedState.Name}");
                summaryTable.AddRow("Title", title);
                summaryTable.AddRow("Summary", summary);
                summaryTable.AddRow("Tags", string.Join(", ", tagList));
                summaryTable.AddRow("Issue ID", issueId?.ToString() ?? "-");
                AnsiConsole.Write(summaryTable);

                if (!AnsiConsole.Confirm("Create this knowledge entry?"))
                {
                    AnsiConsole.MarkupLine("[red]Cancelled.[/]");
                    return;
                }

                try
                {
                    var command = new CreateKnowledgeCommand(
                        title, summary, content, selectedSystem.SystemId,
                        selectedUser.UserId, selectedType.Id, (int)selectedState.Id,
                        issueId, tagList, knowledgeId);

                    var entry = mediator.Send(command).Result;

                    if (json)
                    {
                        CliOutput.WriteJson(Mappers.ToKnowledgeCreateResult(entry));
                        return;
                    }

                    AnsiConsole.MarkupLine($"[green]Knowledge created:[/] {entry.KnowledgeId}");
                    AnsiConsole.MarkupLine($"  [bold]Title:[/] {entry.Title}");
                    AnsiConsole.MarkupLine($"  [bold]System:[/] {selectedSystem.Name}");
                    AnsiConsole.MarkupLine($"  [bold]Type:[/] {selectedType.Name}");
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

                    var jsonEntry = new JsonKnowledgeEntry
                    {
                        KnowledgeId = knowledgeId, Title = title, Summary = summary, Content = content,
                        SystemId = selectedSystem.SystemId, CreatedByUserId = selectedUser.UserId,
                        KnowledgeTypeId = selectedType.Id, KnowledgeStateId = (int)selectedState.Id,
                        IssueId = issueId, Tags = tagList, VersionNumber = 1,
                        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                    };

                    try
                    {
                        store.AppendAsync("knowledge", jsonEntry).Wait();

                        if (json)
                        {
                            CliOutput.WriteJson(jsonEntry);
                            return;
                        }

                        AnsiConsole.MarkupLine($"[yellow]DB unavailable - Knowledge saved to local store:[/] {knowledgeId}");
                        AnsiConsole.MarkupLine($"  [bold]Title:[/] {title}");
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
                json = result.GetValue(knowledgeCreateJsonOpt);

                var tags = result.GetValue(tagsOpt);
                var tagList = string.IsNullOrWhiteSpace(tags)
                    ? new List<string>()
                    : [.. tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)];

                var systemId = Resolvers.ResolveSystemId(result.GetValue(systemIdOpt), result.GetValue(systemEaiOpt), references, json);
                var typeId = Resolvers.ResolveKnowledgeTypeId(result.GetValue(typeIdOpt), result.GetValue(typeCodeOpt), references, json);
                var stateId = Resolvers.ResolveKnowledgeStateId(result.GetValue(stateIdOpt), result.GetValue(stateCodeOpt), references, json);
                var createdByUserId = Resolvers.ResolveUserId(result.GetValue(userIdOpt), result.GetValue(createdByEmailOpt), references, json);
                if (systemId is null || typeId is null || stateId is null || createdByUserId is null)
                {
                    return;
                }

                var title = result.GetValue(titleOpt)!;
                var summary = result.GetValue(summaryOpt) ?? string.Empty;
                var content = result.GetValue(contentOpt)!;
                var issueId = result.GetValue(issueIdOpt);
                var knowledgeId = Guid.NewGuid();

                try
                {
                    var command = new CreateKnowledgeCommand(
                        title, summary, content, systemId.Value,
                        createdByUserId.Value, typeId.Value, stateId.Value,
                        issueId, tagList, knowledgeId);

                    var entry = mediator.Send(command).Result;

                    if (json)
                    {
                        CliOutput.WriteJson(Mappers.ToKnowledgeCreateResult(entry));
                        return;
                    }

                    AnsiConsole.MarkupLine($"[green]Knowledge created:[/] {entry.KnowledgeId}");
                    AnsiConsole.MarkupLine($"  [bold]Title:[/] {entry.Title}");
                    AnsiConsole.MarkupLine($"  [bold]System ID:[/] {entry.SystemId}");
                    AnsiConsole.MarkupLine($"  [bold]Type ID:[/] {entry.KnowledgeTypeId}");
                    AnsiConsole.MarkupLine($"  [bold]State ID:[/] {entry.KnowledgeStateId}");
                }
                catch
                {
                    var store = CliOutput.CreateJsonStore();
                    if (store is null)
                    {
                        CliOutput.WriteError("Database is not available and JSON store could not be created.", json);
                        return;
                    }

                    var jsonEntry = new JsonKnowledgeEntry
                    {
                        KnowledgeId = knowledgeId, Title = title, Summary = summary, Content = content,
                        SystemId = systemId.Value, CreatedByUserId = createdByUserId.Value,
                        KnowledgeTypeId = typeId.Value, KnowledgeStateId = stateId.Value,
                        IssueId = issueId, Tags = tagList, VersionNumber = 1,
                        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                    };

                    try
                    {
                        store.AppendAsync("knowledge", jsonEntry).Wait();

                        if (json)
                        {
                            CliOutput.WriteJson(jsonEntry);
                            return;
                        }

                        AnsiConsole.MarkupLine($"[yellow]DB unavailable - Knowledge saved to local store:[/] {knowledgeId}");
                        AnsiConsole.MarkupLine($"  [bold]Title:[/] {title}");
                        AnsiConsole.MarkupLine($"  [bold]System ID:[/] {systemId}");
                        AnsiConsole.MarkupLine($"  [bold]Type ID:[/] {typeId}");
                        AnsiConsole.MarkupLine($"  [bold]State ID:[/] {stateId}");
                    }
                    catch (Exception storeEx)
                    {
                        CliOutput.WriteError($"Failed to save to local store: {storeEx.Message}", json);
                    }
                }
            }
        });

        return createCmd;
    }

    private static Command CreateUpdate(IHost host)
    {
        var knowledgeUpdateCmd = new Command("update", "Update a knowledge entry");
        var knowledgeUpdateIdArg = new Argument<Guid>("id");
        var knowledgeUpdateTitleOpt = new Option<string>("--title") { Required = true };
        var knowledgeUpdateSummaryOpt = new Option<string>("--summary");
        var knowledgeUpdateContentOpt = new Option<string>("--content") { Required = true };
        var knowledgeUpdateSystemIdOpt = new Option<long>("--system-id");
        var knowledgeUpdateSystemEaiOpt = new Option<string>("--system-eai");
        var knowledgeUpdateTypeIdOpt = new Option<long>("--type-id");
        var knowledgeUpdateTypeCodeOpt = new Option<string>("--type-code");
        var knowledgeUpdateStateIdOpt = new Option<int>("--state-id");
        var knowledgeUpdateStateCodeOpt = new Option<string>("--state-code");
        var knowledgeUpdateTagsOpt = new Option<string>("--tags");
        var knowledgeUpdateIssueIdOpt = new Option<Guid?>("--issue-id");
        var knowledgeUpdateJsonOpt = CliOutput.NewJsonOption();
        knowledgeUpdateCmd.Arguments.Add(knowledgeUpdateIdArg);
        knowledgeUpdateCmd.Options.Add(knowledgeUpdateTitleOpt);
        knowledgeUpdateCmd.Options.Add(knowledgeUpdateSummaryOpt);
        knowledgeUpdateCmd.Options.Add(knowledgeUpdateContentOpt);
        knowledgeUpdateCmd.Options.Add(knowledgeUpdateSystemIdOpt);
        knowledgeUpdateCmd.Options.Add(knowledgeUpdateSystemEaiOpt);
        knowledgeUpdateCmd.Options.Add(knowledgeUpdateTypeIdOpt);
        knowledgeUpdateCmd.Options.Add(knowledgeUpdateTypeCodeOpt);
        knowledgeUpdateCmd.Options.Add(knowledgeUpdateStateIdOpt);
        knowledgeUpdateCmd.Options.Add(knowledgeUpdateStateCodeOpt);
        knowledgeUpdateCmd.Options.Add(knowledgeUpdateTagsOpt);
        knowledgeUpdateCmd.Options.Add(knowledgeUpdateIssueIdOpt);
        knowledgeUpdateCmd.Options.Add(knowledgeUpdateJsonOpt);

        knowledgeUpdateCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
            var json = result.GetValue(knowledgeUpdateJsonOpt);

            var id = result.GetValue(knowledgeUpdateIdArg);
            var tags = result.GetValue(knowledgeUpdateTagsOpt);
            var tagList = string.IsNullOrWhiteSpace(tags)
                ? new List<string>()
                : [.. tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)];

            var systemId = Resolvers.ResolveSystemId(result.GetValue(knowledgeUpdateSystemIdOpt), result.GetValue(knowledgeUpdateSystemEaiOpt), references, json);
            var typeId = Resolvers.ResolveKnowledgeTypeId(result.GetValue(knowledgeUpdateTypeIdOpt), result.GetValue(knowledgeUpdateTypeCodeOpt), references, json);
            var stateId = Resolvers.ResolveKnowledgeStateId(result.GetValue(knowledgeUpdateStateIdOpt), result.GetValue(knowledgeUpdateStateCodeOpt), references, json);
            if (systemId is null || typeId is null || stateId is null)
            {
                return;
            }

            var title = result.GetValue(knowledgeUpdateTitleOpt)!;
            var summary = result.GetValue(knowledgeUpdateSummaryOpt) ?? string.Empty;
            var content = result.GetValue(knowledgeUpdateContentOpt)!;
            var issueId = result.GetValue(knowledgeUpdateIssueIdOpt);

            try
            {
                var command = new UpdateKnowledgeCommand(
                    id, title, summary, content, systemId.Value,
                    typeId.Value, stateId.Value, issueId, tagList);

                var entry = mediator.Send(command).Result;

                if (entry is null)
                {
                    CliOutput.WriteError("Knowledge entry not found.", json);
                    return;
                }

                if (json)
                {
                    CliOutput.WriteJson(new
                    {
                        entry.KnowledgeId,
                        entry.Title,
                        entry.Summary,
                        entry.Content,
                        entry.SystemId,
                        entry.KnowledgeTypeId,
                        entry.KnowledgeStateId,
                        entry.IssueId,
                        Tags = tagList,
                        entry.VersionNumber,
                        entry.UpdatedAt
                    });
                    return;
                }

                AnsiConsole.MarkupLine($"[green]Knowledge updated:[/] {entry.KnowledgeId}");
                AnsiConsole.MarkupLine($"  [bold]Title:[/] {entry.Title}");
                AnsiConsole.MarkupLine($"  [bold]System ID:[/] {entry.SystemId}");
                AnsiConsole.MarkupLine($"  [bold]Type ID:[/] {entry.KnowledgeTypeId}");
                AnsiConsole.MarkupLine($"  [bold]State ID:[/] {entry.KnowledgeStateId}");
                AnsiConsole.MarkupLine($"  [bold]Version:[/] {entry.VersionNumber}");
            }
            catch
            {
                var store = CliOutput.CreateJsonStore();
                if (store is null)
                {
                    CliOutput.WriteError("Database is not available and JSON store could not be created.", json);
                    return;
                }

                var existingEntry = store.FindByIdAsync<JsonKnowledgeEntry>("knowledge", e => e.KnowledgeId == id).Result;
                if (existingEntry is null)
                {
                    CliOutput.WriteError("Knowledge entry not found.", json);
                    return;
                }

                var updatedEntry = new JsonKnowledgeEntry
                {
                    KnowledgeId = id, Title = title, Summary = summary, Content = content,
                    SystemId = systemId.Value, CreatedByUserId = existingEntry.CreatedByUserId,
                    KnowledgeTypeId = typeId.Value, KnowledgeStateId = stateId.Value,
                    IssueId = issueId, Tags = tagList,
                    VersionNumber = existingEntry.VersionNumber + 1,
                    CreatedAt = existingEntry.CreatedAt, UpdatedAt = DateTime.UtcNow
                };

                try
                {
                    store.UpdateAsync("knowledge", e => e.KnowledgeId == id, updatedEntry).Wait();

                    if (json)
                    {
                        CliOutput.WriteJson(updatedEntry);
                        return;
                    }

                    AnsiConsole.MarkupLine($"[yellow]DB unavailable - Knowledge updated in local store:[/] {id}");
                    AnsiConsole.MarkupLine($"  [bold]Title:[/] {title}");
                    AnsiConsole.MarkupLine($"  [bold]System ID:[/] {systemId}");
                }
                catch (Exception storeEx)
                {
                    CliOutput.WriteError($"Failed to update in local store: {storeEx.Message}", json);
                }
            }
        });

        return knowledgeUpdateCmd;
    }

    private static Command CreateList(IHost host)
    {
        var listCmd = new Command("list", "List all knowledge entries");
        var knowledgeListJsonOpt = CliOutput.NewJsonOption();
        listCmd.Options.Add(knowledgeListJsonOpt);

        listCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var json = result.GetValue(knowledgeListJsonOpt);

            try
            {
                var entries = mediator.Send(new ListKnowledgeQuery()).Result;
                if (json)
                {
                    CliOutput.WriteJson(entries);
                    return;
                }

                var table = new Table();
                table.AddColumns("Id", "Title", "System", "Type", "State", "Tags", "Updated");

                foreach (var entry in entries)
                {
                    table.AddRow(
                        entry.KnowledgeId.ToString()[..8],
                        entry.Title,
                        entry.SystemName,
                        entry.TypeName,
                        entry.StateName,
                        string.Join(", ", entry.Tags),
                        entry.UpdatedAt.ToString("yyyy-MM-dd"));
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

                var entries = store.ReadAllAsync<JsonKnowledgeEntry>("knowledge").Result;

                if (json)
                {
                    CliOutput.WriteJson(entries);
                    return;
                }

                AnsiConsole.MarkupLine("[yellow]DB unavailable - showing data from local store[/]");
                var table = new Table();
                table.AddColumns("Id", "Title", "SystemId", "TypeId", "StateId", "Tags", "Updated");

                foreach (var entry in entries)
                {
                    table.AddRow(
                        entry.KnowledgeId.ToString()[..8],
                        entry.Title,
                        entry.SystemId.ToString(),
                        entry.KnowledgeTypeId.ToString(),
                        entry.KnowledgeStateId.ToString(),
                        string.Join(", ", entry.Tags),
                        entry.UpdatedAt.ToString("yyyy-MM-dd"));
                }

                AnsiConsole.Write(table);
            }
        });

        return listCmd;
    }

    private static Command CreateShow(IHost host)
    {
        var showCmd = new Command("show", "Show knowledge entry details");
        var idArg = new Argument<Guid>("id");
        var knowledgeShowJsonOpt = CliOutput.NewJsonOption();
        showCmd.Arguments.Add(idArg);
        showCmd.Options.Add(knowledgeShowJsonOpt);

        showCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var id = result.GetValue(idArg);
            var json = result.GetValue(knowledgeShowJsonOpt);

            try
            {
                var entry = mediator.Send(new GetKnowledgeByIdQuery(id)).Result;

                if (entry is null)
                {
                    CliOutput.WriteError("Knowledge entry not found.", json);
                    return;
                }

                if (json)
                {
                    CliOutput.WriteJson(Mappers.ToKnowledgeDetails(entry));
                    return;
                }

                var panel = new Panel(
                    new Markup(
                        $"[bold]Title:[/] {entry.Title}\n" +
                        $"[bold]Summary:[/] {entry.Summary}\n" +
                        $"[bold]System:[/] {entry.System?.Name ?? entry.SystemId.ToString()}\n" +
                        $"[bold]Type:[/] {entry.Type?.Name ?? entry.KnowledgeTypeId.ToString()}\n" +
                        $"[bold]State:[/] {entry.State?.Name ?? entry.KnowledgeStateId.ToString()}\n" +
                        $"[bold]Created By:[/] {entry.CreatedBy?.Name ?? entry.CreatedByUserId.ToString()}\n" +
                        $"[bold]Tags:[/] {string.Join(", ", entry.KnowledgeKnowledgeTags?.Select(t => t.Tag?.TagName ?? string.Empty) ?? [])}\n" +
                        $"[bold]Version:[/] {entry.VersionNumber}\n" +
                        $"[bold]Created:[/] {entry.CreatedAt:yyyy-MM-dd HH:mm:ss}\n" +
                        $"[bold]Updated:[/] {entry.UpdatedAt:yyyy-MM-dd HH:mm:ss}\n" +
                        $"[bold]Content:[/]\n{entry.Content}"))
                {
                    Header = new PanelHeader($"Knowledge - {entry.KnowledgeId}")
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

                var entry = store.FindByIdAsync<JsonKnowledgeEntry>("knowledge", e => e.KnowledgeId == id).Result;
                if (entry is null)
                {
                    CliOutput.WriteError("Knowledge entry not found.", json);
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
                        $"[bold]Title:[/] {entry.Title}\n" +
                        $"[bold]Summary:[/] {entry.Summary}\n" +
                        $"[bold]System ID:[/] {entry.SystemId}\n" +
                        $"[bold]Type ID:[/] {entry.KnowledgeTypeId}\n" +
                        $"[bold]State ID:[/] {entry.KnowledgeStateId}\n" +
                        $"[bold]Created By User ID:[/] {entry.CreatedByUserId}\n" +
                        $"[bold]Tags:[/] {string.Join(", ", entry.Tags)}\n" +
                        $"[bold]Version:[/] {entry.VersionNumber}\n" +
                        $"[bold]Created:[/] {entry.CreatedAt:yyyy-MM-dd HH:mm:ss}\n" +
                        $"[bold]Updated:[/] {entry.UpdatedAt:yyyy-MM-dd HH:mm:ss}\n" +
                        $"[bold]Content:[/]\n{entry.Content}"))
                {
                    Header = new PanelHeader($"Knowledge - {entry.KnowledgeId} (local store)")
                };

                AnsiConsole.Write(panel);
            }
        });

        return showCmd;
    }

    private static Command CreateSearch(IHost host)
    {
        var searchCmd = new Command("search", "Search knowledge entries");
        var queryArg = new Argument<string>("query");
        var knowledgeSearchJsonOpt = CliOutput.NewJsonOption();
        searchCmd.Arguments.Add(queryArg);
        searchCmd.Options.Add(knowledgeSearchJsonOpt);

        searchCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var json = result.GetValue(knowledgeSearchJsonOpt);

            var query = result.GetValue(queryArg) ?? string.Empty;

            try
            {
                var entries = mediator.Send(new SearchKnowledgeQuery(query)).Result;
                if (json)
                {
                    CliOutput.WriteJson(entries);
                    return;
                }

                var table = new Table();
                table.AddColumns("Id", "Title", "System", "Type", "State");

                foreach (var entry in entries)
                {
                    table.AddRow(
                        entry.KnowledgeId.ToString()[..8],
                        entry.Title,
                        entry.SystemName,
                        entry.TypeName,
                        entry.StateName);
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

                var allEntries = store.ReadAllAsync<JsonKnowledgeEntry>("knowledge").Result;
                var q = query.ToLowerInvariant();
                var matches = allEntries
                    .Where(e => e.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
                        || e.Summary.Contains(q, StringComparison.OrdinalIgnoreCase)
                        || e.Content.Contains(q, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (json)
                {
                    CliOutput.WriteJson(matches);
                    return;
                }

                AnsiConsole.MarkupLine("[yellow]DB unavailable - showing data from local store[/]");
                var table = new Table();
                table.AddColumns("Id", "Title", "SystemId", "TypeId", "StateId");

                foreach (var entry in matches)
                {
                    table.AddRow(
                        entry.KnowledgeId.ToString()[..8],
                        entry.Title,
                        entry.SystemId.ToString(),
                        entry.KnowledgeTypeId.ToString(),
                        entry.KnowledgeStateId.ToString());
                }

                AnsiConsole.Write(table);
            }
        });

        return searchCmd;
    }
}
