using System.CommandLine;
using Axiom.Application;
using Axiom.Cli.Commands;
using Axiom.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

var connectionString = Environment.GetEnvironmentVariable("AXIOM_CONNECTION_STRING")
    ?? "Server=localhost;Database=AXIOM;Integrated Security=True;TrustServerCertificate=True;";

builder.Services
    .AddApplication()
    .AddInfrastructure(connectionString);

var host = builder.Build();

var rootCommand = new RootCommand("Axiom - KnowledgeOps and Operational Continuity Platform");

rootCommand.Subcommands.Add(KnowledgeCommands.Create(host));
rootCommand.Subcommands.Add(IssueCommands.Create(host));
rootCommand.Subcommands.Add(UserCommands.Create(host));
rootCommand.Subcommands.Add(SystemCommands.Create(host));
rootCommand.Subcommands.Add(ReferenceCommands.Create(host));
rootCommand.Subcommands.Add(StartupCommands.Create(host));
rootCommand.Subcommands.Add(ComponentCommands.Create(host));
rootCommand.Subcommands.Add(DependencyCommands.Create(host));
rootCommand.Subcommands.Add(SyncCommands.Create(host));

var parseResult = rootCommand.Parse(args);
var exitCode = await parseResult.InvokeAsync();
return Environment.ExitCode != 0 ? Environment.ExitCode : exitCode;
