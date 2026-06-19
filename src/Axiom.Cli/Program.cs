using System.CommandLine;
using System.CommandLine.Parsing;
using Axiom.Application;
using Axiom.Application.Commands;
using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Domain.Entities;
using Axiom.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

var builder = Host.CreateApplicationBuilder(args);

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
var systemIdOpt = new Option<long>("--system-id") { Required = true };
var typeIdOpt = new Option<long>("--type-id") { Required = true };
var stateIdOpt = new Option<int>("--state-id") { Required = true };
var userIdOpt = new Option<Guid>("--created-by") { Required = true };
var tagsOpt = new Option<string>("--tags");
var issueIdOpt = new Option<Guid?>("--issue-id");
createCmd.Options.Add(titleOpt);
createCmd.Options.Add(summaryOpt);
createCmd.Options.Add(contentOpt);
createCmd.Options.Add(systemIdOpt);
createCmd.Options.Add(typeIdOpt);
createCmd.Options.Add(stateIdOpt);
createCmd.Options.Add(userIdOpt);
createCmd.Options.Add(tagsOpt);
createCmd.Options.Add(issueIdOpt);

createCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    var tags = result.GetValue(tagsOpt);
    var tagList = string.IsNullOrWhiteSpace(tags)
        ? new List<string>()
        : [.. tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)];

    var command = new CreateKnowledgeCommand(
        result.GetValue(titleOpt)!,
        result.GetValue(summaryOpt) ?? string.Empty,
        result.GetValue(contentOpt)!,
        result.GetValue(systemIdOpt),
        result.GetValue(userIdOpt),
        result.GetValue(typeIdOpt),
        result.GetValue(stateIdOpt),
        result.GetValue(issueIdOpt),
        tagList);

    var entry = mediator.Send(command).Result;

    AnsiConsole.MarkupLine($"[green]Knowledge created:[/] {entry.KnowledgeId}");
    AnsiConsole.MarkupLine($"  [bold]Title:[/] {entry.Title}");
    AnsiConsole.MarkupLine($"  [bold]System ID:[/] {entry.SystemId}");
    AnsiConsole.MarkupLine($"  [bold]Type ID:[/] {entry.KnowledgeTypeId}");
    AnsiConsole.MarkupLine($"  [bold]State ID:[/] {entry.KnowledgeStateId}");
});

knowledgeCmd.Subcommands.Add(createCmd);

var listCmd = new Command("list", "List all knowledge entries");
listCmd.SetAction((ParseResult _) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    var entries = mediator.Send(new ListKnowledgeQuery()).Result;

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
showCmd.Arguments.Add(idArg);
showCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    var entry = mediator.Send(new GetKnowledgeByIdQuery(result.GetValue(idArg))).Result;

    if (entry is null)
    {
        AnsiConsole.MarkupLine("[red]Knowledge entry not found.[/]");
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
searchCmd.Arguments.Add(queryArg);
searchCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    var query = result.GetValue(queryArg) ?? string.Empty;
    var entries = mediator.Send(new SearchKnowledgeQuery(query)).Result;

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
var issueSystemIdOpt = new Option<long>("--system-id") { Required = true };
var problemOpt = new Option<string>("--problem") { Required = true };
var analysisOpt = new Option<string>("--analysis");
var resolutionOpt = new Option<string>("--resolution");
var issueStateIdOpt = new Option<int>("--state-id") { Required = true };
var issueUserIdOpt = new Option<Guid>("--created-by") { Required = true };
var ritmOpt = new Option<string>("--ritm-number");
var incidentOpt = new Option<string>("--incident-number");
issueCreateCmd.Options.Add(issueSummaryOpt);
issueCreateCmd.Options.Add(issueSystemIdOpt);
issueCreateCmd.Options.Add(problemOpt);
issueCreateCmd.Options.Add(analysisOpt);
issueCreateCmd.Options.Add(resolutionOpt);
issueCreateCmd.Options.Add(issueStateIdOpt);
issueCreateCmd.Options.Add(issueUserIdOpt);
issueCreateCmd.Options.Add(ritmOpt);
issueCreateCmd.Options.Add(incidentOpt);

issueCreateCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    var command = new CreateIssueCommand(
        result.GetValue(issueSummaryOpt)!,
        result.GetValue(issueSystemIdOpt),
        result.GetValue(problemOpt)!,
        result.GetValue(analysisOpt) ?? string.Empty,
        result.GetValue(resolutionOpt) ?? string.Empty,
        result.GetValue(issueStateIdOpt),
        result.GetValue(issueUserIdOpt),
        result.GetValue(ritmOpt),
        result.GetValue(incidentOpt));

    var issue = mediator.Send(command).Result;

    AnsiConsole.MarkupLine($"[green]Issue created:[/] {issue.IssueId}");
    AnsiConsole.MarkupLine($"  [bold]Summary:[/] {issue.Summary}");
    AnsiConsole.MarkupLine($"  [bold]System ID:[/] {issue.SystemId}");
    AnsiConsole.MarkupLine($"  [bold]State ID:[/] {issue.StateId}");
});

issueCmd.Subcommands.Add(issueCreateCmd);

var eaiOpt = new Option<string>("--eai", "Filter by system EAI code");
var issueListCmd = new Command("list", "List all issues");
issueListCmd.Options.Add(eaiOpt);
issueListCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    var eai = result.GetValue(eaiOpt);
    var issues = mediator.Send(new ListIssuesQuery(eai)).Result;

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
issueShowCmd.Arguments.Add(issueIdArg);
issueShowCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    var issue = mediator.Send(new GetIssueByIdQuery(result.GetValue(issueIdArg))).Result;

    if (issue is null)
    {
        AnsiConsole.MarkupLine("[red]Issue not found.[/]");
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

var startupCmd = new Command("startup", "Initialize reference data (Users, Systems, KnowledgeTypes, IssueStates, KnowledgeStates)");

startupCmd.SetAction((ParseResult _) =>
{
    using var scope = host.Services.CreateScope();
    var svc = scope.ServiceProvider.GetRequiredService<IStartupService>();
    var ct = CancellationToken.None;

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

            foreach (var u in users)
            {
                svc.CreateUserAsync(u.Email, u.Name, ct).Wait();
                totalUsers++;
                ctx.Status($"Created user: {u.Email}");
            }

            foreach (var s in systems)
            {
                svc.CreateSystemAsync(s.EAI, s.Name, s.OwnerUserId, ct).Wait();
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

var parseResult = rootCommand.Parse(args);
return await parseResult.InvokeAsync();
