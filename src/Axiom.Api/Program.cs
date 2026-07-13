using Axiom.Api.ApiEndpoints;
using Axiom.Api.Middleware;
using Axiom.Application;
using Axiom.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(
        builder.Configuration.GetConnectionString("Axiom")
        ?? throw new InvalidOperationException(
            "No se encontró la cadena de conexión 'Axiom' en appsettings.json. " +
            "Agrega la sección ConnectionStrings:Axiom."));

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapOpenApi();

app.MapGroup("/api/knowledge").MapKnowledge();
app.MapGroup("/api/issues").MapIssues();
app.MapGroup("/api/users").MapUsers();
app.MapGroup("/api/systems").MapSystems();
app.MapGroup("/api/components").MapComponents();
app.MapGroup("/api/dependencies").MapDependencies();
app.MapGroup("/api/reference").MapReference();

app.Run();
