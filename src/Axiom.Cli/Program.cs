using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using Axiom.Application;
using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Domain.Entities;
using Axiom.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};

Option<bool> NewJsonOption() => new("--json")
{
    Description = "Write machine-readable JSON output"
};

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

var connectionString = Environment.GetEnvironmentVariable("AXIOM_CONNECTION_STRING")
    ?? "Server=localhost;Database=AXIOM;Integrated Security=True;TrustServerCertificate=True;";

builder.Services
    .AddApplication()
    .AddInfrastructure(connectionString);

var host = builder.Build();

var rootCommand = new RootCommand("Axiom - KnowledgeOps and Operational Continuity Platform");

var knowledgeCmd = new Command("knowledge", "Manage knowledge entries");

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
var knowledgeCreateJsonOpt = NewJsonOption();
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

createCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
    var json = result.GetValue(knowledgeCreateJsonOpt);

    var tags = result.GetValue(tagsOpt);
    var tagList = string.IsNullOrWhiteSpace(tags)
        ? new List<string>()
        : [.. tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)];

    var systemId = ResolveSystemId(result.GetValue(systemIdOpt), result.GetValue(systemEaiOpt), references, json);
    var typeId = ResolveKnowledgeTypeId(result.GetValue(typeIdOpt), result.GetValue(typeCodeOpt), references, json);
    var stateId = ResolveKnowledgeStateId(result.GetValue(stateIdOpt), result.GetValue(stateCodeOpt), references, json);
    var createdByUserId = ResolveUserId(result.GetValue(userIdOpt), result.GetValue(createdByEmailOpt), references, json);
    if (systemId is null || typeId is null || stateId is null || createdByUserId is null)
    {
        return;
    }

    var command = new CreateKnowledgeCommand(
        result.GetValue(titleOpt)!,
        result.GetValue(summaryOpt) ?? string.Empty,
        result.GetValue(contentOpt)!,
        systemId.Value,
        createdByUserId.Value,
        typeId.Value,
        stateId.Value,
        result.GetValue(issueIdOpt),
        tagList);

    var entry = mediator.Send(command).Result;

    if (json)
    {
        WriteJson(ToKnowledgeCreateResult(entry));
        return;
    }

    AnsiConsole.MarkupLine($"[green]Knowledge created:[/] {entry.KnowledgeId}");
    AnsiConsole.MarkupLine($"  [bold]Title:[/] {entry.Title}");
    AnsiConsole.MarkupLine($"  [bold]System ID:[/] {entry.SystemId}");
    AnsiConsole.MarkupLine($"  [bold]Type ID:[/] {entry.KnowledgeTypeId}");
    AnsiConsole.MarkupLine($"  [bold]State ID:[/] {entry.KnowledgeStateId}");
});

knowledgeCmd.Subcommands.Add(createCmd);

var listCmd = new Command("list", "List all knowledge entries");
var knowledgeListJsonOpt = NewJsonOption();
listCmd.Options.Add(knowledgeListJsonOpt);
listCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    var entries = mediator.Send(new ListKnowledgeQuery()).Result;
    if (result.GetValue(knowledgeListJsonOpt))
    {
        WriteJson(entries);
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
});

knowledgeCmd.Subcommands.Add(listCmd);

var showCmd = new Command("show", "Show knowledge entry details");
var idArg = new Argument<Guid>("id");
var knowledgeShowJsonOpt = NewJsonOption();
showCmd.Arguments.Add(idArg);
showCmd.Options.Add(knowledgeShowJsonOpt);
showCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    var entry = mediator.Send(new GetKnowledgeByIdQuery(result.GetValue(idArg))).Result;

    if (entry is null)
    {
        WriteError("Knowledge entry not found.", result.GetValue(knowledgeShowJsonOpt));
        return;
    }

    if (result.GetValue(knowledgeShowJsonOpt))
    {
        WriteJson(ToKnowledgeDetails(entry));
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
});

knowledgeCmd.Subcommands.Add(showCmd);

var searchCmd = new Command("search", "Search knowledge entries");
var queryArg = new Argument<string>("query");
var knowledgeSearchJsonOpt = NewJsonOption();
searchCmd.Arguments.Add(queryArg);
searchCmd.Options.Add(knowledgeSearchJsonOpt);
searchCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    var query = result.GetValue(queryArg) ?? string.Empty;
    var entries = mediator.Send(new SearchKnowledgeQuery(query)).Result;
    if (result.GetValue(knowledgeSearchJsonOpt))
    {
        WriteJson(entries);
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
});

knowledgeCmd.Subcommands.Add(searchCmd);
rootCommand.Subcommands.Add(knowledgeCmd);

var issueCmd = new Command("issue", "Manage issue records");

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
var issueCreateJsonOpt = NewJsonOption();
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

issueCreateCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
    var json = result.GetValue(issueCreateJsonOpt);

    var systemId = ResolveSystemId(result.GetValue(issueSystemIdOpt), result.GetValue(issueSystemEaiOpt), references, json);
    var stateId = ResolveIssueStateId(result.GetValue(issueStateIdOpt), result.GetValue(issueStateCodeOpt), references, json);
    var createdByUserId = ResolveUserId(result.GetValue(issueUserIdOpt), result.GetValue(issueCreatedByEmailOpt), references, json);
    if (systemId is null || stateId is null || createdByUserId is null)
    {
        return;
    }

    var command = new CreateIssueCommand(
        result.GetValue(issueSummaryOpt)!,
        systemId.Value,
        result.GetValue(problemOpt)!,
        result.GetValue(analysisOpt) ?? string.Empty,
        result.GetValue(resolutionOpt) ?? string.Empty,
        stateId.Value,
        createdByUserId.Value,
        result.GetValue(ritmOpt),
        result.GetValue(incidentOpt));

    var issue = mediator.Send(command).Result;

    if (json)
    {
        WriteJson(ToIssueCreateResult(issue));
        return;
    }

    AnsiConsole.MarkupLine($"[green]Issue created:[/] {issue.IssueId}");
    AnsiConsole.MarkupLine($"  [bold]Summary:[/] {issue.Summary}");
    AnsiConsole.MarkupLine($"  [bold]System ID:[/] {issue.SystemId}");
    AnsiConsole.MarkupLine($"  [bold]State ID:[/] {issue.StateId}");
});

issueCmd.Subcommands.Add(issueCreateCmd);

var eaiOpt = new Option<string>("--eai")
{
    Description = "Filter by system EAI code"
};
var issueListCmd = new Command("list", "List all issues");
var issueListJsonOpt = NewJsonOption();
issueListCmd.Options.Add(eaiOpt);
issueListCmd.Options.Add(issueListJsonOpt);
issueListCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    var eai = result.GetValue(eaiOpt);
    var issues = mediator.Send(new ListIssuesQuery(eai)).Result;
    if (result.GetValue(issueListJsonOpt))
    {
        WriteJson(issues);
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
});

issueCmd.Subcommands.Add(issueListCmd);

var issueShowCmd = new Command("show", "Show issue record details");
var issueIdArg = new Argument<Guid>("id");
var issueShowJsonOpt = NewJsonOption();
issueShowCmd.Arguments.Add(issueIdArg);
issueShowCmd.Options.Add(issueShowJsonOpt);
issueShowCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    var issue = mediator.Send(new GetIssueByIdQuery(result.GetValue(issueIdArg))).Result;

    if (issue is null)
    {
        WriteError("Issue not found.", result.GetValue(issueShowJsonOpt));
        return;
    }

    if (result.GetValue(issueShowJsonOpt))
    {
        WriteJson(ToIssueDetails(issue));
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
});

issueCmd.Subcommands.Add(issueShowCmd);
rootCommand.Subcommands.Add(issueCmd);

var userCmd = new Command("user", "Manage users");
var userListCmd = new Command("list", "List users");
var userListJsonOpt = NewJsonOption();
userListCmd.Options.Add(userListJsonOpt);
userListCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
    var users = references.ListUsersAsync().Result;
    if (result.GetValue(userListJsonOpt))
    {
        WriteJson(users);
        return;
    }

    var table = new Table();
    table.AddColumns("Id", "Email", "Name");
    foreach (var user in users)
    {
        table.AddRow(user.UserId.ToString()[..8], user.Email, user.Name);
    }

    AnsiConsole.Write(table);
});
userCmd.Subcommands.Add(userListCmd);
rootCommand.Subcommands.Add(userCmd);

var systemCmd = new Command("system", "Manage systems");
var systemListCmd = new Command("list", "List systems");
var systemListJsonOpt = NewJsonOption();
systemListCmd.Options.Add(systemListJsonOpt);
systemListCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
    var systems = references.ListSystemsAsync().Result;
    if (result.GetValue(systemListJsonOpt))
    {
        WriteJson(systems);
        return;
    }

    var table = new Table();
    table.AddColumns("Id", "EAI", "Name", "Owner");
    foreach (var system in systems)
    {
        table.AddRow(system.SystemId.ToString(), system.EAI, system.Name, system.OwnerName);
    }

    AnsiConsole.Write(table);
});
systemCmd.Subcommands.Add(systemListCmd);
rootCommand.Subcommands.Add(systemCmd);

var knowledgeTypeCmd = new Command("knowledge-type", "Manage knowledge types");
var knowledgeTypeListCmd = new Command("list", "List knowledge types");
var knowledgeTypeListJsonOpt = NewJsonOption();
knowledgeTypeListCmd.Options.Add(knowledgeTypeListJsonOpt);
knowledgeTypeListCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
    var types = references.ListKnowledgeTypesAsync().Result;
    WriteReferenceList(types, result.GetValue(knowledgeTypeListJsonOpt), "Id", "Code", "Name");
});
knowledgeTypeCmd.Subcommands.Add(knowledgeTypeListCmd);
rootCommand.Subcommands.Add(knowledgeTypeCmd);

var knowledgeStateCmd = new Command("knowledge-state", "Manage knowledge states");
var knowledgeStateListCmd = new Command("list", "List knowledge states");
var knowledgeStateListJsonOpt = NewJsonOption();
knowledgeStateListCmd.Options.Add(knowledgeStateListJsonOpt);
knowledgeStateListCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
    var states = references.ListKnowledgeStatesAsync().Result;
    WriteReferenceList(states, result.GetValue(knowledgeStateListJsonOpt), "Id", "Code", "Name");
});
knowledgeStateCmd.Subcommands.Add(knowledgeStateListCmd);
rootCommand.Subcommands.Add(knowledgeStateCmd);

var issueStateCmd = new Command("issue-state", "Manage issue states");
var issueStateListCmd = new Command("list", "List issue states");
var issueStateListJsonOpt = NewJsonOption();
issueStateListCmd.Options.Add(issueStateListJsonOpt);
issueStateListCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
    var states = references.ListIssueStatesAsync().Result;
    WriteReferenceList(states, result.GetValue(issueStateListJsonOpt), "Id", "Code", "Name");
});
issueStateCmd.Subcommands.Add(issueStateListCmd);
rootCommand.Subcommands.Add(issueStateCmd);

var startupCmd = new Command("startup", "Initialize reference data (Users, Systems, KnowledgeTypes, IssueStates, KnowledgeStates)");
var startupDemoOpt = new Option<bool>("--demo")
{
    Description = "Seed idempotent demo data without prompts"
};
var startupJsonOpt = NewJsonOption();
startupCmd.Options.Add(startupDemoOpt);
startupCmd.Options.Add(startupJsonOpt);

startupCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var svc = scope.ServiceProvider.GetRequiredService<IStartupService>();
    var ct = CancellationToken.None;
    var json = result.GetValue(startupJsonOpt);

    if (result.GetValue(startupDemoOpt))
    {
        if (json)
        {
            WriteJson(svc.SeedDemoDataAsync(ct).Result);
            return;
        }

        AnsiConsole.Status()
            .Start("Seeding demo data...", _ =>
            {
                svc.SeedDemoDataAsync(ct).Wait();
            });
        AnsiConsole.MarkupLine("[bold green]Demo data initialized successfully![/]");
        return;
    }

    if (json)
    {
        WriteError("Use --json together with --demo for non-interactive startup output.", true);
        return;
    }

    AnsiConsole.Write(new FigletText("Axiom Setup").Color(Color.Yellow));
    AnsiConsole.Write(new Rule("[yellow]Reference Data Initialization[/]"));
    AnsiConsole.MarkupLine("This wizard will help you load the initial reference data.\n");

    var users = new List<User>();
    AnsiConsole.Write(new Rule("[cyan]Users[/]"));
    do
    {
        var email = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Email:[/]"));
        var name = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Name:[/]"));
        users.Add(new User(email, name));
    }
    while (AnsiConsole.Confirm("Add another user?", false));

    var systems = new List<AxiomSystem>();
    AnsiConsole.Write(new Rule("[cyan]Systems[/]"));
    do
    {
        var eai = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]EAI:[/]").Validate(v =>
            string.IsNullOrWhiteSpace(v) ? ValidationResult.Error("[red]EAI cannot be empty[/]") : ValidationResult.Success()));
        var name = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Name:[/]").Validate(v =>
            string.IsNullOrWhiteSpace(v) ? ValidationResult.Error("[red]Name cannot be empty[/]") : ValidationResult.Success()));
        var owner = AnsiConsole.Prompt(
            new SelectionPrompt<User>()
                .Title("[cyan]Owner:[/]")
                .AddChoices(users)
                .UseConverter(u => $"{u.Name} ({u.Email})"));
        systems.Add(new AxiomSystem(eai, name, owner.UserId));
    }
    while (AnsiConsole.Confirm("Add another system?", false));

    var knowledgeTypes = new List<KnowledgeType>();
    AnsiConsole.Write(new Rule("[cyan]Knowledge Types[/]"));
    do
    {
        var code = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Code:[/]").Validate(v =>
            string.IsNullOrWhiteSpace(v) ? ValidationResult.Error("[red]Code cannot be empty[/]") : ValidationResult.Success()));
        var name = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Name:[/]").Validate(v =>
            string.IsNullOrWhiteSpace(v) ? ValidationResult.Error("[red]Name cannot be empty[/]") : ValidationResult.Success()));
        knowledgeTypes.Add(new KnowledgeType(code, name));
    }
    while (AnsiConsole.Confirm("Add another type?", false));

    var issueStates = new List<IssueState>();
    AnsiConsole.Write(new Rule("[cyan]Issue States[/]"));
    do
    {
        var code = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Code:[/]").Validate(v =>
            string.IsNullOrWhiteSpace(v) ? ValidationResult.Error("[red]Code cannot be empty[/]") : ValidationResult.Success()));
        var name = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Name:[/]").Validate(v =>
            string.IsNullOrWhiteSpace(v) ? ValidationResult.Error("[red]Name cannot be empty[/]") : ValidationResult.Success()));
        issueStates.Add(new IssueState(code, name));
    }
    while (AnsiConsole.Confirm("Add another state?", false));

    var knowledgeStates = new List<KnowledgeState>();
    AnsiConsole.Write(new Rule("[cyan]Knowledge States[/]"));
    do
    {
        var code = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Code:[/]").Validate(v =>
            string.IsNullOrWhiteSpace(v) ? ValidationResult.Error("[red]Code cannot be empty[/]") : ValidationResult.Success()));
        var name = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Name:[/]").Validate(v =>
            string.IsNullOrWhiteSpace(v) ? ValidationResult.Error("[red]Name cannot be empty[/]") : ValidationResult.Success()));
        knowledgeStates.Add(new KnowledgeState(code, name));
    }
    while (AnsiConsole.Confirm("Add another state?", false));

    AnsiConsole.Write(new Rule("[yellow]Summary[/]"));
    var summaryTable = new Table();
    summaryTable.AddColumns("Entity", "Count");
    summaryTable.AddRow("Users", users.Count.ToString());
    summaryTable.AddRow("Systems", systems.Count.ToString());
    summaryTable.AddRow("Knowledge Types", knowledgeTypes.Count.ToString());
    summaryTable.AddRow("Issue States", issueStates.Count.ToString());
    summaryTable.AddRow("Knowledge States", knowledgeStates.Count.ToString());
    AnsiConsole.Write(summaryTable);

    if (!AnsiConsole.Confirm("Proceed with seeding?", true))
    {
        AnsiConsole.MarkupLine("[red]Operation cancelled.[/]");
        return;
    }

    var totalUsers = 0;
    var totalSystems = 0;
    var totalTypes = 0;
    var totalIssueStates = 0;
    var totalKnowledgeStates = 0;

    AnsiConsole.Status()
        .Start("Seeding reference data...", ctx =>
        {
            ctx.Spinner(Spinner.Known.Star);

            var persistedUserIds = new Dictionary<Guid, Guid>();

            foreach (var u in users)
            {
                var persistedUser = svc.CreateUserAsync(u.Email, u.Name, ct).Result;
                persistedUserIds[u.UserId] = persistedUser.UserId;
                totalUsers++;
                ctx.Status($"Created user: {u.Email}");
            }

            foreach (var s in systems)
            {
                var ownerUserId = persistedUserIds[s.OwnerUserId];
                svc.CreateSystemAsync(s.EAI, s.Name, ownerUserId, ct).Wait();
                totalSystems++;
                ctx.Status($"Created system: {s.Name}");
            }

            foreach (var t in knowledgeTypes)
            {
                svc.CreateKnowledgeTypeAsync(t.Code, t.Name, ct).Wait();
                totalTypes++;
                ctx.Status($"Created type: {t.Code}");
            }

            foreach (var s in issueStates)
            {
                svc.CreateIssueStateAsync(s.Code, s.Name, ct).Wait();
                totalIssueStates++;
                ctx.Status($"Created issue state: {s.Code}");
            }

            foreach (var s in knowledgeStates)
            {
                svc.CreateKnowledgeStateAsync(s.Code, s.Name, ct).Wait();
                totalKnowledgeStates++;
                ctx.Status($"Created knowledge state: {s.Code}");
            }
        });

    var resultTable = new Table();
    resultTable.AddColumns("Entity", "Created");
    resultTable.AddRow("[green]Users[/]", totalUsers.ToString());
    resultTable.AddRow("[green]Systems[/]", totalSystems.ToString());
    resultTable.AddRow("[green]Knowledge Types[/]", totalTypes.ToString());
    resultTable.AddRow("[green]Issue States[/]", totalIssueStates.ToString());
    resultTable.AddRow("[green]Knowledge States[/]", totalKnowledgeStates.ToString());
    AnsiConsole.Write(resultTable);
    AnsiConsole.MarkupLine("[bold green]Reference data initialized successfully![/]");
});

rootCommand.Subcommands.Add(startupCmd);

void WriteJson(object value)
{
    Console.WriteLine(JsonSerializer.Serialize(value, jsonOptions));
}

void WriteError(string message, bool json)
{
    Environment.ExitCode = 1;
    if (json)
    {
        WriteJson(new { error = message });
        return;
    }

    AnsiConsole.MarkupLine($"[red]{message}[/]");
}

void WriteReferenceList(IEnumerable<Axiom.Application.Dtos.ReferenceCodeDto> references, bool json, params string[] columns)
{
    if (json)
    {
        WriteJson(references);
        return;
    }

    var table = new Table();
    table.AddColumns(columns);
    foreach (var reference in references)
    {
        table.AddRow(reference.Id.ToString(), reference.Code, reference.Name);
    }

    AnsiConsole.Write(table);
}

long? ResolveSystemId(long id, string? eai, IReferenceDataService references, bool json)
{
    if (id > 0 && !string.IsNullOrWhiteSpace(eai))
    {
        WriteError("Use either --system-id or --system-eai, not both.", json);
        return null;
    }

    if (id > 0)
    {
        return id;
    }

    if (string.IsNullOrWhiteSpace(eai))
    {
        WriteError("A system reference is required. Use --system-id or --system-eai.", json);
        return null;
    }

    var system = references.FindSystemByEaiAsync(eai).Result;
    if (system is null)
    {
        WriteError($"System EAI not found: {eai}", json);
        return null;
    }

    return system.SystemId;
}

long? ResolveKnowledgeTypeId(long id, string? code, IReferenceDataService references, bool json)
{
    if (id > 0 && !string.IsNullOrWhiteSpace(code))
    {
        WriteError("Use either --type-id or --type-code, not both.", json);
        return null;
    }

    if (id > 0)
    {
        return id;
    }

    if (string.IsNullOrWhiteSpace(code))
    {
        WriteError("A knowledge type reference is required. Use --type-id or --type-code.", json);
        return null;
    }

    var type = references.FindKnowledgeTypeByCodeAsync(code).Result;
    if (type is null)
    {
        WriteError($"Knowledge type code not found: {code}", json);
        return null;
    }

    return type.Id;
}

int? ResolveKnowledgeStateId(int id, string? code, IReferenceDataService references, bool json)
{
    if (id > 0 && !string.IsNullOrWhiteSpace(code))
    {
        WriteError("Use either --state-id or --state-code, not both.", json);
        return null;
    }

    if (id > 0)
    {
        return id;
    }

    if (string.IsNullOrWhiteSpace(code))
    {
        WriteError("A knowledge state reference is required. Use --state-id or --state-code.", json);
        return null;
    }

    var state = references.FindKnowledgeStateByCodeAsync(code).Result;
    if (state is null)
    {
        WriteError($"Knowledge state code not found: {code}", json);
        return null;
    }

    return (int)state.Id;
}

int? ResolveIssueStateId(int id, string? code, IReferenceDataService references, bool json)
{
    if (id > 0 && !string.IsNullOrWhiteSpace(code))
    {
        WriteError("Use either --state-id or --state-code, not both.", json);
        return null;
    }

    if (id > 0)
    {
        return id;
    }

    if (string.IsNullOrWhiteSpace(code))
    {
        WriteError("An issue state reference is required. Use --state-id or --state-code.", json);
        return null;
    }

    var state = references.FindIssueStateByCodeAsync(code).Result;
    if (state is null)
    {
        WriteError($"Issue state code not found: {code}", json);
        return null;
    }

    return (int)state.Id;
}

Guid? ResolveUserId(Guid id, string? email, IReferenceDataService references, bool json)
{
    if (id != Guid.Empty && !string.IsNullOrWhiteSpace(email))
    {
        WriteError("Use either --created-by or --created-by-email, not both.", json);
        return null;
    }

    if (id != Guid.Empty)
    {
        return id;
    }

    if (string.IsNullOrWhiteSpace(email))
    {
        WriteError("A user reference is required. Use --created-by or --created-by-email.", json);
        return null;
    }

    var user = references.FindUserByEmailAsync(email).Result;
    if (user is null)
    {
        WriteError($"User email not found: {email}", json);
        return null;
    }

    return user.UserId;
}

object ToKnowledgeCreateResult(Knowledge entry)
{
    return new
    {
        entry.KnowledgeId,
        entry.Title,
        entry.Summary,
        entry.Content,
        entry.SystemId,
        entry.CreatedByUserId,
        entry.KnowledgeTypeId,
        entry.KnowledgeStateId,
        entry.IssueId,
        entry.VersionNumber,
        entry.CreatedAt,
        entry.UpdatedAt
    };
}

object ToKnowledgeDetails(Knowledge entry)
{
    return new
    {
        entry.KnowledgeId,
        entry.Title,
        entry.Summary,
        entry.Content,
        entry.SystemId,
        SystemName = entry.System?.Name,
        entry.CreatedByUserId,
        CreatedByName = entry.CreatedBy?.Name,
        entry.KnowledgeTypeId,
        TypeName = entry.Type?.Name,
        entry.KnowledgeStateId,
        StateName = entry.State?.Name,
        entry.IssueId,
        Tags = entry.KnowledgeKnowledgeTags
            .Select(t => t.Tag?.TagName)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList(),
        entry.VersionNumber,
        entry.CreatedAt,
        entry.UpdatedAt
    };
}

object ToIssueCreateResult(Issue issue)
{
    return new
    {
        issue.IssueId,
        issue.Summary,
        issue.SystemId,
        issue.StateId,
        issue.CreatedByUserId,
        issue.RitmNumber,
        issue.IncidentNumber,
        issue.CreatedAt,
        issue.UpdatedAt,
        issue.ResolvedAt
    };
}

object ToIssueDetails(Issue issue)
{
    return new
    {
        issue.IssueId,
        issue.Summary,
        issue.SystemId,
        SystemName = issue.System?.Name,
        issue.StateId,
        StateName = issue.State?.Name,
        issue.CreatedByUserId,
        CreatedByName = issue.CreatedBy?.Name,
        issue.RitmNumber,
        issue.IncidentNumber,
        issue.Problem,
        issue.Analysis,
        issue.Resolution,
        issue.CreatedAt,
        issue.UpdatedAt,
        issue.ResolvedAt
    };
}

var parseResult = rootCommand.Parse(args);
var exitCode = await parseResult.InvokeAsync();
return Environment.ExitCode != 0 ? Environment.ExitCode : exitCode;
